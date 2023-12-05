namespace Tenor.Services.DevicesService.ViewModels
{
    public class DeviceBindingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string SupplierId { get; set; }
        public int? ParentId { get; set; }
    }
}