using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services.DevicesService.ViewModels
{
    public class DeviceListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DeviceViewModel : DeviceListViewModel
    {
        public string SupplierId { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public List<Subset> Subsets { get; set; }
    }

    public class DeviceBindingModel : DeviceListViewModel
    {
        public string SupplierId { get; set; }
        public int? ParentId { get; set; }
    }

    public class DeviceFilterModel : GeneralFilterModel { }
}