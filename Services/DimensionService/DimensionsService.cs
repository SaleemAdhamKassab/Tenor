using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Services.DimensionService.ViewModels.DimensionModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Services.DimensionService
{

    public interface IDimensionsService
    {
        ResultWithMessage GetDimLevelByDevice(int deviceId, string? searchQuery);
    }

    public class DimensionsService : IDimensionsService
    {
        private readonly TenorDbContext _db;
        private readonly IMapper _mapper;
        public DimensionsService(TenorDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public ResultWithMessage GetDimLevelByDevice(int deviceId, string? searchQuery)
        {
            List<DimensionViewModel> dims = new List<DimensionViewModel>();
            var query = _db.Dimensions.Include(x => x.DimensionLevels).ThenInclude(x => x.Level)
                .Where(x => x.DeviceId == deviceId);

            if (query == null || query.Count() == 0)
            {
                return new ResultWithMessage(null, "No data found");

            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(searchQuery.ToLower())
                       || x.DimensionLevels.Any(y => y.ColumnName.ToLower().Contains(searchQuery.ToLower()))
                       || x.Id.ToString() == searchQuery);
            }


            var result = query.ToList();

            foreach(var s in result)
            {
                DimensionViewModel dim = new DimensionViewModel();
                dim.Id = s.Id;
                dim.Name = s.Name;
                dim.HasChild = s.DimensionLevels!= null ? s.DimensionLevels.Count() > 0 ? true : false:false;
                dim.Levels = s.DimensionLevels.Where(x=>x.ParentId==null).Select(x => new DimLevelViewModel()
                {
                    Id = x.Id,
                    Name = x.ColumnName,
                    LevelId = x.LevelId,
                    LevelName = x.Level.Name,
                    Order = x.OrderBy,
                    IsFilter = x.Level.IsFilter,
                    IsLevel = x.Level.IsLevel,
                    HasChild = x.Childs!=null?x.Childs.Count() > 0 ? true :false :false,
                    SubLevels = x.Childs!=null ? getChilds(x.Id, new List<DimLevelViewModel>()) : null
                }).ToList();
               
                dims.Add(dim);
            }
            return new ResultWithMessage(dims, null);

        }

        private List<DimLevelViewModel> getChilds(int ? parentId, List<DimLevelViewModel> response)
        {
           
            var dimLev = _db.DimensionLevels.Include(x => x.Level).Where(x => x.ParentId == parentId).ToList();
            if(dimLev==null)
            {
                return null;
            }

            foreach(var s in dimLev)
            {
                
                response.Add(_mapper.Map<DimLevelViewModel>(s));

                if (s.Childs!=null)
                {
                    foreach(var t in s.Childs)
                    {
                       if(t.ParentId==null)
                        {
                            getChilds(t.Id, response);

                        }
                    }
                }
            }

           
            return response;

        }


    }
}
