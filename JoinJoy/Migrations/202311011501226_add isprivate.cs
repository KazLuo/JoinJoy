namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addisprivate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "isPrivate", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "isPrivate");
        }
    }
}
