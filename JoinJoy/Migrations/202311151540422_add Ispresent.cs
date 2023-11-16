namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIspresent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GroupParticipants", "IsPresent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GroupParticipants", "IsPresent");
        }
    }
}
