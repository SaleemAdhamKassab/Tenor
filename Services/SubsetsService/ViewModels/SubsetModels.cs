using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Services.SubsetsService.ViewModels
{
    public class SubsetViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public string SchemaName { get; set; }
        public string RefSchema { get; set; }
        public int? MaxDataDate { get; set; }
        public bool IsLoad { get; set; } = false;
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public int? GranularityPeriod { get; set; }
        public string DimensionTable { get; set; }
        public string JoinExpression { get; set; }
        public char? StartChar { get; set; }
        public string? FactDimensionReference { get; set; }
        public int? LoadPriorety { get; set; }
        public string? SummaryType { get; set; }
        public bool IsDeleted { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public List<SubsetExtraFieldValueViewModel> ExtraFields { get; set; }
    }

    public class SubsetListViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public string SchemaName { get; set; }
        public string RefSchema { get; set; }
        public int? MaxDataDate { get; set; }
        public bool IsLoad { get; set; } = false;
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public int? GranularityPeriod { get; set; }
        public string? SummaryType { get; set; }
        public bool IsDeleted { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
    }

    public class SubsetBindingModel
    {
        public int Id { get; set; }

        [Required]
        public string SupplierId { get; set; }

        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string TableName { get; set; }

        public string RefTableName { get; set; }
        public string SchemaName { get; set; }
        public string RefSchema { get; set; }
        public int? MaxDataDate { get; set; }
        public bool IsLoad { get; set; } = false;
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public int? GranularityPeriod { get; set; }
        public string DimensionTable { get; set; }
        public string JoinExpression { get; set; }
        public char? StartChar { get; set; }
        public string? FactDimensionReference { get; set; }
        public int? LoadPriorety { get; set; }
        public string? SummaryType { get; set; }
        public bool IsDeleted { get; set; }

        [Required]
        public int DeviceId { get; set; }

        public List<ExtraFieldValue>? ExtraFields { get; set; }
    }

    public class SubsetFilterModel : GeneralFilterModel
    {
        public string? DeviceId { get; set; }

    }

    public class SubsetExtraField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public dynamic Content { get; set; }
        public string? Url { get; set; }
    }

    public class SubsetExtraFieldValueViewModel
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string Type { get; set; }
        public string FieldName { get; set; }
        public object Value { get; set; }
    }
}