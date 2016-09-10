namespace Elasticsearch.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Quests",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 20),
                        Description = c.String(nullable: false, maxLength: 255),
                        BeginDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        StartLat = c.Double(nullable: false),
                        StartLong = c.Double(nullable: false),
                        EndLat = c.Double(nullable: false),
                        EndLong = c.Double(nullable: false),
                        Difficulty = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Treasures",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 20),
                        Description = c.String(nullable: false, maxLength: 255),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TreasureQuests",
                c => new
                    {
                        Treasure_Id = c.Guid(nullable: false),
                        Quest_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Treasure_Id, t.Quest_Id })
                .ForeignKey("dbo.Treasures", t => t.Treasure_Id, cascadeDelete: true)
                .ForeignKey("dbo.Quests", t => t.Quest_Id, cascadeDelete: true)
                .Index(t => t.Treasure_Id)
                .Index(t => t.Quest_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TreasureQuests", "Quest_Id", "dbo.Quests");
            DropForeignKey("dbo.TreasureQuests", "Treasure_Id", "dbo.Treasures");
            DropIndex("dbo.TreasureQuests", new[] { "Quest_Id" });
            DropIndex("dbo.TreasureQuests", new[] { "Treasure_Id" });
            DropTable("dbo.TreasureQuests");
            DropTable("dbo.Treasures");
            DropTable("dbo.Quests");
        }
    }
}
