using System;
using System.Collections.Generic;
using Nest;

namespace Elasticsearch.Entity
{
    public class Treasure
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }

        public virtual List<Quest> Quests { get; set; }
    }
}