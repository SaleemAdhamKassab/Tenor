using AutoMapper;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text.RegularExpressions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using Tenor.Services.SubsetsService.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Tenor.Helper.Constant;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Services.CountersService
{
    public interface ICountersService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter(object filter);
        ResultWithMessage add(CounterBindingModel model);
        ResultWithMessage edit(CounterBindingModel CounterDto);
        ResultWithMessage delete(int id);
        Task<ResultWithMessage> GetExtraFields();

    }

    public class CountersService : ICountersService
    {
        private readonly TenorDbContext _db;
        private readonly IMapper _mapper;
        public CountersService(TenorDbContext tenorDbContext, IMapper mapper)
        {
            _db = tenorDbContext;
            _mapper = mapper;
        } 
     
        public ResultWithMessage getById(int id)
        {
            CounterViewModel Counter = _db.Counters
                .Select(e => new CounterViewModel()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Code = e.Code,
                    ColumnName = e.ColumnName,
                    RefColumnName = e.RefColumnName,
                    Description = e.Description,
                    Aggregation = e.Aggregation,
                    IsDeleted = e.IsDeleted,
                    SupplierId = e.SupplierId,
                    SubsetId = e.SubsetId,
                    SubsetName = e.Subset.Name
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(Counter, "");
        }

        public ResultWithMessage getByFilter(object counterFilter)
        {
            try
            {
                //--------------------------Data Source---------------------------------------
                IQueryable<Counter> query = _db.Counters.Where(e => true);
                List<string> counterFields = _db.CounterFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
                //------------------------------Conver Dynamic filter--------------------------
                dynamic data = JsonConvert.DeserializeObject<dynamic>(counterFilter.ToString());
                CounterFilterModel counterFilterModel = new CounterFilterModel()
                {
                    SearchQuery = counterFilter.ToString().Contains("SearchQuery") ? data["SearchQuery"] : data["searchQuery"],
                    PageIndex = counterFilter.ToString().Contains("PageIndex") ? data["PageIndex"] : data["pageIndex"],
                    PageSize = counterFilter.ToString().Contains("PageSize") ? data["PageSize"] : data["pageSize"],
                    SortActive = counterFilter.ToString().Contains("SortActive") ? data["SortActive"] : data["sortActive"],
                    SortDirection = counterFilter.ToString().Contains("SortDirection") ? data["SortDirection"] : data["sortDirection"],                   
                    SubsetId = counterFilter.ToString().Contains("SubsetId") ? Convert.ToString(data["SubsetId"]) : Convert.ToString(data["subsetId"]),
                    DeviceId = (counterFilter.ToString().Contains("DeviceId") ? data["DeviceId"] : data["deviceId"]) != "" ?
                               (counterFilter.ToString().Contains("DeviceId") ? data["DeviceId"] : data["deviceId"]) : null

                };
                //--------------------------------Filter and conver data to VM----------------------------------------------
                IQueryable<Counter> fiteredData = getFilteredData(data, query, counterFilterModel, counterFields);
                //-------------------------------Data sorting and pagination------------------------------------------
                var queryViewModel = convertCountersToViewModel(fiteredData);
                return sortAndPagination(counterFilterModel, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }

        public ResultWithMessage add(CounterBindingModel model)
        {
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            Counter counter = new()
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                ColumnName = model.ColumnName,
                RefColumnName = model.RefColumnName,
                Description = model.Description,
                Aggregation = model.Aggregation,
                IsDeleted = model.IsDeleted,
                SupplierId = model.SupplierId,
                SubsetId = model.SubsetId
            };

            try
            {
                _db.Add(counter);
                _db.SaveChanges();

                CounterViewModel counterViewModel = new()
                {
                    Id = counter.Id,
                    Name = counter.Name,
                    Code = counter.Code,
                    ColumnName = counter.ColumnName,
                    RefColumnName = counter.RefColumnName,
                    Description = counter.Description,
                    Aggregation = counter.Aggregation,
                    IsDeleted = counter.IsDeleted,
                    SupplierId = counter.SupplierId,
                    SubsetId = counter.SubsetId,
                    SubsetName = counter.Subset.Name
                };

                return new ResultWithMessage(counterViewModel, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage edit(CounterBindingModel model)
        {
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            Counter counter = _db.Counters.Find(model.Id);

            if (counter is null)
                return new ResultWithMessage(null, $"Not found Counter with id: {model.Id}");

            counter.Id = model.Id;
            counter.Name = model.Name;
            counter.Code = model.Code;
            counter.ColumnName = model.ColumnName;
            counter.RefColumnName = model.RefColumnName;
            counter.Description = model.Description;
            counter.Aggregation = model.Aggregation;
            counter.IsDeleted = model.IsDeleted;
            counter.SupplierId = model.SupplierId;
            counter.SubsetId = model.SubsetId;

            try
            {
                _db.Update(counter);
                _db.SaveChanges();

                CounterViewModel counterViewModel = new()
                {
                    Id = counter.Id,
                    Name = counter.Name,
                    Code = counter.Code,
                    ColumnName = counter.ColumnName,
                    RefColumnName = counter.RefColumnName,
                    Description = counter.Description,
                    Aggregation = counter.Aggregation,
                    IsDeleted = counter.IsDeleted,
                    SupplierId = counter.SupplierId,
                    SubsetId = counter.SubsetId,
                    SubsetName = counter.Subset.Name
                };

                return new ResultWithMessage(counterViewModel, "");
            }

            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage delete(int id)
        {
            Counter counter = _db.Counters.Find(id);

            if (counter is null)
                return new ResultWithMessage(null, $"Not found Counter with id: {id}");

            try
            {
                counter.IsDeleted = true;
                _db.Update(counter);
                _db.SaveChanges();

                return new ResultWithMessage(null, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(null, e.Message);
            }
        }
        public async Task<ResultWithMessage> GetExtraFields()
        {
            var extraFields = _mapper.Map<List<KpiExtraField>>(_db.CounterFields.Where(x => x.IsActive).Include(x => x.ExtraField).ToList());
            return new ResultWithMessage(extraFields, null);

        }
        private IQueryable<Counter> getFilteredData(dynamic data, IQueryable<Counter> query, CounterFilterModel counterFilterModel, List<string> counterFields)
        {
            //Build filter for extra field
            List<Filter> filters = new List<Filter>();
            foreach (var s in counterFields)
            {
                Filter filter = new Filter();
                object property = data[s]!=null? data[s]: data[char.ToLower(s[0]) + s.Substring(1)];
                if (property != null)
                {
                    if (!string.IsNullOrEmpty(property.ToString()))
                    {
                        filter.key = s;
                        filter.values = Regex.Replace(property.ToString().Replace("{", "").Replace("}", ""), @"\t|\n|\r|\s+", "");                       
                        filters.Add(filter);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Regex.Replace(counterFilterModel.SubsetId.ToString().Replace("[", "").Replace("]", ""), @"\t|\n|\r|\s+", "")))
            {
                string convertdevice = Regex.Replace(counterFilterModel.SubsetId.ToString().Replace("[", "").Replace("]", ""), @"\t|\n|\r|\s+", "");
                var subsetlist = convertdevice.Split(',').ToList();
                query = query.Where(x => subsetlist.Contains(x.SubsetId.ToString()));


            }
            
            // Applay filter on data query
            if (filters.Count != 0)
            {

                foreach (var f in filters)
                {
                    if(typeof(Counter).GetProperty(f.key)!=null)
                    {
                      var expression = ExpressionUtils.BuildPredicate<Counter>(f.key,"==",f.values.ToString());
                      query = query.Where(expression);

                    }
                    
                    else

                    {

                        string fileds = Convert.ToString(string.Join(",", f.values));
                        string convertFields = fileds.Replace("\",\"", ",").Replace("[", "").Replace("]", "").Replace("\"", "");
                        if(!string.IsNullOrEmpty(convertFields))
                        {
                            query = query.Where(x => x.CounterFieldValues.Any(y => convertFields.Contains(y.FieldValue) && y.CounterField.ExtraField.Name.ToLower() == f.key.ToLower()));

                        }

                    }
                }
            }

            if (counterFilterModel.DeviceId != null && counterFilterModel.DeviceId != 0)
            {
                query = query.Where(x => x.Subset.DeviceId == counterFilterModel.DeviceId);
            }


            if (!string.IsNullOrEmpty(counterFilterModel.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(counterFilterModel.SearchQuery.ToLower())
                              || x.SupplierId.Contains(counterFilterModel.SearchQuery)
                              || x.Id.ToString().Equals(counterFilterModel.SearchQuery)
                              );
            }

            return query;
        }
        private ResultWithMessage sortAndPagination(CounterFilterModel counterFilterModel,IQueryable<CounterListViewModel> queryViewModel)
        {
            if (!string.IsNullOrEmpty(counterFilterModel.SortActive))
            {

                var sortProperty = typeof(CounterListViewModel).GetProperty(char.ToUpper(counterFilterModel.SortActive[0]) + counterFilterModel.SortActive.Substring(1));
                if (sortProperty != null && counterFilterModel.SortDirection == "asc")
                    queryViewModel = queryViewModel.OrderBy2(counterFilterModel.SortActive);

                else if (sortProperty != null && counterFilterModel.SortDirection == "desc")
                    queryViewModel = queryViewModel.OrderByDescending2(counterFilterModel.SortActive);

                int Count = queryViewModel.Count();

                var result = queryViewModel.Skip((counterFilterModel.PageIndex) * counterFilterModel.PageSize)
                .Take(counterFilterModel.PageSize).ToList();

                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

            else
            {
                int Count = queryViewModel.Count();
                var result = queryViewModel.Skip((counterFilterModel.PageIndex) * counterFilterModel.PageSize)
                .Take(counterFilterModel.PageSize).ToList();

                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

        }
        private IQueryable<CounterListViewModel> convertCountersToViewModel(IQueryable<Counter> model) =>
      model.Select(e => new CounterListViewModel
      {
          Id = e.Id,
          Name = e.Name,
          Code = e.Code,
          ColumnName = e.ColumnName,
          RefColumnName = e.RefColumnName,
          Description = e.Description,
          Aggregation = e.Aggregation,
          IsDeleted = e.IsDeleted,
          SupplierId = e.SupplierId,
          SubsetId = e.SubsetId,
          SubsetName = e.Subset.Name
      });

    }
}