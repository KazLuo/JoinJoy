namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StoreFollows", "InitDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.MemberFollows", "InitDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MemberFollows", "InitDate");
            DropColumn("dbo.StoreFollows", "InitDate");
        }
    }
}
