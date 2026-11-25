using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Models;

namespace Web_Development_Coursework.Services;

public record MarketRefreshResult(bool Updated, string Message);

public class MarketPriceService
{
    public const string SourceName = "ebay.de";
    public const int MinSamples = 5;
    private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);
    private const decimal CheapestFloorShare = 0.3m;
    private const int MaxOffers = 10;

    private readonly ApplicationDbContext _context;
    private readonly EbayClient _ebay;
    private readonly ILogger<MarketPriceService> _logger;

    public MarketPriceService(ApplicationDbContext context, EbayClient ebay, ILogger<MarketPriceService> logger)
    {
        _context = context;
        _ebay = ebay;
        _logger = logger;
    }

    public static decimal Median(IReadOnlyList<decimal> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted[mid]
            : (sorted[mid - 1] + sorted[mid]) / 2m;
    }

    public async Task<MarketRefreshResult> RefreshAsync(int modelId, CancellationToken ct = default)
    {
        var model = await _context.MiniatureModels.FirstOrDefaultAsync(m => m.Id == modelId, ct);
        if (model == null) return new MarketRefreshResult(false, "Model not found.");

        var latest = await _context.MarketPrices
            .Where(p => p.ModelId == modelId && p.Source == SourceName)
            .OrderByDescending(p => p.FetchedAt)
            .FirstOrDefaultAsync(ct);

        if (latest != null
            && DateTime.UtcNow - latest.FetchedAt < MaxAge
            && await _context.MarketOffers.AnyAsync(o => o.MarketPriceId == latest.Id, ct))
        {
            return new MarketRefreshResult(false,
                $"Market price is already up to date (checked {latest.FetchedAt:yyyy-MM-dd HH:mm} UTC).");
        }

        if (!_ebay.IsConfigured)
        {
            return new MarketRefreshResult(false, "eBay keys are not configured. Retail price (RRP) is used instead.");
        }

        var listings = await _ebay.SearchUsedListingsAsync($"Warhammer {model.Name}", ct);
        if (listings == null)
        {
            return new MarketRefreshResult(false,
                "eBay is not available right now. The previous price or the retail price (RRP) is used.");
        }

        if (listings.Count < MinSamples)
        {
            return new MarketRefreshResult(false,
                $"Only {listings.Count} used listings found (at least {MinSamples} needed). Retail price (RRP) is used instead.");
        }

        var median = Math.Round(Median(listings.Select(l => l.Price).ToList()), 2);

        var offers = listings
            .Where(l => l.Url != null && l.Price >= median * CheapestFloorShare)
            .OrderBy(l => l.Total)
            .Take(MaxOffers)
            .Select(l => new MarketOffer
            {
                Title = l.Title.Length > 200 ? l.Title[..200] : l.Title,
                Price = l.Price,
                Shipping = l.Shipping,
                Url = l.Url!
            })
            .ToList();
        var cheapest = offers.FirstOrDefault();

        var oldOffers = await _context.MarketOffers
            .Where(o => o.MarketPrice!.ModelId == modelId)
            .ToListAsync(ct);
        _context.MarketOffers.RemoveRange(oldOffers);

        _context.MarketPrices.Add(new MarketPrice
        {
            ModelId = modelId,
            Source = SourceName,
            MedianPrice = median,
            SampleSize = listings.Count,
            FetchedAt = DateTime.UtcNow,
            CheapestPrice = cheapest == null ? null : cheapest.Price + cheapest.Shipping,
            CheapestUrl = cheapest?.Url,
            CheapestTitle = cheapest?.Title,
            Offers = offers
        });
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Market price for model {ModelId} updated: {Median} from {Count} listings",
            modelId, median, listings.Count);

        return new MarketRefreshResult(true,
            $"Market price updated: median {median:0.00} EUR from {listings.Count} used listings on eBay.de.");
    }
}