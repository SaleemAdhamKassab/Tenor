using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Helper.Constant;
using static Tenor.Services.SharedService.ViewModels.SharedModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
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
            public List<ContainerOfFilter> ContainerOfFilters { get; set; }
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

        public class HavingViewModel
        {

            public int Id { get; set; }
            public int? OperatorId { get; set; }
            public string ? OperatorName { get; set; }
            public enLogicalOperator LogicOpt { get; set; }
            public string ? LogicOptName { get; set; }
            public string? Value { get; set; }
        }
        public class ReportLevelDto
        {
            public int Id { get; set; }
            public int DisplayOrder { get; set; }
            public enSortDirection SortDirection { get; set; }
            public int LevelId { get; set; }
        }
        public class ReportLevelViewModel
        {
            public int Id { get; set; }
            public int DisplayOrder { get; set; }
            public enSortDirection SortDirection { get; set; }
            public string SortDirectionName { get; set; }
            public int LevelId { get; set; }
            public string LevelName { get; set; }
            public bool IsLevel { get; set; }
            public bool IsFilter { get; set; }
        }
        public class ReportFilterDto
        {
            public int Id { get; set; }
            public enLogicalOperator LogicalOperator { get; set; }
            public string ? LogicalOperatorName { get; set; }
            public string[] ? Value { get; set; }
            public int DimensionLevelId { get; set; }
            public bool IsMandatory { get; set; }

        }
        public class ContainerOfFilter
        {
            public int Id { get; set; }
            public enLogicalOperator LogicalOperator { get; set; }
            public string ? LogicalOperatorName { get; set; }
            public List<ReportFilterDto> ReportFilters { get; set; }
        }
        public class MeasureViewModel
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public OperationDto Operation { get; set; }
            public List<HavingViewModel> Havings { get; set; }

        }
        public class ReportViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DeviceId { get; set; }
            public string DeviceName { get; set; }
            public bool IsPublic { get; set; }
            public string? CreatedBy { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int? ChildId { get; set; }
            public List<MeasureViewModel> Measures { get; set; }
            public List<ReportLevelViewModel> Levels { get; set; }
            public List<ReportFieldValueViewModel>? ReportFields { get; set; }
            public List<ContainerOfFilter> ContainerOfFilters { get; set; }

        }
        public class ReportFieldValueViewModel
        {
            public int Id { get; set; }
            public int FieldId { get; set; }
            public string Type { get; set; }
            public string FieldName { get; set; }
            public object Value { get; set; }
        }

    }
}
