using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
	public class ReportLevel
	{
		[Key]
		public int ID { get; set; }

		public int DisplayOrder { get; set; }

		public string SortDirection { get; set; }


		public int ReportID { get; set; }
		public Report Report { get; set; }

		public int DimensionLevelID { get; set; }
		public DimensionLevel DimensionLevel { get; set; }
	}
}