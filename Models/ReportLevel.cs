using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class ReportLevel
	{
		[Key]
		public int Id { get; set; }
		public int DisplayOrder { get; set; }
		public enSortDirection SortDirection { get; set; }

		public int ReportID { get; set; }
		public Report Report { get; set; }

		public int DimensionLevelID { get; set; }
		public DimensionLevel DimensionLevel { get; set; }
	}
}