using Microsoft.EntityFrameworkCore.Migrations;
using NuoDb.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NuoDb.Data.Client;

namespace NuoDb.EntityFrameworkCore.Tests.Migrations
{
    public class NuoDbHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;
        private static string SQL = $"CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" ({EOL}    \"MigrationId\" varchar(150) NOT NULL,{EOL}    \"ProductVersion\" varchar(32) NOT NULL,{EOL}    CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY (\"MigrationId\"){EOL});{EOL}";

        [ConditionalFact]
        public void GetCreateScript_works()
        {
            var script = CreateHistoryRepository().GetCreateScript();
            Assert.Equal(SQL, script);
        }

        [ConditionalFact]
        public void GetExists_works()
        {
            var exists = CreateHistoryRepository().Exists();
            Assert.False(exists);
        }

        [ConditionalFact]
        public void GetEndIfScript_works()
        {
            Assert.Throws<NotSupportedException>(()=>CreateHistoryRepository().GetEndIfScript());
            
        }

        [ConditionalFact]
        public void GetCreateIfNotExistsScript_works()
        {
            var script = CreateHistoryRepository().GetCreateIfNotExistsScript();
            Assert.Equal(SQL, script);
        }

        [ConditionalFact]
        public async Task GetBeginIfNotExistsScript_works()
        {
            Assert.Throws<NotSupportedException>(()=>CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1"));
        }

        [ConditionalFact]
        public async Task GetBeginIfExistsScript_works()
        {
            Assert.Throws<NotSupportedException>(()=>CreateHistoryRepository().GetBeginIfExistsScript("Migration1"));
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
