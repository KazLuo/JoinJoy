namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixsomeMaxLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Members", "Introduce", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Members", "Introduce", c => c.String(maxLength: 120));
        }
    }
}
