namespace MigrationsTest.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class Initial : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"hockey.TestEntities",
				c => new
					{
						Id = c.Int(nullable: false, identity: true),
						FooBar = c.Int(nullable: false),
					})
				.PrimaryKey(t => t.Id);
			AddColumn("hockey.SomeTable", "SomeColumn", c => c.Int(nullable: false, defaultValue: 6));
			AlterColumn("hockey.SomeTable", "SomeColumn", c => c.Long(nullable: false, defaultValue: 10));
			CreateIndex("hockey.SomeTable", "FooBar", unique: false, name: "IX_SomeTable_FooBar");
			CreateIndex("hockey.SomeTable", "FooBar", unique: true, name: "UX_SomeTable_FooBar");
		}

		public override void Down()
		{
			DropTable("hockey.TestEntities");
		}
	}
}
