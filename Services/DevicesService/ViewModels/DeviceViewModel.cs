using Tenor.Models;

namespace Tenor.Services.DevicesService.ViewModels
{
    public class DeviceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string SupplierId { get; set; }

        //navigation properties
        public int? ParentId { get; set; }
        public string ParentName { get; set; }

        public List<Subset> Subsets { get; set; }
    }
}