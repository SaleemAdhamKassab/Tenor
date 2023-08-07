namespace Tenor.Models
{
    public class Subset
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public string Schema { get; set; }
        public int MaxDataDate { get; set; }
        public bool IsLoad { get; set; }
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public bool IsDeleted { get; set; }



        public virtual List<Counter> Counters { get; set; }

        public  int MainSetId { get; set; }
        public virtual MainSet MainSet { get; set; }

        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
