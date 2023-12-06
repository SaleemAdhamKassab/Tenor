using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services.SubsetsService.ViewModels
{
    public class SubsetListViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public bool IsLoad { get; set; }
        public string Technology { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SubsetViewModel : SubsetListViewModel
    {
        public string SchemaName { get; set; }
        public string RefSchema { get; set; }
        public int? MaxDataDate { get; set; }
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public int? GranularityPeriod { get; set; }
        public string DimensionTable { get; set; }
        public string JoinExpression { get; set; }
        public char? StartChar { get; set; }
        public string FactDimensionReference { get; set; }
        public int? TechnologyId { get; set; }
        public int? LoadPriorety { get; set; }
        public string SummaryType { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public List<Counter> Counters { get; set; }
    }

    public class SubsetBindingModel : SubsetListViewModel
    {
        public string SchemaName { get; set; }
        public string RefSchema { get; set; }
        public int MaxDataDate { get; set; }
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public int? GranularityPeriod { get; set; }
        public string DimensionTable { get; set; }
        public string JoinExpression { get; set; }
        public char? StartChar { get; set; }
        public string FactDimensionReference { get; set; }
        public int? TechnologyId { get; set; }
        public int? LoadPriorety { get; set; }
        public string SummaryType { get; set; }
        public int DeviceId { get; set; }
    }

    public class SubsetFilterModel : GeneralFilterModel { }
}
