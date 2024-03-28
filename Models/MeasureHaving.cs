using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class MeasureHaving
	{
		[Key]
		public int ID { get; set; }

		public enLogicalOperator LogicalOperator { get; set; }

		public string Name { get; set; }

		public int MeasureID { get; set; }
		public ReportMeasure ReportMeasure { get; set; }
	}
}
