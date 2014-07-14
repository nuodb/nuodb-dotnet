using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuoDb.Data.Client;
using NuoDb.Data.Client.EntityFramework6;

namespace MigrationsTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var migrator = new DbMigrator(new Migrations.Configuration());
			var scripting = new MigratorScriptingDecorator(migrator);
			var script = scripting.ScriptUpdate("0", null);
			Console.WriteLine(script);
		}
	}

	[DbConfigurationType(typeof(NuoDbContextConfiguration))]
	class NuoDbContext : DbContext
	{
		public NuoDbContext()
			: base("name=Foobar")
		{ }

		public DbSet<TestEntity> TestEntities { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			var builder = new NuoDbConnectionStringBuilder(Database.Connection.ConnectionString);
			modelBuilder.HasDefaultSchema(builder.Schema);
		}
	}

	class NuoDbContextConfiguration : DbConfiguration
	{
		public NuoDbContextConfiguration()
		{
			SetMigrationSqlGenerator("NuoDb.Data.Client", () => new NuoDbMigrationSqlGenerator());
			SetDatabaseInitializer<NuoDbContext>(null);
		}
	}

	class TestEntity
	{
		public int Id { get; set; }
		public int FooBar { get; set; }
	}
}
