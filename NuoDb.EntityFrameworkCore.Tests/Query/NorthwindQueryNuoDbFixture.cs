﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Query
{
    public class NorthwindQueryNuoDbFixture<TModelCustomizer>: NorthwindQueryRelationalFixture<TModelCustomizer> where TModelCustomizer : IModelCustomizer, new()
    {
        protected override ITestStoreFactory TestStoreFactory
            => NuoDbNorthwindTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerID)
                .HasColumnType("nchar(5)");

            modelBuilder.Entity<Employee>(
                b =>
                {
                    b.Property(c => c.EmployeeID).HasColumnType("integer");
                    b.Property(c => c.ReportsTo).HasColumnType("integer");
                });

            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.EmployeeID).HasColumnType("integer");
                    b.Property(o => o.OrderDate).HasColumnType("datetime");
                });

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Product>(
                b =>
                {
                    b.Property(p => p.UnitPrice).HasColumnType("money");
                    b.Property(p => p.UnitsInStock).HasColumnType("smallint");
                });

            modelBuilder.Entity<MostExpensiveProduct>()
                .Property(p => p.UnitPrice)
                .HasColumnType("money");
        }
    }
}
