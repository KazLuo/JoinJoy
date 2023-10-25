namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixsalt : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Members", "Password", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Members", "Password", c => c.String(nullable: false, maxLength: 12));
        }
    }
}
