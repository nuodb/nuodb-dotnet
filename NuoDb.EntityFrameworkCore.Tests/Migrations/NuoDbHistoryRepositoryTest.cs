using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.Tests.Migrations
{
    public class NuoDbHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [ConditionalFact]
        public void GetCreateScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateScript();

            Assert.Equal(
                "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (" 
                +EOL
                + "    \"MigrationId\" NATIONAL CHAR VARYING(150) NOT NULL PRIMARY KEY,"
                + EOL
                + "    \"ProductVersion\" NATIONAL CHAR VARYING(32) NOT NULL"
                + EOL
                + ");"
                + EOL,
                sql);
        }

        private static IHistoryRepository CreateHistoryRepository(string schema = null)
            => new TestDbContext(
                    new DbContextOptionsBuilder()
                        .UseInternalServiceProvider(NuoDbTestHelpers.Instance.CreateServiceProvider())
                        .UseNuoDb(
                            new NuoDbConnection("Server=localhost;Database=demo;User=dba;Password=dba;Schema=USER"),
                            b => b.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema))
                        .Options)
                .GetService<IHistoryRepository>();


        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }

        }

        private class Blog
        {
            public int Id { get; set; }
        }
    }
}
