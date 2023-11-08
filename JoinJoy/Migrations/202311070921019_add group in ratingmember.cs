namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addgroupinratingmember : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MemberRatings", "GroupId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MemberRatings", "GroupId");
        }
    }
}
