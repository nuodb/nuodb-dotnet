using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.NuoDb.Design.Internal;

namespace NuoDb.EntityFrameworkCore.Tests.Design
{
    public class DesignTests
    {
        [ConditionalFact]
        public void ConfigureDesignTimeServices_RegistersServicesCorrectly()
        {
            var serviceCollection = new ServiceCollection();
#pragma warning disable EF1001
            var designServices = new NuoDbDesignTimeServices();
            designServices.ConfigureDesignTimeServices(serviceCollection);
#pragma warning restore EF1001

            var provider = serviceCollection.BuildServiceProvider();
            var modelFactory = provider.GetService<IDatabaseModelFactory>();
            Assert.NotNull(modelFactory);
            
        }


       
    }
}
