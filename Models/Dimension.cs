﻿using OfficeOpenXml.Drawing.Style.Coloring;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Dimension
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        [ForeignKey("Device")]
        public int ? DeviceId { get; set; }

        public virtual ICollection<DimensionJoiner> DimensionJoiners { get; set; } 
        public virtual ICollection<DimensionLevel> DimensionLevels { get; set; }
        public virtual Device Device { get; set; }

    }
}
