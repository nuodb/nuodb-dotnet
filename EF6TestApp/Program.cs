using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuoDb.Data.Client;

namespace EF6TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var ctx = GetContext())
			{
				Console.WriteLine(ctx.GetType().BaseType.Assembly.FullName);

				var query1 = ctx.FooBars.Where(x => x.Id == 666);
				var query2 = ctx.FooBars.Where(x => new[] { 1, 2, 3 }.Contains(x.Id));
				Console.WriteLine(query1);
				Console.WriteLine(query2);
			}
		}

		static ISomeNuoDbDbContext GetContext()
		{
			//return new NuoDbDirectContext();
			return new NuoDbConnectionStringContext();
		}
	}

	class NuoDbDirectContext : DbContext, ISomeNuoDbDbContext
	{
		public NuoDbDirectContext()
			: base(new NuoDbConnection("Server=localhost;Database=ef6;User=ef;Password=ef;Schema=user"), true)
		{ }

		public DbSet<FooBar> FooBars { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.HasDefaultSchema("user");
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

			modelBuilder.HasDefaultSchema("user");
		}
	}

	interface ISomeNuoDbDbContext : IDisposable
	{
		DbSet<FooBar> FooBars { get; }
	}

	class FooBar
	{
		public int Id { get; set; }
		public string FooBarBaz { get; set; }
	}
}
