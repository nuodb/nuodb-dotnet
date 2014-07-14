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
		}
        
        public override void Down()
        {
            DropTable("hockey.TestEntities");
        }
    }
}
