using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services.CountersService.ViewModels
{
    public class CounterViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }
        public int SubsetId { get; set; }
        public string SubsetName { get; set; }
    }

    public class CounterListViewModel
    {
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string? Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }
        public int SubsetId { get; set; }
        public string SubsetName { get; set; }
    }

    public class CounterBindingModel
    {
        public int Id { get; set; }

        [Required]
        public string SupplierId { get; set; }


        [Required, StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 char")]
        public string Name { get; set; }

        public string? Code { get; set; }
        public string ColumnName { get; set; }
        public string RefColumnName { get; set; }
        public string Description { get; set; }
        public string Aggregation { get; set; }
        public bool IsDeleted { get; set; }
        public int SubsetId { get; set; }
    }

    public class CounterFilterModel : GeneralFilterModel 
    {
        public int ? SubsetId { get; set; }
        public int? DeviceId { get; set; }

    }
}