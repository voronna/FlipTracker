using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Models;

namespace Web_Development_Coursework.Controllers
{
    public record DealsIndexViewModel(
        List<Deal> Deals,
        int FoundCount,
        int BoughtCount,
        int InProgressCount,
        int SoldCount,
        decimal TotalProfit,
        decimal? AverageRoi);

    [Authorize]
    public class DealsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DealsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private IQueryable<Deal> MyDeals() =>
            _context.Deals.Where(d => d.Listing!.UserId == CurrentUserId);

        // GET: Deals
        public async Task<IActionResult> Index()
        {
            var deals = await MyDeals()
                .Include(d => d.Listing)
                .ThenInclude(l => l!.Model)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            var foundCount = await _context.Listings
                .CountAsync(l => l.UserId == CurrentUserId && l.Deal == null);

            var sold = deals.Where(d => d.Status == DealStatus.Sold).ToList();
            var totalProfit = sold.Sum(d => d.Profit ?? 0m);
            var rois = sold.Where(d => d.RoiPct.HasValue).Select(d => d.RoiPct!.Value).ToList();
            decimal? averageRoi = rois.Count > 0 ? rois.Average() : null;

            var model = new DealsIndexViewModel(
                deals,
                foundCount,
                deals.Count(d => d.Status == DealStatus.Bought),
                deals.Count(d => d.Status == DealStatus.InProgress),
                sold.Count,
                totalProfit,
                averageRoi);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int listingId)
        {
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == listingId && l.UserId == CurrentUserId);
            if (listing == null) return NotFound();

            var existing = await _context.Deals.FirstOrDefaultAsync(d => d.ListingId == listing.Id);
            if (existing != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existing.Id });
            }

            var deal = new Deal
            {
                ListingId = listing.Id,
                Status = DealStatus.Bought,
                BuyPrice = listing.Price + listing.Shipping
            };
            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = deal.Id });
        }

        // GET: Deals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var deal = await MyDeals()
                .Include(d => d.Listing)
                .ThenInclude(l => l!.Model)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (deal == null) return NotFound();

            return View(deal);
        }

        // POST: Deals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Status,BuyPrice,ExtraCosts,SellPrice,SoldAt")] Deal input)
        {
            if (id != input.Id) return NotFound();

            var deal = await MyDeals()
                .Include(d => d.Listing)
                .ThenInclude(l => l!.Model)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (deal == null) return NotFound();

            if (input.Status == DealStatus.Sold && input.SellPrice == null)
            {
                ModelState.AddModelError(nameof(Deal.SellPrice), "Enter the sell price for a sold deal.");
            }

            if (!ModelState.IsValid)
            {
                input.Listing = deal.Listing;
                return View(input);
            }

            deal.Status = input.Status;
            deal.BuyPrice = input.BuyPrice;
            deal.ExtraCosts = input.ExtraCosts;
            deal.SellPrice = input.SellPrice;
            deal.SoldAt = input.Status == DealStatus.Sold
                ? (input.SoldAt ?? DateTime.Today)
                : null;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Deals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deal = await MyDeals().FirstOrDefaultAsync(d => d.Id == id);
            if (deal != null)
            {
                _context.Deals.Remove(deal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
