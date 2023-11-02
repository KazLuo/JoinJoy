namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixsotre : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Stores", "Phone", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Stores", "Phone", c => c.Int(nullable: false));
        }
    }
}
