using Tenor.Data;
using Tenor.Dtos;

namespace Tenor.Services.DataServices
{
    public interface IQueryBuilderService
    {
        public string getFilterOptionsQuery(int levelId, string searchQuery, int pageIndex, int pageSize);
    }
    public class QueryBuilderService: IQueryBuilderService
    {
        private readonly TenorDbContext _db;
        public QueryBuilderService(TenorDbContext db)
        {
            _db = db;
        }

        public string getFilterOptionsQuery(int levelId, string searchQuery, int pageIndex, int pageSize)
        {
            var queryObj = _db.Levels.Where(x => x.Id == levelId).Select(x => x.DimensionLevels.Select(y => new
            {
                y.ColumnName,
                y.OrderBy,
                y.Dimension.TableName
            })).SelectMany(x => x).FirstOrDefault();
            var filterOptionsQuery = "SELECT DISTINCT"
                	                + $" {queryObj.ColumnName}"
                	                + $" FROM {queryObj.TableName}"
                	                + $" WHERE LOWER({queryObj.ColumnName}) like '{searchQuery.ToLower()}%'"
                 	                + $" ORDER BY {queryObj.OrderBy}"
                 	                + $" OFFSET {pageIndex * pageSize}"
                 	                + $" ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return filterOptionsQuery.ToString();
        }
    }
}
