﻿using System.Data.Entity;

namespace Elasticsearch.Entity
{
    public class Context : DbContext
    {
        public Context( )
        {
            this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Treasure> Treasures { get; set; }
        public DbSet<Quest> Quests { get; set; }
    }
}