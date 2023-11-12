namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcover : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StorePhotoes", "IsCover", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StorePhotoes", "IsCover");
        }
    }
}
