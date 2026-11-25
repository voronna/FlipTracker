using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Models;

namespace Web_Development_Coursework.Services
{
    public class ScoringService
    {
        private readonly ApplicationDbContext _context;
        private readonly DealScoreCalculator _calculator = new DealScoreCalculator();

        public ScoringService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rule> GetRuleAsync(string userId) =>
            await _context.Rules.FirstOrDefaultAsync(r => r.UserId == userId)
            ?? new Rule { UserId = userId };

        public async Task ScoreAsync(Listing listing, string userId)
        {
            var model = await _context.MiniatureModels.FindAsync(listing.ModelId);
            if (model == null) return;

            var rule = await GetRuleAsync(userId);
            var market = await GetLatestMarketPriceAsync(listing.ModelId);
            Apply(listing, model, rule, market);
        }

        public async Task RescoreAllAsync(string userId)
        {
            var rule = await GetRuleAsync(userId);
            var listings = await _context.Listings
                .Where(l => l.UserId == userId)
                .Include(l => l.Model)
                .ToListAsync();

            foreach (var listing in listings)
            {
                if (listing.Model == null) continue;
                var market = await GetLatestMarketPriceAsync(listing.ModelId);
                Apply(listing, listing.Model, rule, market);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<MarketPrice?> GetLatestMarketPriceAsync(int modelId) =>
            await _context.MarketPrices
                .Where(m => m.ModelId == modelId)
                .OrderByDescending(m => m.FetchedAt)
                .FirstOrDefaultAsync();

        private void Apply(Listing listing, MiniatureModel model, Rule rule, MarketPrice? market)
        {
            var result = _calculator.Calculate(
                listing, model, rule, market?.MedianPrice, market?.SampleSize ?? 0);

            listing.DealScore = result.Score;
            listing.Verdict = result.Verdict;
            listing.ScoreNote = result.Note;
        }
    }
}
