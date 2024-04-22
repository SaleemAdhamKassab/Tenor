using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Helper.Constant;
using static Tenor.Services.SharedService.ViewModels.SharedModels;
namespace Tenor.Services.ReportService.ViewModels
{
    public class ReportModels
    {
        public class CreateReport
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DeviceId { get; set;}
            public List<ReportMeasureDto> Measures { get; set;}
            public List<ReportLevelDto> Levels { get; set; }
            public List<ReportFilterDto> Filters { get; set; }
            public List<ExtraFieldValue>? ReportFields { get; set; }
            public bool IsPublic { get; set; }
            public int? ChildId { get; set; }
            public string ? CreatedBy { get; set; }
            public DateTime ? CreatedDate { get; set; }

        }
        public class ReportMeasureDto
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public OperationBinding Operation { get; set; }
            public List<Having> Havings { get; set; }
        }
        public class Having
        {
            public int Id { get; set; }
            public int? OperatorId { get; set; }
            public enLogicalOperator LogicOpt { get; set; }
            public string? Value { get; set; }
        }
        public class ReportLevelDto
        {
            public int Id { get; set; }
            public int DisplayOrder { get; set; }
            public enSortDirection SortDirection { get; set; }
            public int DimensionLevelId { get; set; }
        }
        public class ReportFilterDto
        {
            public int Id { get; set; }
            public enLogicalOperator LogicalOperator { get; set; }
            public string[] ? Value { get; set; }
            public int DimensionLevelId { get; set; }
            public bool IsMandatory { get; set; }

        }
    }
}
