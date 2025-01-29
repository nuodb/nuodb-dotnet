using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.Tests.TestUtilities
{
    public class NuoDbTestHelpers: RelationalTestHelpers
    {
        protected NuoDbTestHelpers()
        {
        }

        public static NuoDbTestHelpers Instance { get; } = new();

        public override IServiceCollection AddProviderServices(IServiceCollection services) =>
            services.AddEntityFrameworkNuoDb();

        public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNuoDb(
                new NuoDbConnection("Server=localhost;Database=demo;User=dba;Password=dba;Schema=USER"));

    }
}
