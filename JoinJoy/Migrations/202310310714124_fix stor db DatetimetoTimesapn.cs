namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixstordbDatetimetoTimesapn : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Stores", "OpenTime", c => c.Time(nullable: false, precision: 7));
            AlterColumn("dbo.Stores", "CloseTime", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Stores", "CloseTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Stores", "OpenTime", c => c.DateTime(nullable: false));
        }
    }
}
