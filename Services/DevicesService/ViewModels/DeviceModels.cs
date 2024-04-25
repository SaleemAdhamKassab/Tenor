using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using Tenor.Services.SubsetsService.ViewModels;

namespace Tenor.Services.DevicesService.ViewModels
{
    public class DeviceViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
    }

    public class DeviceListViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
    }

    public class DeviceBindingModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }
    }

    public class DeviceFilterModel : GeneralFilterModel { }

    public class FileBytesModel
    {
        public byte[]? Bytes { get; set; } = null;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }

   
}