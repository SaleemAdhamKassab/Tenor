using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class ReportFilter
	{
		[Key]
		public int Id { get; set; }
		public enLogicalOperator LogicalOperator { get; set; }
		public string? Value { get; set; }
		public int ReportId { get; set; }
		public int DimensionLevelId { get; set; }


		public DimensionLevel DimensionLevel { get; set; }
        public Report Report { get; set; }

		public ReportFilter() { }
		public ReportFilter(int id, enLogicalOperator logicalOperator, string? value, int reportId, int dimensionLevelId)
		{
			Id = id;
			LogicalOperator = logicalOperator;
			Value = value;
			ReportId = reportId;
			DimensionLevelId = dimensionLevelId;
		}

    }
}