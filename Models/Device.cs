using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        [Required,StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }
        
        public string Description { get; set; }

        public bool IsDeleted { get; set; } //Without Gloable filter


        [Required]
        public string SupplierId { get; set; } //set Id from Hawawi
        

        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public virtual Device Parent { get; set; }


        public virtual ICollection<Subset> Subsets { get; set; }           
        public virtual ICollection<Device> Childs { get; set; }
        public virtual ICollection<TenantDevice> TenantDevices { get; set; }
        public virtual ICollection<Kpi> Kpis { get; set; }
        public virtual ICollection<Dimension> Dimensions { get; set; }
        public virtual ICollection<ExtraField> ExtraFields { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}