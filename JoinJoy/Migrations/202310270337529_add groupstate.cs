namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addgroupstate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "GroupState", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "GroupState");
        }
    }
}
