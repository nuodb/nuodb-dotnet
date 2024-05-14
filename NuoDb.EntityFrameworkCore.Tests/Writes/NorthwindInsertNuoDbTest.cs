using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.EntityFrameworkCore.Tests.Query;

namespace NuoDb.EntityFrameworkCore.Tests.Writes
{
    public class NorthwindInsertNuoDbTest: IClassFixture<NorthwindQueryNuoDbFixture<NoopModelCustomizer>>
    {
        private readonly NorthwindQueryNuoDbFixture<NoopModelCustomizer> _fixture;
        public NorthwindInsertNuoDbTest(NorthwindQueryNuoDbFixture<NoopModelCustomizer> fixture)
        {
            _fixture = fixture;
        }

        [ConditionalFact]
        public void Can_Insert()
        {
            using (var ctx = _fixture.CreateContext())
            {
                var p = new Product
                {
                    ProductName = "Heinz Ketchup",
                    Discontinued = false,
                    QuantityPerUnit = "2",
                    ReorderLevel = 5,
                    UnitPrice = 5.99m,
                    UnitsInStock = 10,
                    UnitsOnOrder = 10,
                    CategoryID = 2
                };
                ctx.Set<Product>().Add(p);
                ctx.SaveChanges();
            }
        }
    }
}
