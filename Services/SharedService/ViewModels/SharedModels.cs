using static Tenor.Helper.Constant;

namespace Tenor.Services.SharedService.ViewModels
{
    public class SharedModels
    {
        public class ExtraFieldValue
        {
            public int Id { get; set; }
            public int FieldId { get; set; }
            public dynamic? Value { get; set; }
        }
        public class OperationBinding
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string? Value { get; set; }
            public enOPerationTypes Type { get; set; }
            public enAggregation Aggregation { get; set; }
            public int? CounterId { get; set; }
            public int? KpiId { get; set; }
            public int? FunctionId { get; set; }
            public int? OperatorId { get; set; }
            public int? ParentId { get; set; } //Self Join
            public List<OperationBinding>? Childs { get; set; }

        }
    }
}
