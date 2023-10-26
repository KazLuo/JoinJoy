namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addstoreowner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "IsStoreOwner", c => c.Boolean(nullable: false));
            DropColumn("dbo.Members", "IsStoreOwer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Members", "IsStoreOwer", c => c.Boolean(nullable: false));
            DropColumn("dbo.Members", "IsStoreOwner");
        }
    }
}
