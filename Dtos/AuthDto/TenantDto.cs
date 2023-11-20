namespace Tenor.Dtos.AuthDto
{
    public class TenantDto
    {
        public string userName { get; set; }
        public List<TenantAccess> tenantAccesses { get; set; }
    }

    public class TenantAccess
    {
        public string tenantName { get; set; }
        public List<string> RoleList { get; set; }= new List<string>();
    }
}
