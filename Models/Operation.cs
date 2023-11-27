using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
    public class Operation
    {
        [Key]
        public int Id { get; set; }
        public int Order { get; set; }
        public string? Value { get; set; }
        public enOPerationTypes Type { get; set; }
        public enAggregation Aggregation { get; set; }

        [ForeignKey("Counter")]
        public int? CounterId { get; set; }

        [ForeignKey("Kpi")]
        public int? KpiId { get; set; }

        [ForeignKey("Function")]
        public int? FunctionId { get; set; }

        [ForeignKey("Operator")]
        public int? OperatorId { get; set; }

        [ForeignKey("Parent")]
        public int? ParentId { get; set; } //Self Join


        public virtual ICollection<Operation>? Childs { get; set; }
        public virtual Function? Function { get; set; }
        public virtual Counter? Counter { get; set; }
        public virtual Kpi? Kpi { get; set; }
        public virtual Operator? Operator { get; set; }
        public virtual Operation? Parent { get; set; }
    }
}