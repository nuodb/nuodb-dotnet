using Microsoft.EntityFrameworkCore.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class NuoDbNorthwindTestStoreFactory: NuoDbTestStoreFactory
    {
        public const string Name = "Northwind";
        public static readonly string NorthwindConnectionString = NuoDbTestStore.CreateConnectionString("Northwind");
        public static new NuoDbNorthwindTestStoreFactory Instance { get; } = new();

        protected NuoDbNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => NuoDbTestStore.GetOrCreate(storeName, "Northwind.sql");
    }
}
