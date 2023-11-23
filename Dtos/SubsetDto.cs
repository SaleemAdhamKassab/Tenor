namespace Tenor.Dtos
{
    public class SubsetDto
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string RefTableName { get; set; }
        public string SchemaName { get; set; }
        public int MaxDataDate { get; set; }
        public bool IsLoad { get; set; }
        public string DataTS { get; set; }
        public string IndexTS { get; set; }
        public string DbLink { get; set; }
        public string RefDbLink { get; set; }
        public bool IsDeleted { get; set; }
        public int DeviceId { get; set; }
    }
}