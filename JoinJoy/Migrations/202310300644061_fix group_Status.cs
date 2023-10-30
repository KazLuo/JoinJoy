namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixgroup_Status : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GroupParticipants", "AttendanceStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GroupParticipants", "AttendanceStatus", c => c.String());
        }
    }
}
