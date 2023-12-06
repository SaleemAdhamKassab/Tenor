using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services.CountersService
{
    public class CounterListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }
        public string SupplierId { get; set; }
    }

    public class CounterViewModel : CounterListViewModel
    {

        public int TechnologyId { get; set; }
        public int GroupDeviceTypeId { get; set; }
        public int GroupCategoryId { get; set; }
        public int GroupLevelId { get; set; }
        public int SubsetId { get; set; }
        public string SubsetName { get; set; }

        public List<Operation> Operations { get; set; }
        public List<CounterFieldValue> CounterFieldValues { get; set; }
    }

    public class CounterBindingModel : CounterListViewModel
    {
        public int TechnologyId { get; set; }
        public int GroupDeviceTypeId { get; set; }
        public int GroupCategoryId { get; set; }
        public int GroupLevelId { get; set; }
        public int SubsetId { get; set; }
    }

    public class SubsetFilterModel : GeneralFilterModel { }
}