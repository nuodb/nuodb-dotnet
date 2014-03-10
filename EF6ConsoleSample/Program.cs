using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using NuoDb.Data.Client;

namespace EF6ConsoleSample
{
    interface ISomeNuoDbDbContext : IDisposable
    {
        DbSet<FooBar> FooBars { get; }
    }

    class FooBar
    {
        public int Id { get; set; }
        public string FooBarBaz { get; set; }
    }

    class NuoDbDirectContext : DbContext, ISomeNuoDbDbContext
    {
        public NuoDbDirectContext()
            : base(new NuoDbConnection("Server=localhost;Database=test;User=dba;Password=goalie;Schema=hello"), true)
        { }

        public DbSet<FooBar> FooBars { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("hello");
        }
    }

    class NuoDbConnectionStringContext : DbContext, ISomeNuoDbDbContext
    {
        public NuoDbConnectionStringContext()
            : base("FooBar")
        { }

        public DbSet<FooBar> FooBars { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("hello");
        }
    }

    class Program
    {
        static ISomeNuoDbDbContext GetContext()
        {
            //return new NuoDbDirectContext();
            return new NuoDbConnectionStringContext();
        }
        
        static void Main(string[] args)
        {
            try
            {
                using (var ctx = GetContext())
                {
                    Console.WriteLine("Using Entity Framework version: " + ctx.GetType().BaseType.Assembly.FullName);

                    var query1 = ctx.FooBars.Where(x => x.Id == 666);
                    Console.WriteLine("Linq query  : ctx.FooBars.Where(x => x.Id == 666)");
                    Console.WriteLine("-> SQL query:");
                    Console.WriteLine(query1);
                    Console.WriteLine();
                    var query2 = ctx.FooBars.Where(x => new[] { 1, 2, 3 }.Contains(x.Id));
                    Console.WriteLine("Linq query  : ctx.FooBars.Where(x => new[] { 1, 2, 3 }.Contains(x.Id))");
                    Console.WriteLine("-> SQL query:");
                    Console.WriteLine(query2);
                }
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine(e.Message);
                System.Console.Out.WriteLine(e.StackTrace);
            }
        }
    }
}
