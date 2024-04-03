using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenor.Models
{
	public class Report
	{
		[Key]
		public int Id { get; set; }

		[Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
		public string Name { get; set; }

		[Required, MaxLength(50)]
		public string CreatedBy { get; set; }

		[Required, MaxLength(50)]
		public string ChangedBy { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime ChangedDate { get; set; }

		public bool IsPublic { get; set; }

		[ForeignKey("Child")]
		public int? ChildId { get; set; }
		public virtual Report Child { get; set; }

		public int DeviceId { get; set; }
		public Device Device { get; set; }


		public virtual ICollection<ReportFieldValue> ReportFieldValues { get; set; }
		public virtual ICollection<ReportLevel> Levels { get; set; }
		public virtual ICollection<ReportFilter> Filters { get; set; }
		public virtual ICollection<ReportMeasure> Measures { get; set; }
	}
}