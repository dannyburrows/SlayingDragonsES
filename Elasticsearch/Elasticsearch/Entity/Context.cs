using System.Data.Entity;

namespace Elasticsearch.Entity
{
    public class Context : DbContext
    {
        public DbSet<Treasure> Treasures { get; set; }
        public DbSet<Quest> Quests { get; set; }
    }
}