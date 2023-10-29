namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixgroupmaxlength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Groups", "Description", c => c.String(maxLength: 100));
            AlterColumn("dbo.Groups", "Address", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Groups", "Address", c => c.String(maxLength: 200));
            AlterColumn("dbo.Groups", "Description", c => c.String(maxLength: 500));
        }
    }
}
