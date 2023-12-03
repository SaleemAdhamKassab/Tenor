namespace Tenor.Dtos.KpiDto
{
    public class KpiExtraField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public dynamic Content { get; set; }
        public string? Url { get; set; }
    }
}
