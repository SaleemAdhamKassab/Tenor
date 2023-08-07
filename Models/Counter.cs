namespace Tenor.Models
{
    public class Counter
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }



        public long SubsetId { get; set; }
        public virtual Subset subset { get; set; }

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}