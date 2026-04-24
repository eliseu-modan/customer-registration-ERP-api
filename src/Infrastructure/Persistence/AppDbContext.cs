using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Username).IsUnique();
            entity.Property(user => user.Name).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Username).HasMaxLength(60).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(customer => customer.Name).HasMaxLength(120).IsRequired();
            entity.Property(customer => customer.Document).HasMaxLength(20).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(120);
            entity.Property(customer => customer.Phone).HasMaxLength(20);
            entity.Property(customer => customer.Cep).HasMaxLength(8);
            entity.Property(customer => customer.Street).HasMaxLength(150);
            entity.Property(customer => customer.Number).HasMaxLength(20);
            entity.Property(customer => customer.Neighborhood).HasMaxLength(80);
            entity.Property(customer => customer.City).HasMaxLength(80);
            entity.Property(customer => customer.State).HasMaxLength(2);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.Sku).IsUnique();
            entity.Property(product => product.Name).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Sku).HasMaxLength(30).IsRequired();
            entity.Property(product => product.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(item => item.TotalPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId);
            entity.HasOne(item => item.Product)
                .WithMany(product => product.OrderItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
