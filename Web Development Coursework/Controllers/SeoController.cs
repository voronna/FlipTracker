using System.Security;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Data;

namespace Web_Development_Coursework.Controllers
{
    // robots.txt и sitemap.xml
    [AllowAnonymous]
    public class SeoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeoController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string BaseUrl => $"{Request.Scheme}://{Request.Host}";

        // GET: /robots.txt
        [HttpGet("robots.txt")]
        public IActionResult Robots()
        {
            var text =
                "User-agent: *\n" +
                "Disallow: /Listings\n" +
                "Disallow: /Rules\n" +
                "Disallow: /Deals\n" +
                "Disallow: /MiniatureModels\n" +
                "Disallow: /MiniatureModels\n" +
                "Disallow: /Identity\n" +
                "\n" +
                $"Sitemap: {BaseUrl}/sitemap.xml\n";
            return Content(text, "text/plain", Encoding.UTF8);
        }

        // GET: /sitemap.xml
        [HttpGet("sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            var slugs = await _context.MiniatureModels
                .AsNoTracking()
                .OrderBy(m => m.Slug)
                .Select(m => m.Slug)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            void Add(string path) =>
                sb.AppendLine($"  <url><loc>{SecurityElement.Escape(BaseUrl + path)}</loc></url>");

            Add("/");
            Add("/models");
            foreach (var slug in slugs)
            {
                Add("/models/" + Uri.EscapeDataString(slug));
            }

            sb.AppendLine("</urlset>");
            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}
