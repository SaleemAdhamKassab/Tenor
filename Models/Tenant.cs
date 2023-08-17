using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class Tenant
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
    }
}