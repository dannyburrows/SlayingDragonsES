using System;
using Nest;

namespace Elasticsearch.Models
{
    [ElasticsearchType(Name = "TreasureSearchModel", IdProperty = "Id")]
    public class Treasure
    {
        [String]
        public Guid Id { get; set; }
        [String]
        public string Name { get; set; }
        [Number(NumberType.Double)]
        public double Value { get; set; }
    }
}