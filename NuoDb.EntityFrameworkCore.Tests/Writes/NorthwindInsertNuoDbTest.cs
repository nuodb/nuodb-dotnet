using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NuoDb.Data.Client;
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

        [ConditionalFact(Skip = "Skip")]
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

                var fetchedProduct = ctx.Products.FirstOrDefault(x => x.ProductID == p.ProductID);
                Assert.NotNull(fetchedProduct);
                Assert.Equivalent(p, fetchedProduct);
            }
        }

        [ConditionalFact(Skip = "Skip")]
        public void Can_Update()
        {
            using (var ctx = _fixture.CreateContext())
            {
                var p = new Product
                {
                    ProductName = "Heinz Mustard",
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

                var id = p.ProductID;


                var fetchedProduct = ctx.Products.FirstOrDefault(x => x.ProductID == id);
                fetchedProduct.UnitsInStock = 25;
                ctx.Update(fetchedProduct);
                ctx.SaveChanges();

                var fetchedUpdatedProduct = ctx.Products.FirstOrDefault(x => x.ProductID == id);
                Assert.Equal(25,fetchedUpdatedProduct!.UnitsInStock);
            }
        }

        [ConditionalFact]
        public async Task Transaction_Finalized_On_Connection_Close()
        {
            string connectionString;
            using (var ctx = _fixture.CreateContext())
            {
                connectionString = ctx.Database.GetConnectionString();
               
            }

            using (var conn = new NuoDbConnection(connectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var command = conn.CreateCommand();
                    command.CommandText = "BLAH"; //intentionally bad query
                    command.ExecuteNonQuery();
                    //ctx.Database.ExecuteSqlRaw("SELECT *");
                    transaction.Commit();
                }
                catch (Exception ex)
                {

                }
                conn.Close();
                conn.Open();
                var tx2 = conn.BeginTransaction();
            }
        }
    }
}
