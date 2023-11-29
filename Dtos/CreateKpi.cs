using System.ComponentModel.DataAnnotations;
using static Tenor.Helper.Constant;

namespace Tenor.Dtos
{
    public class CreateKpi
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        public OperationDto Operation { get; set; }

    }

    public class OperationDto
    {
        public int Order { get; set; }
        public string? Value { get; set; }
        public enOPerationTypes Type { get; set; }
        public enAggregation Aggregation { get; set; }
        public int? CounterId { get; set; }
        public int? KpiId { get; set; }
        public int? FunctionId { get; set; }
        public int? OperatorId { get; set; }
        public int? ParentId { get; set; } //Self Join
        public  List<OperationDto>? Childs { get; set; }

    }

}
