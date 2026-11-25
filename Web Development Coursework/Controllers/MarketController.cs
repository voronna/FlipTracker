using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Services;

namespace Web_Development_Coursework.Controllers
{
    // Market price updates are available only to logged-in users, to prevent anonymous users from consuming eBay API rate limits.
    [Authorize]
    public class MarketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MarketPriceService _market;
        private readonly ScoringService _scoring;

        public MarketController(ApplicationDbContext context, MarketPriceService market, ScoringService scoring)
        {
            _context = context;
            _market = market;
            _scoring = scoring;
        }

        // POST: Market/Refresh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refresh(int modelId)
        {
            var model = await _context.MiniatureModels.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == modelId);
            if (model == null) return NotFound();

            var result = await _market.RefreshAsync(modelId);
            if (result.Updated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                await _scoring.RescoreAllAsync(userId);
            }

            TempData["MarketMessage"] = result.Message;
            return Redirect("/models/" + Uri.EscapeDataString(model.Slug));
        }
    }
}
