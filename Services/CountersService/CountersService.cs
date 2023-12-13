using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
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
    }

    public class CountersService : ICountersService
    {
        public CountersService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        private readonly TenorDbContext _db;


        private IQueryable<Counter> getCountersData(CounterFilterModel filter)
        {
            IQueryable<Counter> qeury = _db.Counters.Where(e => true);

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                qeury = qeury.Where(e => e.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()));

            return qeury;
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

                List<Filter> filters = new List<Filter>();
                IQueryable<Counter> query= _db.Counters.Where(e => true);
                List<string> counterFields = _db.CounterFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
                List<string> mainFields = new List<string>() { "supplierId", "code", "subsetId","id"};
                counterFields.AddRange(mainFields);
                //--------------------------------------------------------------
                dynamic data = JsonConvert.DeserializeObject<dynamic>(counterFilter.ToString());
                GeneralFilterModel counterFilterModel = new GeneralFilterModel()
                {
                    SearchQuery = data["searchQuery"],
                    PageIndex = data["pageIndex"],
                    PageSize = data["pageSize"],
                    SortActive = data["sortActive"],
                    SortDirection = data["sortDirection"]

                };
                //--------------------------------------------------------------
                foreach (var s in counterFields)
                {
                    Filter filter = new Filter();
                    object property = data[s];
                    if (property != null)
                    {
                        filter.key = s;
                        filter.values = Regex.Replace(property.ToString().Replace("{", "").Replace("}", ""), @"\t|\n|\r|\s+", "");
                        filters.Add(filter);
                    }
                }

                if (!string.IsNullOrEmpty(counterFilterModel.SearchQuery))
                {
                    query = query.Where(x => x.Name.ToLower().StartsWith(counterFilterModel.SearchQuery.ToLower()));
                }

                if (filters.Count != 0)
                {
                    var expression = ExpressionUtils.BuildPredicate<Counter>(filters);
                    if (expression != null)
                    {
                        query = query.Where(expression);
                    }
                    else
                    {
                        foreach (var f in filters)
                        {
                            string fileds = Convert.ToString(string.Join(",", f.values));
                            string convertFields = fileds.Replace("\",\"", ",").Replace("[", "").Replace("]", "").Replace("\"", "");
                            query = query.Where(x => x.CounterFieldValues.Any(y => y.FieldValue.Contains(convertFields) && y.CounterField.ExtraField.Name == f.key));
                        }
                    }
                }

                var queryViewModel = convertCountersToViewModel(query);


                if (!string.IsNullOrEmpty(counterFilterModel.SortActive))
                {

                    var sortProperty = typeof(CounterListViewModel).GetProperty(counterFilterModel.SortActive);
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
    }
}