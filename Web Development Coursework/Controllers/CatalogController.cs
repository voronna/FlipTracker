using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Models;

namespace Web_Development_Coursework.Controllers
{
    public record CatalogDetailsViewModel(MiniatureModel Item, MarketPrice? Market);
    public record CatalogListItem(MiniatureModel Item, MarketPrice? Market);

    [AllowAnonymous]
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /models
        [HttpGet("models")]
        public async Task<IActionResult> Index()
        {
            var models = await _context.MiniatureModels
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();

            var latestByModel = (await _context.MarketPrices
                    .AsNoTracking()
                    .Where(p => p.Source == "ebay.de")
                    .OrderByDescending(p => p.FetchedAt)
                    .ToListAsync())
                .GroupBy(p => p.ModelId)
                .ToDictionary(g => g.Key, g => g.First());

            var items = models
                .Select(m => new CatalogListItem(m, latestByModel.GetValueOrDefault(m.Id)))
                .ToList();

            return View(items);
        }

        // GET: /models/necron-warriors
        [HttpGet("models/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var item = await _context.MiniatureModels
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Slug == slug);
            if (item == null) return NotFound();

            var market = await _context.MarketPrices
                .AsNoTracking()
                .Include(p => p.Offers)
                .Where(m => m.ModelId == item.Id)
                .OrderByDescending(m => m.FetchedAt)
                .FirstOrDefaultAsync();

            return View(new CatalogDetailsViewModel(item, market));
        }
    }
}
