using Archivr.UI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Archivr.UI.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder
                .Entity<ArchiveItem>()
                .Property(e => e.ItemType)
                .HasConversion(
                    v => v.ToString(),
                    v => (ArchiveItemType)Enum.Parse(typeof(ArchiveItemType), v));
        }

        public DbSet<ArchiveItem> ArchiveItems { get; set; }
    }
}