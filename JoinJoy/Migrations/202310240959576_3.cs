namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StoreFollows",
                c => new
                    {
                        FollowId = c.Int(nullable: false, identity: true),
                        StoreId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FollowId)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId)
                .Index(t => t.MemberId);
            
            CreateTable(
                "dbo.MemberFollows",
                c => new
                    {
                        FollowId = c.Int(nullable: false, identity: true),
                        MemberId = c.Int(nullable: false),
                        StoreId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FollowId)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.MemberId)
                .Index(t => t.StoreId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemberFollows", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.MemberFollows", "MemberId", "dbo.Members");
            DropForeignKey("dbo.StoreFollows", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.StoreFollows", "MemberId", "dbo.Members");
            DropIndex("dbo.MemberFollows", new[] { "StoreId" });
            DropIndex("dbo.MemberFollows", new[] { "MemberId" });
            DropIndex("dbo.StoreFollows", new[] { "MemberId" });
            DropIndex("dbo.StoreFollows", new[] { "StoreId" });
            DropTable("dbo.MemberFollows");
            DropTable("dbo.StoreFollows");
        }
    }
}
