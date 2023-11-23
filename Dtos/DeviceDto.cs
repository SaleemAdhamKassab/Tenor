namespace Tenor.Dtos
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }
    }
}