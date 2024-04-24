using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Tenor.Helper.Constant;

namespace Tenor.Models
{
	public class ReportLevel
	{
		[Key]
		public int Id { get; set; }
		public int DisplayOrder { get; set; }
		public enSortDirection SortDirection { get; set; }
		public int ReportId { get; set; }
		[ForeignKey("Level")]
		public int LevelId { get; set; }


		public virtual Level Level { get; set; }
        public virtual Report Report { get; set; }

		public ReportLevel() { }
		public ReportLevel(int id, int displayOrder, enSortDirection sortDirection, int reportId, int levelId)
		{
			Id = id;
            DisplayOrder=displayOrder;
			SortDirection=sortDirection;
			ReportId = reportId;
            LevelId = levelId;

        }

    }
}