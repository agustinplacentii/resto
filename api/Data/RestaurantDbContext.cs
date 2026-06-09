using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Models;

namespace Restaurant.Api.Data;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductGroup>(entity =>
        {
            entity.Property(group => group.Name).HasMaxLength(80).IsRequired();
            entity.Property(group => group.Description).HasMaxLength(240).IsRequired();
            entity.HasIndex(group => group.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Category).HasMaxLength(80).IsRequired();
            entity.Property(product => product.Measure).HasMaxLength(40).IsRequired();
            entity.Property(product => product.Price).HasColumnType("numeric(12,2)");
            entity.HasOne(product => product.ProductGroup)
                .WithMany(group => group.Products)
                .HasForeignKey(product => product.ProductGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.TableName).HasMaxLength(80);
            entity.Property(order => order.CustomerName).HasMaxLength(120);
            entity.Property(order => order.Notes).HasMaxLength(500);
            entity.Property(order => order.Total).HasColumnType("numeric(12,2)");
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(order => order.PaidAt);
            entity.HasOne(order => order.CashRegister)
                .WithMany(cashRegister => cashRegister.Orders)
                .HasForeignKey(order => order.CashRegisterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.ProductName).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Measure).HasMaxLength(40).IsRequired();
            entity.Property(item => item.UnitPrice).HasColumnType("numeric(12,2)");
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.Property(log => log.Type).HasMaxLength(40).IsRequired();
            entity.Property(log => log.Description).HasMaxLength(500).IsRequired();
            entity.HasIndex(log => log.CreatedAt);
        });

        modelBuilder.Entity<CashRegister>(entity =>
        {
            entity.Property(cashRegister => cashRegister.Total).HasColumnType("numeric(12,2)");
            entity.HasIndex(cashRegister => cashRegister.OpenedAt);
            entity.HasIndex(cashRegister => cashRegister.ClosedAt);
        });
    }
}
