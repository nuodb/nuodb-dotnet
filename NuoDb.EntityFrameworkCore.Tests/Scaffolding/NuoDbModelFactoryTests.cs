using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuoDb.EntityFrameworkCore.NuoDb.Design.Internal;
using NuoDb.EntityFrameworkCore.NuoDb.Diagnostics.Internal;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;

namespace NuoDb.EntityFrameworkCore.Tests.Scaffolding
{
    public class NuoDbModelFactoryTests: IClassFixture<NuoDbModelFactoryTests.NuoDbDatabaseModelFixture>
    {
        protected NuoDbDatabaseModelFixture Fixture {get;}
        public NuoDbModelFactoryTests(NuoDbDatabaseModelFixture fixture)
        {
            Fixture = fixture;
            Fixture.ListLoggerFactory.Clear();
        }

        private void Test(
            string createSql,
            IEnumerable<string> tables,
            IEnumerable<string> schemas,
            Action<DatabaseModel> asserter,
            string cleanupSql)
        {
            Fixture.TestStore.ExecuteNonQuery(createSql);

            try
            {
                // NOTE: You may need to update AddEntityFrameworkDesignTimeServices() too
                var databaseModelFactory = GetDatabaseModelFactory();

                var databaseModel = databaseModelFactory.Create(
                    Fixture.TestStore.ConnectionString,
                    new DatabaseModelFactoryOptions(tables, schemas));
                Assert.NotNull(databaseModel);
                asserter(databaseModel);
            }
            finally
            {
                if (!string.IsNullOrEmpty(cleanupSql))
                {
                    Fixture.TestStore.ExecuteNonQuery(cleanupSql);
                }
            }
        }

        private IDatabaseModelFactory GetDatabaseModelFactory()
        {
            var services = new ServiceCollection()
                .AddSingleton<TypeMappingSourceDependencies>()
                .AddSingleton<RelationalTypeMappingSourceDependencies>()
                .AddSingleton<ValueConverterSelectorDependencies>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<LoggingDefinitions, NuoDbLoggingDefinitions>()
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                .AddSingleton<IValueConverterSelector, ValueConverterSelector>()
                .AddSingleton<ILoggerFactory>(Fixture.ListLoggerFactory)
                .AddSingleton<IDbContextLogger, NullDbContextLogger>();

            new NuoDbDesignTimeServices().ConfigureDesignTimeServices(services);

            var databaseModelFactory = services
                .BuildServiceProvider() // No scope validation; design services only resolved once
                .GetRequiredService<IDatabaseModelFactory>();
            return databaseModelFactory;
        }

        [ConditionalFact]
        public void GenerateTableFilter_filters()
        {
            //var schema = Fixture.TestStore.
            Test($@"
                CREATE TABLE Table1(
                    Id integer PRIMARY KEY
                );
                CREATE TABLE {Fixture.TestStore.Name}.Table2(
                    Id integer PRIMARY KEY
                );
            ",
                new List<string>(){"Table1",$"{Fixture.TestStore.Name}.Table2" },
                new List<string>(){"NuoDbModelFactoryTests"},
                dbModel =>
                {
                    Assert.Equal(2,dbModel.Tables.Count);
                },
                $@"DROP TABLE Table1;
                  DROP TABLE {Fixture.TestStore.Name}.Table2;");
        }

        public class NuoDbDatabaseModelFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = nameof(NuoDbModelFactoryTests);

            protected override ITestStoreFactory TestStoreFactory
                => NuoDbTestStoreFactory.Instance;

            public new NuoDbTestStore TestStore
                => (NuoDbTestStore)base.TestStore;

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Scaffolding.Name;
        }
    }
}
