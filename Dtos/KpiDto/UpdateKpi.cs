using System.ComponentModel.DataAnnotations;

namespace Tenor.Dtos.KpiDto
{
    public class UpdateKpi
    {
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
        [Required]
        public string Name { get; set; }
        public OperationDto Operation { get; set; }
    }
}
