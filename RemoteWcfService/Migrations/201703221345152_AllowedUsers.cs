namespace RemoteWcfService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AllowedUsers : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Devices", "User_Id", "dbo.Users");
            DropIndex("dbo.Devices", new[] { "User_Id" });
            CreateTable(
                "dbo.UserDevices",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Device_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Device_Id })
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.Devices", t => t.Device_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Device_Id);
            
            DropColumn("dbo.Devices", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Devices", "User_Id", c => c.Int());
            DropForeignKey("dbo.UserDevices", "Device_Id", "dbo.Devices");
            DropForeignKey("dbo.UserDevices", "User_Id", "dbo.Users");
            DropIndex("dbo.UserDevices", new[] { "Device_Id" });
            DropIndex("dbo.UserDevices", new[] { "User_Id" });
            DropTable("dbo.UserDevices");
            CreateIndex("dbo.Devices", "User_Id");
            AddForeignKey("dbo.Devices", "User_Id", "dbo.Users", "Id");
        }
    }
}
