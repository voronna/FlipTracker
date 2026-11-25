using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Models;
using Web_Development_Coursework.Services;

namespace Web_Development_Coursework.Controllers
{
    [Authorize]
    public class ListingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ScoringService _scoring;

        public ListingsController(ApplicationDbContext context, ScoringService scoring)
        {
            _context = context;
            _scoring = scoring;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private IQueryable<Listing> MyListings() =>
            _context.Listings.Where(l => l.UserId == CurrentUserId);

        private void FillModelsList(int? selectedId = null)
        {
            ViewData["ModelId"] = new SelectList(
                _context.MiniatureModels.OrderBy(m => m.Name), "Id", "Name", selectedId);
        }

        // GET: Listings
        public async Task<IActionResult> Index()
        {
            var items = await MyListings()
            .Include(l => l.Model)
            .Include(l => l.Deal)
            .OrderByDescending(l => l.PostedAt)
            .ToListAsync();
            return View(items);
        }

        // GET: Listings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var listing = await MyListings()
                .Include(l => l.Model)
                .Include(l => l.Deal)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();

            return View(listing);
        }

        // GET: Listings/Create
        public async Task<IActionResult> Create(int? offerId)
        {
            var listing = new Listing { PostedAt = DateTime.Today };

            if (offerId != null)
            {
                var offer = await _context.MarketOffers
                    .AsNoTracking()
                    .Include(o => o.MarketPrice)
                    .FirstOrDefaultAsync(o => o.Id == offerId);

                if (offer?.MarketPrice != null)
                {
                    listing.ModelId = offer.MarketPrice.ModelId;
                    listing.Title = offer.Title;
                    listing.Price = offer.Price;
                    listing.Shipping = offer.Shipping;
                    listing.SourceUrl = offer.Url;
                }
            }

            FillModelsList();
            return View(listing);
        }

        // POST: Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ModelId,Title,Price,Shipping,Material,IsComplete,IsPainted,IsBitsOnly,PostedAt,SourceUrl")] Listing listing)
        {
            listing.UserId = CurrentUserId;
            ModelState.Remove(nameof(Listing.UserId));

            if (!ModelState.IsValid)
            {
                FillModelsList(listing.ModelId);
                return View(listing);
            }

            await _scoring.ScoreAsync(listing, CurrentUserId);

            _context.Add(listing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Listings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var listing = await MyListings().FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();

            FillModelsList(listing.ModelId);
            return View(listing);
        }

        // POST: Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,ModelId,Title,Price,Shipping,Material,IsComplete,IsPainted,IsBitsOnly,PostedAt,SourceUrl")] Listing input)
        {
            if (id != input.Id) return NotFound();

            var listing = await MyListings().FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();

            ModelState.Remove(nameof(Listing.UserId));
            if (!ModelState.IsValid)
            {
                FillModelsList(input.ModelId);
                return View(input);
            }

            listing.ModelId = input.ModelId;
            listing.Title = input.Title;
            listing.Price = input.Price;
            listing.Shipping = input.Shipping;
            listing.Material = input.Material;
            listing.IsComplete = input.IsComplete;
            listing.IsPainted = input.IsPainted;
            listing.IsBitsOnly = input.IsBitsOnly;
            listing.PostedAt = input.PostedAt;
            listing.SourceUrl = input.SourceUrl;

            await _scoring.ScoreAsync(listing, CurrentUserId);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Listings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var listing = await MyListings()
                .Include(l => l.Model)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null) return NotFound();

            return View(listing);
        }

        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await MyListings().FirstOrDefaultAsync(l => l.Id == id);
            if (listing != null)
            {
                _context.Listings.Remove(listing);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}