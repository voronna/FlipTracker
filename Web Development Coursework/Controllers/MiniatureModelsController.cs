
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Models;
using Web_Development_Coursework.Data;
using Microsoft.AspNetCore.Authorization;


[Authorize]
public class MiniatureModelsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MiniatureModelsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MINIATUREMODELS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.MiniatureModels.ToListAsync());
    }

    // GET: MINIATUREMODELS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var miniaturemodel = await _context.MiniatureModels
            .FirstOrDefaultAsync(m => m.Id == id);
        if (miniaturemodel == null)
        {
            return NotFound();
        }

        return View(miniaturemodel);
    }

    // GET: MINIATUREMODELS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MINIATUREMODELS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Faction,RrpPrice,Slug,Listings,MarketPrices")] MiniatureModel miniaturemodel)
    {
        if (ModelState.IsValid)
        {
            _context.Add(miniaturemodel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(miniaturemodel);
    }

    // GET: MINIATUREMODELS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var miniaturemodel = await _context.MiniatureModels.FindAsync(id);
        if (miniaturemodel == null)
        {
            return NotFound();
        }
        return View(miniaturemodel);
    }

    // POST: MINIATUREMODELS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Faction,RrpPrice,Slug,Listings,MarketPrices")] MiniatureModel miniaturemodel)
    {
        if (id != miniaturemodel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(miniaturemodel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MiniatureModelExists(miniaturemodel.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(miniaturemodel);
    }

    // GET: MINIATUREMODELS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var miniaturemodel = await _context.MiniatureModels
            .FirstOrDefaultAsync(m => m.Id == id);
        if (miniaturemodel == null)
        {
            return NotFound();
        }

        return View(miniaturemodel);
    }

    // POST: MINIATUREMODELS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var miniaturemodel = await _context.MiniatureModels.FindAsync(id);
        if (miniaturemodel != null)
        {
            _context.MiniatureModels.Remove(miniaturemodel);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MiniatureModelExists(int? id)
    {
        return _context.MiniatureModels.Any(e => e.Id == id);
    }
}
