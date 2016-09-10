using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

namespace Elasticsearch.Models
{
    [ElasticsearchType(Name = "QuestSearchModel", IdProperty = "Id")]
    public class Quest
    {
        [String]
        public Guid Id { get; set; }
        [String]
        public string Name { get; set; }
        [Date]
        public DateTime BeginDate { get; set; }
        [Date]
        public DateTime EndDate { get; set; }
        [GeoPoint]
        public Geo CoordStart { get; set; }
        [GeoPoint]
        public Geo CoordEnd { get; set; }
        [Nested(IncludeInParent = true)]
        public List<Treasure> Treasures { get; set; }
        [Boolean]
        public bool HasTreasure => Treasures.Any( );
        [Number(NumberType.Integer, Coerce = true)]
        public int Difficulty { get; set; }
    }
}