using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
	public class ReportMeasure
	{
		[Key]
		public int ID { get; set; }

		public string DisplayName { get; set; }

		public int ReportID { get; set; }
		public Report Report { get; set; }

		public int OperationID { get; set; }
		public Operation Operation { get; set; }
	}
}
