namespace RemoteWcfService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PridaneDevices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        CpuInfo = c.String(),
                        OsInfo = c.String(),
                        MacAddress = c.String(),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Devices", "User_Id", "dbo.Users");
            DropIndex("dbo.Devices", new[] { "User_Id" });
            DropTable("dbo.Devices");
        }
    }
}
