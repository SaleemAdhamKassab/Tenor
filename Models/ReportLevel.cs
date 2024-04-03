using System.ComponentModel.DataAnnotations;
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
		public int DimensionLevelId { get; set; }


		public virtual DimensionLevel DimensionLevel { get; set; }
        public virtual Report Report { get; set; }

		public ReportLevel() { }
		public ReportLevel(int id, int displayOrder, enSortDirection sortDirection, int reportId, int dimensionLevelId)
		{
			Id = id;
            DisplayOrder=displayOrder;
			SortDirection=sortDirection;
			ReportId = reportId;
			DimensionLevelId = dimensionLevelId;

        }

    }
}