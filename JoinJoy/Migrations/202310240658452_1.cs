namespace JoinJoy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        GroupId = c.Int(nullable: false, identity: true),
                        MemberId = c.Int(nullable: false),
                        StoreId = c.Int(),
                        GroupName = c.String(maxLength: 100),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        MaxParticipants = c.Int(nullable: false),
                        CurrentParticipants = c.Int(nullable: false),
                        Description = c.String(maxLength: 500),
                        IsHomeEvent = c.Boolean(nullable: false),
                        Address = c.String(maxLength: 200),
                        InitialMembersCount = c.Int(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.GroupId)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .ForeignKey("dbo.Stores", t => t.StoreId)
                .Index(t => t.MemberId)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.GroupComments",
                c => new
                    {
                        CommentId = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        CommentContent = c.String(maxLength: 500),
                        CommentDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CommentId)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.GroupId);
            
            CreateTable(
                "dbo.GroupParticipants",
                c => new
                    {
                        ParticipantId = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        JoinDate = c.DateTime(nullable: false),
                        AttendanceStatus = c.String(),
                    })
                .PrimaryKey(t => t.ParticipantId)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.GroupId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Groups", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.Groups", "MemberId", "dbo.Members");
            DropForeignKey("dbo.GroupParticipants", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.GroupComments", "GroupId", "dbo.Groups");
            DropIndex("dbo.GroupParticipants", new[] { "GroupId" });
            DropIndex("dbo.GroupComments", new[] { "GroupId" });
            DropIndex("dbo.Groups", new[] { "StoreId" });
            DropIndex("dbo.Groups", new[] { "MemberId" });
            DropTable("dbo.GroupParticipants");
            DropTable("dbo.GroupComments");
            DropTable("dbo.Groups");
        }
    }
}
