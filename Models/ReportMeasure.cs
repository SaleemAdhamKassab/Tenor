using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
	public class ReportMeasure
	{
		[Key]
		public int Id { get; set; }
		public string DisplayName { get; set; }
		public int ReportId { get; set; }
		public int OperationId { get; set; }

		public virtual Operation Operation { get; set; }
        public virtual Report Report { get; set; }
		public virtual ICollection<MeasureHaving> Havings { get; set; }
        public ReportMeasure() { }
		public ReportMeasure(int id, string displayName, int reportId, Operation operation)
        {
			Id= id;
			DisplayName= displayName;
			ReportId= reportId;
            Operation= operation;

        }
	}
}
