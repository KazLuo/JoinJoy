namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addinitmember : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GroupParticipants", "InitMember", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GroupParticipants", "InitMember");
        }
    }
}
