namespace Tenor.Dtos
{
    public class GeneralFilterModel
    {
        public string? SearchQuery { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? SortActive { get; set; } //Id
        public string? SortDirection { get; set; } //desc
    }

    public class DeviceFilterModel : GeneralFilterModel { }
    public class SubsetFilterModel : GeneralFilterModel { }
    public class CounterFilterModel : GeneralFilterModel { }
    public class KpiFilterModel : GeneralFilterModel { }
}
