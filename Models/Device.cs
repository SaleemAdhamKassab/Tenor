﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string SupplierId { get; set; } //set Id from Hawawi
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public bool IsDeleted { get; set; } //Without Gloable filter


        public virtual ICollection<Subset> Subsets { get; set; }           
        public virtual ICollection<Device> Childs { get; set; }
        public virtual ICollection<TenantDevice> TenantDevices { get; set; }
        public virtual Device Parent { get; set; }
    }
}