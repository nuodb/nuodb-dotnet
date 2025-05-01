using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleEFCoreTest.Entities.Commerce;
using Microsoft.EntityFrameworkCore;

namespace ConsoleEFCoreTest.Contexts
{
    public class CommerceContext: DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNuoDb("Server=localhost;Database=demo;User=dba;Password=dba;schema=USER");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Order>()
                .HasOne<User>(x => x.User)
                .WithMany(x => x.Orders);

            builder.Entity<Order>()
                .HasMany<OrderItem>(x => x.OrderItems)
                .WithOne(x=>x.Order);

            builder.Entity<OrderItem>().HasOne<Product>(x => x.Product);
        }
    }
}
