using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class ReportFilter
	{
		[Key]
		public int Id { get; set; }
		public enLogicalOperator LogicalOperator { get; set; }
		public string? Value { get; set; }
		[ForeignKey("FilterContainer")]
		public int FilterContainerId { get; set; }
		[ForeignKey("Level")]
		public int LevelId { get; set; }
		public bool IsMandatory { get; set; }


        public virtual Level Level { get; set; }
        public virtual ReportFilterContainer FilterContainer { get; set; }

		public ReportFilter() { }
		public ReportFilter(int id, enLogicalOperator logicalOperator, string? value, int filterContainerId, int levelId)
		{
			Id = id;
			LogicalOperator = logicalOperator;
			Value = value;
            FilterContainerId = filterContainerId;
			LevelId = levelId;
		}

    }
}