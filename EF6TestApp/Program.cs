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
				var script = (ctx as IObjectContextAdapter).ObjectContext.CreateDatabaseScript();
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
		}
	}

	class FooBar
	{
		public int Id { get; set; }
		public string FooBarBaz { get; set; }
	}
}
