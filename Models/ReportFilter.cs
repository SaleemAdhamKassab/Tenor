using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class ReportFilter
	{
		[Key]
		public int ID { get; set; }

		public enLogicalOperator LogicalOperator { get; set; }

		public string? Value { get; set; }

		public int ReportID { get; set; }
		public Report Report { get; set; }

		public int DimensionLevelID { get; set; }
		public DimensionLevel DimensionLevel { get; set; }
	}
}