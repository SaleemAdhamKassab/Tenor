using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
    public class Subset
    {
        [Key]
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public string Schema { get; set; }
        public int MaxDataDate { get; set; }
        public bool IsLoad { get; set; }
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public bool IsDeleted { get; set; }



        public virtual ICollection<Counter> Counters { get; set; }

        [ForeignKey("MainSet")]
        public int MainSetId { get; set; }
        public virtual MainSet MainSet { get; set; }
    }
}