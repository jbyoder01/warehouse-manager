using Microsoft.EntityFrameworkCore;

namespace Domain;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().HasIndex(i => i.SKU).IsUnique();
        modelBuilder.Entity<Item>().HasIndex(i => i.Barcode);

        modelBuilder.Entity<Location>().HasIndex(l => l.Name).IsUnique();

        modelBuilder.Entity<Transaction>().Property(t => t.Type).HasConversion<string>();
        modelBuilder.Entity<Transaction>().HasIndex(t => t.MoveId);
    }
}