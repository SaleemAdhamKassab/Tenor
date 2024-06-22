using Tenor.Models;
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
        public class ReportSubqueryModel
        {
            public int DeviceId { get; set; }
            public string? SubsetTableName { get; set; }
            public List<ReportSubqueryMeasure>? ReportSubqueryMeasures { get; set; }
            public List<ReportSubqueryDimension> ReportSubqueryDimensions { get; set; } = new List<ReportSubqueryDimension>();
            public List<ReportFilterContainerSubqueryModel> FilterContainers { get; set; } = new List<ReportFilterContainerSubqueryModel>();
        }
        public class ReportSubqueryMeasure
        {
            public int CounterId { get; set; }
            public string? Aggregation { get; set; }
            public string? ColumnName { get; set; }
        }
        public class ReportSubqueryDimension
        {
            public string? DimensionTableName { get; set; }
            public List<DimensionJoiner> DimensionJoiners { get; set; }
            public List<ReportLevelSubquery>? LevelColumns { get; set; }
            public List<ReportFilterContainerSubqueryModel> FilterContainers { get; set; }
            
        }
        public class ReportFilterContainerSubqueryModel
        {
            public string? LogicalOperator { get; set; }
            public List<ReportFilterWithValue> Filters { get; set; } = new List<ReportFilterWithValue> ();
        }
        public class ReportFilterWithValue
        {
            public string? LogicalOperator { get; set; }
            public string? FilterTableName { get; set; }
            public string FilterColumnName { get; set; }
            public List<string>? FilterValues { get; set; }
            public string? Type { get; set; }
            public bool isMandatory { get; set; }
        }
        public class ReportLevelSubquery
        {
            public string? LevelName { get; set; }
            public string? LevelColumn { get; set; }
            public string? LevelOrderByColumn { get; set; }
            public string? SortDirection { get; set; }
        }
        public class QueryWithSize
        {
            public string? Sql { get; set; }
            public string? CountSql { get; set; }
        }
    }
}
