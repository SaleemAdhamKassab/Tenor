using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
	public class ReportMeasure
	{
		[Key]
		public int Id { get; set; }
		public string DisplayName { get; set; }

		public int ReportId { get; set; }
		public Report Report { get; set; }

		public int OperationId { get; set; }
		public Operation Operation { get; set; }
	}
}
