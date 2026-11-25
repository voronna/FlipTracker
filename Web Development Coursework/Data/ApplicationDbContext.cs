using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web_Development_Coursework.Models;

namespace Web_Development_Coursework.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MiniatureModel> MiniatureModels => Set<MiniatureModel>();
        public DbSet<Listing> Listings => Set<Listing>();
        public DbSet<MarketPrice> MarketPrices => Set<MarketPrice>();
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<Rule> Rules => Set<Rule>();
        public DbSet<MarketOffer> MarketOffers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<MiniatureModel>().HasIndex(m => m.Slug).IsUnique();
            builder.Entity<Deal>().HasIndex(d => d.ListingId).IsUnique();
            builder.Entity<Rule>().HasIndex(r => r.UserId).IsUnique();
        }
    }
}