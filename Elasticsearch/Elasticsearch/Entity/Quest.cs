using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Elasticsearch.Models;
using Nest;

namespace Elasticsearch.Entity
{
    public class Quest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [MaxLength(20)]
        [Required]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }
        [Required]
        public DateTime BeginDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public double StartLat { get; set; }
        [Required]
        public double StartLong { get; set; }
        [Required]
        public double EndLat { get; set; }
        [Required]
        public double EndLong { get; set; }
        [Required]
        public int Difficulty { get; set; }

        public virtual List< Treasure > Treasures { get; set; }
    }
}