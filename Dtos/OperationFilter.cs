namespace Tenor.Dtos
{
    public class OperationFilter
    {
        public string? SearchQuery { get; set; }
        public int? DeviceId { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
