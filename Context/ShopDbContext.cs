using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Models;

namespace ShopWebMVC.Context
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Debt> Debts { get; set; }
        public DbSet<DebtItem> DebtItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Bakery> Bakeries { get; set; }
        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many-to-Many relationship between Invoice and Item
            modelBuilder.Entity<InvoiceItem>()
                .HasKey(ii => new { ii.InvoiceId, ii.ItemId });

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Item)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.ItemId);

            // Many-to-Many relationship between Debt and Item
            modelBuilder.Entity<DebtItem>()
                .HasKey(di => new { di.DebtId, di.ItemId });

            modelBuilder.Entity<DebtItem>()
                .HasOne(di => di.Debt)
                .WithMany(d => d.DebtItems)
                .HasForeignKey(di => di.DebtId);

            modelBuilder.Entity<DebtItem>()
                .HasOne(di => di.Item)
                .WithMany(i => i.DebtItems)
                .HasForeignKey(di => di.ItemId);

            // One-to-Many relationship between Card and Invoice (one invoice per month)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Card)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CardId);

            // One-to-One relationship between Card and Debt
            modelBuilder.Entity<Card>()
                .HasOne(c => c.Debt)
                .WithOne(d => d.Card)
                .HasForeignKey<Debt>(d => d.CardId);

            // One-to-Many relationship between Bakery and Card
            modelBuilder.Entity<Card>()
                .HasOne(c => c.Bakery)
                .WithMany(b => b.Cards)
                .HasForeignKey(c => c.BakeryId);

           
        }
    }
}
