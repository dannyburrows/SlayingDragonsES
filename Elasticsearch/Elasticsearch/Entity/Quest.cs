using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Models;
using Nest;

namespace Elasticsearch.Entity
{
    public class Quest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public double StartLat { get; set; }
        public double StartLong { get; set; }
        public double EndLat { get; set; }
        public double EndLong { get; set; }
        public int Difficulty { get; set; }

        public virtual List< Treasure > Treasures { get; set; }
    }
}