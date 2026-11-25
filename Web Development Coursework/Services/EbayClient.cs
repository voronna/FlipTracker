using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Web_Development_Coursework.Services;

public class EbayOptions
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Marketplace { get; set; } = "EBAY_DE";
    public bool UseSandbox { get; set; }
}

public record EbayListing(string Title, decimal Price, decimal Shipping, string? Url)
{
    public decimal Total => Price + Shipping;
}

public class EbayClient
{
    private const string TokenCacheKey = "ebay-app-token";

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly EbayOptions _options;
    private readonly ILogger<EbayClient> _logger;

    public EbayClient(HttpClient http, IMemoryCache cache, IOptions<EbayOptions> options, ILogger<EbayClient> logger)
    {
        _http = http;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ClientId) && !string.IsNullOrWhiteSpace(_options.ClientSecret);

    private string BaseUrl => _options.UseSandbox ? "https://api.sandbox.ebay.com" : "https://api.ebay.com";

    // OAuth client credentials;
    private async Task<string?> GetTokenAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/identity/v1/oauth2/token");
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["scope"] = "https://api.ebay.com/oauth/api_scope"
        });

        using var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("eBay token request failed with status {Status}", (int)response.StatusCode);
            return null;
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var token = doc.RootElement.GetProperty("access_token").GetString();
        var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
        if (string.IsNullOrEmpty(token)) return null;

        _cache.Set(TokenCacheKey, token, TimeSpan.FromSeconds(Math.Max(60, expiresIn - 60)));
        return token;
    }

    private static string? SafeEbayUrl(string? raw)
    {
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme != Uri.UriSchemeHttps) return null;

        var host = uri.Host.ToLowerInvariant();
        var isEbay = host == "ebay.de" || host == "ebay.com"
                     || host.EndsWith(".ebay.de") || host.EndsWith(".ebay.com");
        return isEbay ? uri.ToString() : null;
    }

    private static decimal? ReadMoney(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var money)) return null;

        var currency = money.TryGetProperty("currency", out var c) ? c.GetString() : null;
        var value = money.TryGetProperty("value", out var v) ? v.GetString() : null;

        return currency == "EUR"
               && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
               && amount >= 0
            ? amount
            : null;
    }

    public async Task<List<EbayListing>?> SearchUsedListingsAsync(string query, CancellationToken ct = default)
    {
        if (!IsConfigured) return null;

        try
        {
            var token = await GetTokenAsync(ct);
            if (token == null) return null;

            var filter = Uri.EscapeDataString("conditions:{USED}");
            var url = $"{BaseUrl}/buy/browse/v1/item_summary/search?q={Uri.EscapeDataString(query)}&limit=50&filter={filter}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-EBAY-C-MARKETPLACE-ID", _options.Marketplace);

            using var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("eBay search failed with status {Status}", (int)response.StatusCode);
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var listings = new List<EbayListing>();

            if (doc.RootElement.TryGetProperty("itemSummaries", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var price = ReadMoney(item, "price");
                    if (price == null || price <= 0) continue;

                    decimal shipping = 0m;
                    if (item.TryGetProperty("shippingOptions", out var options)
                        && options.ValueKind == JsonValueKind.Array
                        && options.GetArrayLength() > 0)
                    {
                        shipping = ReadMoney(options[0], "shippingCost") ?? 0m;
                    }

                    var title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                    var link = item.TryGetProperty("itemWebUrl", out var u) ? SafeEbayUrl(u.GetString()) : null;

                    listings.Add(new EbayListing(title, price.Value, shipping, link));
                }
            }

            return listings;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "eBay request failed");
            return null;
        }
    }
}