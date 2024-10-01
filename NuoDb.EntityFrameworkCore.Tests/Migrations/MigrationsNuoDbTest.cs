using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.NuoDb.Scaffolding.Internal;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;
using Xunit.Abstractions;

namespace NuoDb.EntityFrameworkCore.Tests.Migrations
{
    [Collection("Sequential Tests")]
    public class MigrationsNuoDbTest: MigrationsTestBase<MigrationsNuoDbTest.MigrationsNuoDbFixture>
    {
        protected static string EOL => Environment.NewLine;
        private readonly string _storeName;

        public MigrationsNuoDbTest(MigrationsNuoDbFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            _storeName = fixture.TestStore.Name;
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Create_table()
        {
            await base.Create_table();

            AssertSql(
                @"CREATE TABLE IF NOT EXISTS ""People"" (
    ""Id"" integer NOT NULL,
    ""Name"" string NULL,
    CONSTRAINT ""PK_People"" PRIMARY KEY (""Id"")
);");
        }

        public override async Task Create_table_all_settings()
        {
             var intStoreType = TypeMappingSource.FindMapping(typeof(int)).StoreType;
            var char11StoreType = TypeMappingSource.FindMapping(typeof(string), storeTypeName: null, size: 11).StoreType;

            await Test(
                builder => builder.Entity(
                    "Employers", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    }),
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {

                        e.Property<int>("CustomId");
                        e.Property<int>("EmployerId")
                            .HasComment("Employer ID comment");
                        e.Property<string>("SSN")
                            .HasColumnType(char11StoreType)
                            .UseCollation(NonDefaultCollation)
                            .IsRequired(false);

                        e.HasKey("CustomId");
                        e.HasAlternateKey("SSN");
                        e.HasCheckConstraint("CK_People_EmployerId", $"{DelimitIdentifier("EmployerId")} > 0");
                        e.HasOne("Employers").WithMany("People").HasForeignKey("EmployerId");

                        e.HasComment("Table comment");
                    }),
                model =>
                {
                    var employersTable = Assert.Single(model.Tables, t => t.Name == "Employers");
                    var peopleTable = Assert.Single(model.Tables, t => t.Name == "People");

                    Assert.Equal("People", peopleTable.Name);
                    if (AssertSchemaNames)
                    {
                        Assert.Equal("dbo2", peopleTable.Schema);
                    }

                    Assert.Collection(
                        peopleTable.Columns.OrderBy(c => c.Name),
                        c =>
                        {
                            Assert.Equal("CustomId", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal(intStoreType, c.StoreType);
                            Assert.Null(c.Comment);
                        },
                        c =>
                        {
                            Assert.Equal("EmployerId", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal(intStoreType, c.StoreType);
                            if (AssertComments)
                                Assert.Equal("Employer ID comment", c.Comment);
                        },
                        c =>
                        {
                            Assert.Equal("SSN", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal(char11StoreType, c.StoreType);
                            Assert.Null(c.Comment);
                        });

                    Assert.Same(
                        peopleTable.Columns.Single(c => c.Name == "CustomId"),
                        Assert.Single(peopleTable.PrimaryKey!.Columns));
                    Assert.Same(
                        peopleTable.Columns.Single(c => c.Name == "SSN"),
                        Assert.Single(Assert.Single(peopleTable.UniqueConstraints).Columns));
                    // TODO: Need to scaffold check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408

                    var foreignKey = Assert.Single(peopleTable.ForeignKeys);
                    Assert.Same(peopleTable, foreignKey.Table);
                    Assert.Same(peopleTable.Columns.Single(c => c.Name == "EmployerId"), Assert.Single(foreignKey.Columns));
                    Assert.Same(employersTable, foreignKey.PrincipalTable);
                    Assert.Same(employersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));
                   
                });
        }

        public override Task Rename_index()
        {
            return Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                    }),
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "Foo"),
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "foo2"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal("foo2", index.Name);
                });
        }

        public override async Task Rename_sequence()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Rename_sequence());
        }

        public override Task Create_table_no_key()
        {
            return base.Create_table_no_key();
        }

        public override async Task Alter_sequence_all_settings()
        {
            Test(
                builder => builder.HasSequence<int>("foo"),
                builder => { },
                builder => builder.HasSequence<int>("foo")
                    .StartsAt(5),
                model =>
                {
                   Assert.Single(model.Sequences);
                });
        }

        public override async Task Alter_sequence_increment_by()
        {
            //nuo db doesnt support increment parameters
        }

        public override async Task Create_index()
        {
            Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                    }),
                builder => { },
                builder => builder.Entity("People").HasIndex("FirstName"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(table, index.Table);
                    Assert.Same(table.Columns.Single(c => c.Name == "FirstName"), Assert.Single(index.Columns));
                    Assert.Equal("IX_People_FirstName", index.Name);
                    Assert.False(index.IsUnique);
                    Assert.Null(index.Filter);
                });
        }

        public override async Task Add_column_computed_with_collation()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name")
                    .HasComputedColumnSql("id + 1")
                    .UseCollation(NonDefaultCollation),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var nameColumn = Assert.Single(table.Columns, c => c.Name == "Name");
                    if (AssertComputedColumns)
                        Assert.Contains("hello", nameColumn.ComputedColumnSql);
                    if (AssertCollations)
                        Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                });
        }

        public override async Task Add_column_with_defaultValueSql()
        {
            await Test(
                builder =>
                {
                    builder.Entity("People").Property<int>("Id");
                },
                builder => { },
                builder => builder.Entity("People").Property<int>("Sum")
                    .HasDefaultValueSql("1 + 2"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Contains("3", sumColumn.DefaultValueSql);
                });
        }

        public override async Task SqlOperation()
        {
            await Test(
                builder => { },
                new SqlOperation { Sql = "select 'test' from dual" },
                model =>
                {
                    Assert.Empty(model.Tables);
                    Assert.Empty(model.Sequences);
                });
        }

        public override async Task Move_sequence()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>base.Move_sequence());
        }

        public override async Task Create_index_with_filter()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Create_index_with_filter());
        }

        public override async Task Create_unique_index_with_filter()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=> base.Create_unique_index_with_filter());
        }

        public override async Task Alter_computed_column_add_comment()
        {
            //NuoDb Does not support comments for fields
            //return base.Alter_computed_column_add_comment();
        }

        public class MigrationsNuoDbFixture : MigrationsFixtureBase
        {
            protected override string StoreName { get; } = nameof(MigrationsNuoDbTest);

            protected override ITestStoreFactory TestStoreFactory
                => NuoDbTestStoreFactory.Instance;

            public override TestHelpers TestHelpers
                => NuoDbTestHelpers.Instance;


            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, NuoDbDatabaseModelFactory>();
        }

        public override Task Create_sequence_all_settings()
        {
            //nothing really to see here
            //return base.Create_sequence_all_settings();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Overriden to ensure necessary schemas are provided to DatabaseModelFactory
        /// </summary>
        /// <param name="sourceModel"></param>
        /// <param name="targetModel"></param>
        /// <param name="operations"></param>
        /// <param name="asserter"></param>
        /// <returns></returns>
        protected override async Task Test(
            IModel sourceModel,
            IModel targetModel,
            IReadOnlyList<MigrationOperation> operations,
            Action<DatabaseModel> asserter)
        {
            var context = CreateContext();
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var migrationsSqlGenerator = serviceProvider.GetRequiredService<IMigrationsSqlGenerator>();
            var modelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();
            var migrationsCommandExecutor = serviceProvider.GetRequiredService<IMigrationCommandExecutor>();
            var connection = serviceProvider.GetRequiredService<IRelationalConnection>();
            var databaseModelFactory = serviceProvider.GetRequiredService<IDatabaseModelFactory>();

            try
            {
                // Apply migrations to get to the source state, and do a scaffolding snapshot for later comparison.
                // Suspending event recording, we're not interested in the SQL of this part
                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    await migrationsCommandExecutor.ExecuteNonQueryAsync(
                        migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(null, sourceModel.GetRelationalModel()), sourceModel),
                        connection);
                }

                // Apply migrations to get from source to target, then reverse-engineer and execute the
                // test-provided assertions on the resulting database model
                await migrationsCommandExecutor.ExecuteNonQueryAsync(
                    migrationsSqlGenerator.Generate(operations, targetModel), connection);


                //Front load schemas to be used with modelfactory for verification
                List<string> schemas = operations.Where(x => x is TableOperation)
                    .Select(x => (x as TableOperation).Schema)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                var scaffoldedModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions(schemas: schemas
                            .ToList()
                            .Append(_storeName)
                            .ToList()));

                asserter?.Invoke(scaffoldedModel);
            }
            finally
            {
                using var _ = Fixture.TestSqlLoggerFactory.SuspendRecordingEvents();
                Fixture.TestStore.Clean(context);
            }
        }

        protected override string NonDefaultCollation { get; }
        protected override bool AssertSchemaNames { get; } = false;
        protected override bool AssertComments { get; } = false;
        protected override bool AssertComputedColumns { get; } = false;
        protected override bool AssertIndexFilters { get; } = false;
    }


  
}
