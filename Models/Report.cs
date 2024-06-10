using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Tenor.Services.ReportService.ViewModels.ReportModels;

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

		[MaxLength(50)]
		public string ? ChangedBy { get; set; }

		public DateTime CreatedDate { get; set; }

		public DateTime ? ChangedDate { get; set; }

		public bool IsPublic { get; set; }

		[ForeignKey("Child")]
		public int? ChildId { get; set; }
		public int DeviceId { get; set; }
		public bool IsDeleted { get; set; }

		public virtual ICollection<ReportFieldValue> ReportFieldValues { get; set; }
		public virtual ICollection<ReportLevel> Levels { get; set; }
        public virtual ICollection<ReportFilterContainer> FilterContainers { get; set; }
        public virtual ICollection<ReportMeasure> Measures { get; set; }
        public virtual Report Child { get; set; }
        public Device Device { get; set; }



        public Report() { }
		public Report(CreateReport input)
		{
			Id = input.Id;
			Name=input.Name;
			IsPublic=input.IsPublic;
			ChildId = input.ChildId;
			DeviceId=input.DeviceId;
			CreatedBy = input.CreatedBy;
			CreatedDate = (DateTime) input.CreatedDate;
			
		}
        public Report(CreateReport input , string? changedBy , DateTime? changedDate)
        {
            Id = input.Id;
            Name = input.Name;
            IsPublic = input.IsPublic;
            ChildId = input.ChildId;
            DeviceId = input.DeviceId;
            CreatedBy = input.CreatedBy;
            CreatedDate = (DateTime)input.CreatedDate;
			ChangedBy = changedBy;
			ChangedDate = changedDate;
        }
    }
}