using static Tenor.Helper.Constant;
using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;

public class CreateExtraFieldViewModel
{

    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
    public string Name { get; set; }
    [Required]
    public fieldTypes Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public bool  IsForKpi { get; set; }
    public bool IsForReport { get; set; }
    public bool IsForDashboard { get; set; }
    public bool IsMandatory { get; set; }

}

public class ExtraFieldViewModel
{

    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
    public string Name { get; set; }
    [Required]
    public fieldTypes Type { get; set; }
    public string TypeName { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public bool IsForKpi { get; set; }
    public bool IsForReport { get; set; }
    public bool IsForDashboard { get; set; }
    public bool IsMandatory { get; set; }

}

public class ExtraFieldFilter: GeneralFilterModel
{
    public bool? IsKpi { get; set; }
    public bool? IsReport { get; set; }
    public bool? IsDashboard { get; set; }
}