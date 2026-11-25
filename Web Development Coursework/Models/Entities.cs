using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Web_Development_Coursework.Models;

public enum Material { Plastic, Metal, Resin }

public enum Verdict { NotScored, Skip, Think, Buy }

public enum DealStatus { Found, Bought, InProgress, Sold }

public class MiniatureModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    [Display(Name = "Model name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Faction { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    [Display(Name = "Retail price (RRP), EUR")]
    public decimal RrpPrice { get; set; }

    [Required, StringLength(140)]
    [Display(Name = "URL name (slug)")]
    public string Slug { get; set; } = string.Empty;

    public List<Listing> Listings { get; set; } = new();
    public List<MarketPrice> MarketPrices { get; set; } = new();
}

public class Listing
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }

    public int ModelId { get; set; }
    public MiniatureModel? Model { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Listing title")]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    [Display(Name = "Price, EUR")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    [Display(Name = "Shipping, EUR")]
    public decimal Shipping { get; set; }

    public Material Material { get; set; }

    [Display(Name = "Complete set")]
    public bool IsComplete { get; set; } = true;

    [Display(Name = "Already painted")]
    public bool IsPainted { get; set; }

    [Display(Name = "Loose bits only")]
    public bool IsBitsOnly { get; set; }

    [Display(Name = "Posted on")]
    public DateTime PostedAt { get; set; }

    [Url, StringLength(500)]
    [Display(Name = "Link to the ad")]
    public string? SourceUrl { get; set; }

    [Display(Name = "Deal score")]
    public int? DealScore { get; set; }

    public Verdict Verdict { get; set; }

    [StringLength(400)]
    [Display(Name = "Why this verdict")]
    public string? ScoreNote { get; set; }

    public Deal? Deal { get; set; }
}

public class MarketPrice
{
    public int Id { get; set; }

    public int ModelId { get; set; }
    public MiniatureModel? Model { get; set; }

    [Required, StringLength(40)]
    public string Source { get; set; } = "ebay.de";

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Median price, EUR")]
    public decimal MedianPrice { get; set; }

    [Display(Name = "Listings sampled")]
    public int SampleSize { get; set; }

    [Display(Name = "Fetched at")]
    public DateTime FetchedAt { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Cheapest listing, EUR")]
    public decimal? CheapestPrice { get; set; }

    [StringLength(500)]
    public string? CheapestUrl { get; set; }

    [StringLength(200)]
    public string? CheapestTitle { get; set; }

    public List<MarketOffer> Offers { get; set; } = new();
}

public class Deal
{
    public int Id { get; set; }

    public int ListingId { get; set; }
    public Listing? Listing { get; set; }

    public DealStatus Status { get; set; } = DealStatus.Found;

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    [Display(Name = "Buy price, EUR")]
    public decimal BuyPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    [Display(Name = "Extra costs, EUR")]
    public decimal ExtraCosts { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000)]
    [Display(Name = "Sell price, EUR")]
    public decimal? SellPrice { get; set; }

    [Display(Name = "Sold on")]
    public DateTime? SoldAt { get; set; }

    [NotMapped]
    public decimal? Profit => SellPrice - BuyPrice - ExtraCosts;

    [NotMapped]
    [Display(Name = "ROI, %")]
    public decimal? RoiPct =>
        Profit.HasValue && (BuyPrice + ExtraCosts) > 0
            ? Profit.Value / (BuyPrice + ExtraCosts) * 100
            : null;
}

public class Rule
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }

    [Range(0, 100)]
    [Display(Name = "Minimum discount, %")]
    public int MinDiscountPct { get; set; } = 20;

    [Display(Name = "Allow metal models")]
    public bool AllowMetal { get; set; }

    [StringLength(500)]
    [Display(Name = "Blocked models (comma-separated)")]
    public string BlockedModels { get; set; } = "Termagant";

    [Range(1, 365)]
    [Display(Name = "Max listing age, days")]
    public int MaxListingAgeDays { get; set; } = 30;
}

public class MarketOffer
{
    public int Id { get; set; }

    public int MarketPriceId { get; set; }
    public MarketPrice? MarketPrice { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Shipping { get; set; }

    [Required, StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [NotMapped]
    public decimal Total => Price + Shipping;
}