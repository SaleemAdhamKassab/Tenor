namespace Tenor.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Description { get; set; }



        public virtual List<MainSet> MainSets { get; set; }
    }
}