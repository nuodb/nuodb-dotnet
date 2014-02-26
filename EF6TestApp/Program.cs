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
			using (var ctx = new NuoDbContext())
			{
				Console.WriteLine(ctx.GetType().BaseType.Assembly.FullName);

				var script = (ctx as IObjectContextAdapter).ObjectContext.CreateDatabaseScript();

				var query1 = ctx.FooBars.Where(x => x.Id == 666);
				var query2 = ctx.FooBars.Where(x => new[] { 1, 2, 3 }.Contains(x.Id));
				Console.WriteLine(query1);
				Console.WriteLine(query2);
			}
		}
	}

	class NuoDbContext : DbContext
	{
		public NuoDbContext()
			: base(new NuoDbConnection("Server=localhost;Database=ef6;User=ef;Password=ef;Schema=user"), true)
		{ }

		public DbSet<FooBar> FooBars { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.HasDefaultSchema("user");
		}
	}

	class FooBar
	{
		public int Id { get; set; }
		public string FooBarBaz { get; set; }
	}
}
