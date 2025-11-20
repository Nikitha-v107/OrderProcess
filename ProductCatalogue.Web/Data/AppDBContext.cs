using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Web.Models;
using System;

namespace ProductCatalogue.Web.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
           : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // Seed some sample products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Price = 85000, Stock = 10 },
                 new Product { Id = 2, Name = "Wireless Mouse", Price = 1500, Stock = 50 },
                new Product { Id = 3, Name = "Keyboard", Price = 2000, Stock = 40 },
                new Product { Id = 4, Name = "Monitor", Price = 12000, Stock = 20 },
                new Product { Id = 5, Name = "USB-C Cable", Price = 500, Stock = 100 }
                  );

        }
    }
}