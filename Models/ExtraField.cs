using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
    public class ExtraField
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }
        [Required]
        public fieldTypes Type { get; set; }
        public string? Content { get; set; }
        public string? Url { get; set; }

        public bool IsMandatory { get; set; }
        public virtual ICollection<KpiField> KpiFields { get; set; }
        public virtual ICollection<CounterField> CounterFields { get; set; }
        public virtual ICollection<ReportField> ReportFields { get; set; }
        public virtual ICollection<DashboardField> DashboardFields { get; set; }
        public virtual ICollection<SubsetField> SubsetFields { get; set; }


        public ExtraField() { }
        public ExtraField(CreateExtraFieldViewModel input)
        {
            Id = input.Id;
            Name=input.Name;
            Type=input.Type;
            Content=input.Content;
            Url=input.Url;        
        }

        public void addKpiField(ExtraField input)
        {
            if (input.KpiFields is null)
            {
                input.KpiFields = new List<KpiField>();

            }
            input.KpiFields.Add(new KpiField(input.Id));

        }
        public void addReportField(ExtraField input)
        {
            if (input.ReportFields is null)
            {
                input.ReportFields = new List<ReportField>();
            }
            input.ReportFields.Add(new ReportField(input.Id));
        }
        public void addDashboardField(ExtraField input)
        {
            if (input.DashboardFields is null)
            {
                input.DashboardFields = new List<DashboardField>();
            }
            input.DashboardFields.Add(new DashboardField(input.Id));
        }
    }
}
