using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;
using Web_Development_Coursework.Models;
using Web_Development_Coursework.Services;

namespace Web_Development_Coursework.Controllers
{
    [Authorize]
    public class RulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ScoringService _scoring;

        public RulesController(ApplicationDbContext context, ScoringService scoring)
        {
            _context = context;
            _scoring = scoring;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Rules
        public async Task<IActionResult> Index()
        {
            var rule = await _scoring.GetRuleAsync(CurrentUserId);
            return View(rule);
        }

        // POST: Rules
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            [Bind("MinDiscountPct,AllowMetal,BlockedModels,MaxListingAgeDays")] Rule input)
        {
            ModelState.Remove(nameof(Rule.UserId));
            ModelState.Remove(nameof(Rule.BlockedModels));

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var rule = await _context.Rules.FirstOrDefaultAsync(r => r.UserId == CurrentUserId);
            if (rule == null)
            {
                rule = new Rule { UserId = CurrentUserId };
                _context.Rules.Add(rule);
            }

            rule.MinDiscountPct = input.MinDiscountPct;
            rule.AllowMetal = input.AllowMetal;
            rule.BlockedModels = input.BlockedModels ?? string.Empty;
            rule.MaxListingAgeDays = input.MaxListingAgeDays;
            await _context.SaveChangesAsync();

            await _scoring.RescoreAllAsync(CurrentUserId);

            TempData["Saved"] = true;
            return RedirectToAction(nameof(Index));
        }
    }
}
