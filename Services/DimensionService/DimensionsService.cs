using Tenor.Data;
using Tenor.Dtos;
using static Tenor.Services.DimensionService.ViewModels.DimensionModels;

namespace Tenor.Services.DimensionService
{

    public interface IDimensionsService
    {
        ResultWithMessage GetDimLevelByDevice(int deviceId, string ? searchQuery);
    }

    public class DimensionsService : IDimensionsService
    {
        private readonly TenorDbContext _db;
        public DimensionsService(TenorDbContext db)
        {
            _db = db;
        }
        public ResultWithMessage GetDimLevelByDevice(int deviceId, string? searchQuery)
        {
            var query = _db.Dimensions.Where(x => x.DeviceId == deviceId);

            if (query == null || query.Count() == 0)
            {
                return new ResultWithMessage(null, "No data found");

            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query= query.Where(x=>x.Name.ToLower().Contains(searchQuery.ToLower())
                       || x.DimensionLevels.Any(y=>y.ColumnName.ToLower().Contains(searchQuery.ToLower()))
                       || x.Id.ToString()==searchQuery);
            }

            var result = query.Select(x => new DimensionViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                HasChild=x.DimensionLevels.Count() > 0,
                Levels=x.DimensionLevels.Select(x=> new DimLevelViewModel()
                {
                    DimLevelId=x.Id,
                    DimLevelColumnName=x.ColumnName,
                    LevelId=x.LevelId,
                    LevelName=x.Level.Name,
                    Order=x.OrderBy

                }).ToList()
            }).ToList();

            return new ResultWithMessage(result, null);

        }
    }
}
