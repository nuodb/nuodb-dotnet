using ConsoleEFCoreTest.Entities;
using Microsoft.EntityFrameworkCore;
using NuoDb.Data.Client;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace ConsoleEFCoreTest.Contexts
{
    public class DataContext: DbContext
    {
        private readonly string _connectionString;
        public DbSet<Player> Players { get; set; }
        public DbSet<Scoring> Scorings { get; set; }

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scoring>()
                .HasOne<Player>(x=>x.Player)
                .WithMany();
            modelBuilder.Entity<Scoring>()
                .HasKey(c => new { c.PlayerId, c.Year, c.Stint, c.TeamId, c.Position });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Log out sql 
            optionsBuilder.LogTo(message => Console.WriteLine(message)
                    , LogLevel.Information)
                .EnableDetailedErrors();
            //
            // NuoDbConnectionStringBuilder builder = new NuoDbConnectionStringBuilder();
            // builder.Server = "localhost";
            // builder.Database = "demo";
            // builder.User = "dba";
            // builder.Password = "dba";
            // builder.Schema = "USER";
            //
            // var connstr = builder.ToString();

            optionsBuilder.UseNuoDb(_connectionString);
        }
    }
}
