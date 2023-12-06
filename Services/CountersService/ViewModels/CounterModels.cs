using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services.CountersService.ViewModels
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

    public class CounterBindingModel : CounterListViewModel
    {
        public int SubsetId { get; set; }
        public int TechnologyId { get; set; }
        public int GroupDeviceTypeId { get; set; }
        public int GroupCategoryId { get; set; }
        public int GroupLevelId { get; set; }
    }

    public class CounterViewModel : CounterBindingModel
    {
        public string SubsetName { get; set; }
    }

    public class SubsetFilterModel : GeneralFilterModel { }
}