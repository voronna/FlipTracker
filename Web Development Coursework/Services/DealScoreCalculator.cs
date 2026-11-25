using System.Globalization;
using Web_Development_Coursework.Models;

namespace Web_Development_Coursework.Services
{
    public record DealScoreResult(int Score, Verdict Verdict, string Note);

    public class DealScoreCalculator
    {
        public const int BuyThreshold = 50;
        public const int ThinkThreshold = 25;
        public const int PaintedPenalty = 10;
        public const int IncompletePenalty = 15;
        public const int StalePenalty = 20;
        public const int MinMarketSample = 5;
        public const decimal BitsMaxShareOfPrice = 0.10m;

        public DealScoreResult Calculate(
            Listing listing,
            MiniatureModel model,
            Rule rule,
            decimal? marketMedian = null,
            int marketSampleSize = 0,
            DateTime? today = null)
        {
            var now = (today ?? DateTime.Today).Date;
            var total = listing.Price + listing.Shipping;

            bool useMarket = marketMedian.HasValue
                             && marketMedian.Value > 0
                             && marketSampleSize >= MinMarketSample;
            decimal reference = useMarket ? marketMedian!.Value : model.RrpPrice;
            string referenceName = useMarket ? "eBay median" : "RRP";

            if (reference <= 0)
                return new DealScoreResult(0, Verdict.NotScored,
                    "No reference price: set the RRP for this model.");

            if (listing.Material == Material.Metal && !rule.AllowMetal)
                return Skip("Metal model, excluded by your rules.");

            if (IsBlocked(model.Name, rule.BlockedModels))
                return Skip("Model is on your block list.");

            if (listing.IsBitsOnly && total > reference * BitsMaxShareOfPrice)
                return Skip("Loose bits are only worth it when almost free.");

            decimal discount = (reference - total) / reference;
            string discountText = (discount * 100).ToString("0", CultureInfo.InvariantCulture);

            if (discount * 100 < rule.MinDiscountPct)
                return Skip($"Discount {discountText}% vs {referenceName} is below your {rule.MinDiscountPct}% minimum.");

            int score = (int)Math.Min(100m, Math.Round(discount * 200m));
            var notes = new List<string> { $"{discountText}% below {referenceName}" };

            if (listing.IsPainted)
            {
                score -= PaintedPenalty;
                notes.Add($"painted (-{PaintedPenalty})");
            }

            if (!listing.IsComplete)
            {
                score -= IncompletePenalty;
                notes.Add($"incomplete (-{IncompletePenalty})");
            }

            if ((now - listing.PostedAt.Date).TotalDays > rule.MaxListingAgeDays)
            {
                score -= StalePenalty;
                notes.Add($"listed over {rule.MaxListingAgeDays} days ago (-{StalePenalty})");
            }

            score = Math.Max(0, score);

            var verdict = score >= BuyThreshold ? Verdict.Buy
                        : score >= ThinkThreshold ? Verdict.Think
                        : Verdict.Skip;

            return new DealScoreResult(score, verdict, string.Join("; ", notes) + ".");
        }

        private static DealScoreResult Skip(string note) =>
            new DealScoreResult(0, Verdict.Skip, note);

        private static bool IsBlocked(string modelName, string blockedModels) =>
            blockedModels
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(b => modelName.Contains(b, StringComparison.OrdinalIgnoreCase));
    }
}
