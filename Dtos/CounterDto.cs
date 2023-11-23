namespace Tenor.Dtos
{
    public class CounterDto
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }
        public int SubsetId { get; set; }
    }
}