using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain;

namespace OrderManagement.Infrastructure;

public sealed class OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).HasMaxLength(200).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(customer => customer.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.Property(order => order.CreatedAtUtc).IsRequired();
            entity.Property(order => order.Subtotal).HasPrecision(18, 2);
            entity.Property(order => order.DiscountCode).HasMaxLength(40);
            entity.Property(order => order.DiscountAmount).HasPrecision(18, 2);
            entity.HasOne(order => order.Customer)
                .WithMany()
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(order => order.Total);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Ignore(item => item.LineTotal);
            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
