namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addgroup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreRatings", "GroupId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StoreRatings", "GroupId");
        }
    }
}
