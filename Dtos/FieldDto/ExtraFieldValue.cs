using System.Dynamic;

namespace Tenor.Dtos.FieldDto
{
    public class ExtraFieldValue
    {
        public int FieldId { get;set; }
        public dynamic Value { get;set; }
    }
}
