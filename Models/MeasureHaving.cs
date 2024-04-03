using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class MeasureHaving
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("Operator")]
		public int ? OperatorId { get; set; }
		public int MeasureId { get; set; }
		public enLogicalOperator LogicOpt { get; set; }
		public string ? Value { get; set; }

        public virtual ReportMeasure ReportMeasure { get; set; }
		public virtual Operator Operator { get; set; }	
	}
}
