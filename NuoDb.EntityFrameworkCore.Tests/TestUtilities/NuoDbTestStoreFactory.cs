using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class NuoDbTestStoreFactory: RelationalTestStoreFactory
    {
        public static NuoDbTestStoreFactory Instance { get; } = new();

        protected NuoDbTestStoreFactory()
        {
        }

        public override TestStore Create(string storeName)
            => NuoDbTestStore.Create(storeName);

        public override TestStore GetOrCreate(string storeName)
            => NuoDbTestStore.GetOrCreate(storeName);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkNuoDb();
    }
}
