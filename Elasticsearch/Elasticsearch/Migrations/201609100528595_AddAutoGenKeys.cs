namespace Elasticsearch.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAutoGenKeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TreasureQuests", "Quest_Id", "dbo.Quests");
            DropForeignKey("dbo.TreasureQuests", "Treasure_Id", "dbo.Treasures");
            DropPrimaryKey("dbo.Quests");
            DropPrimaryKey("dbo.Treasures");
            AlterColumn("dbo.Quests", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("dbo.Treasures", "Id", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("dbo.Quests", "Id");
            AddPrimaryKey("dbo.Treasures", "Id");
            AddForeignKey("dbo.TreasureQuests", "Quest_Id", "dbo.Quests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TreasureQuests", "Treasure_Id", "dbo.Treasures", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TreasureQuests", "Treasure_Id", "dbo.Treasures");
            DropForeignKey("dbo.TreasureQuests", "Quest_Id", "dbo.Quests");
            DropPrimaryKey("dbo.Treasures");
            DropPrimaryKey("dbo.Quests");
            AlterColumn("dbo.Treasures", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.Quests", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.Treasures", "Id");
            AddPrimaryKey("dbo.Quests", "Id");
            AddForeignKey("dbo.TreasureQuests", "Treasure_Id", "dbo.Treasures", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TreasureQuests", "Quest_Id", "dbo.Quests", "Id", cascadeDelete: true);
        }
    }
}
