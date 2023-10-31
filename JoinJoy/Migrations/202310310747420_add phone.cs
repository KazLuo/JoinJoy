namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addphone : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stores", "Phone", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stores", "Phone");
        }
    }
}
