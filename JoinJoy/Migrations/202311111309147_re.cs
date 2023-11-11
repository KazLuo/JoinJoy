namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class re : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Stores", "Iframe", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Stores", "Iframe", c => c.String(nullable: false));
        }
    }
}
