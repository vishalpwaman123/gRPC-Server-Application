using Microsoft.EntityFrameworkCore;

namespace ProductGrpc.Server.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");

            // Seed data so the sample has something to query on first run.
            entity.HasData(
                new Product { Id = 1, Name = "Keyboard", Description = "Mechanical keyboard", Price = 89.99m, Stock = 25 },
                new Product { Id = 2, Name = "Mouse", Description = "Wireless mouse", Price = 39.50m, Stock = 60 },
                new Product { Id = 3, Name = "Monitor", Description = "27 inch 1440p monitor", Price = 299.00m, Stock = 10 });
        });
    }
}
