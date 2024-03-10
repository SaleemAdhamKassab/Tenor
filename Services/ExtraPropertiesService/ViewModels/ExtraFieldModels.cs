using static Tenor.Helper.Constant;
using System.ComponentModel.DataAnnotations;

public class CreateExtraFieldViewModel
{

    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
    public string Name { get; set; }
    [Required]
    public fieldTypes Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
}