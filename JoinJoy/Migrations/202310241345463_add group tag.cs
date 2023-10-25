namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addgrouptag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "IsHomeGroup", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "InitMember", c => c.Int(nullable: false));
            AddColumn("dbo.Groups", "Beginner", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "Expert", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "Practice", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "Open", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "Tutorial", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "Casual", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "Competitive", c => c.Boolean(nullable: false));
            DropColumn("dbo.Groups", "IsHomeEvent");
            DropColumn("dbo.Groups", "InitialMembersCount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Groups", "InitialMembersCount", c => c.Int(nullable: false));
            AddColumn("dbo.Groups", "IsHomeEvent", c => c.Boolean(nullable: false));
            DropColumn("dbo.Groups", "Competitive");
            DropColumn("dbo.Groups", "Casual");
            DropColumn("dbo.Groups", "Tutorial");
            DropColumn("dbo.Groups", "Open");
            DropColumn("dbo.Groups", "Practice");
            DropColumn("dbo.Groups", "Expert");
            DropColumn("dbo.Groups", "Beginner");
            DropColumn("dbo.Groups", "InitMember");
            DropColumn("dbo.Groups", "IsHomeGroup");
        }
    }
}
