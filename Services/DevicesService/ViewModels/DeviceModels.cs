using Tenor.Dtos;

namespace Tenor.Services.DevicesService.ViewModels
{
    public class DeviceListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string SupplierId { get; set; }
        public int? ParentId { get; set; }
    }

    public class DeviceBindingModel : DeviceListViewModel { }

    public class DeviceViewModel : DeviceBindingModel
    {
        public string ParentName { get; set; }
    }

    public class DeviceFilterModel : GeneralFilterModel { }
}