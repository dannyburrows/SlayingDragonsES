namespace Elasticsearch.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KeyGen : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Quests", "Id", c => c.Guid(nullable: false, identity: true, defaultValueSql:"newid()"));
            AlterColumn("dbo.Treasures", "Id", c => c.Guid(nullable: false, identity: true, defaultValueSql: "newid()"));
        }

        public override void Down()
        {
        }
    }
}
