using System;
using System.Collections.Generic;
using System.Data.Entity;
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

		}
	}

	[DbConfigurationType(typeof(NuoDbContextConfiguration))]
	class NuoDbContext : DbContext
	{
		public NuoDbContext()
			: base("name=Foobar")
		{ }

		public DbSet<TestEntity> TestEntities { get; set; }
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
