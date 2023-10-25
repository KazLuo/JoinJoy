namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupGames",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        StoreInventoryId = c.Int(nullable: false),
                        InitDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.StoreInventories", t => t.StoreInventoryId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.StoreInventoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupGames", "StoreInventoryId", "dbo.StoreInventories");
            DropForeignKey("dbo.GroupGames", "GroupId", "dbo.Groups");
            DropIndex("dbo.GroupGames", new[] { "StoreInventoryId" });
            DropIndex("dbo.GroupGames", new[] { "GroupId" });
            DropTable("dbo.GroupGames");
        }
    }
}
