using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using static Tenor.Helper.Constant;

namespace Tenor.Services.CountersService
{
    public interface ICountersService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter(CounterFilterModel filter);
        ResultWithMessage addCounter(CounterBindingModel model);
        ResultWithMessage updateCounter(CounterBindingModel CounterDto);
        ResultWithMessage deleteCounter(int id);
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
              SupplierId = e.SupplierId
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
                    TechnologyId = e.TechnologyId,
                    GroupDeviceTypeId = e.GroupDeviceTypeId,
                    GroupCategoryId = e.GroupCategoryId,
                    GroupLevelId = e.GroupLevelId,
                    SubsetId = e.SubsetId,
                    SubsetName = e.Subset.Name
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(Counter, "");
        }

        public ResultWithMessage getByFilter(CounterFilterModel filter)
        {
            var query = getCountersData(filter);
            var queryViewModel = convertCountersToViewModel(query);

            filter.SortActive = filter.SortActive == string.Empty ? "Id" : filter.SortActive;

            if (filter.SortDirection == enSortDirection.desc.ToString())
                queryViewModel = queryViewModel.OrderByDescending2(filter.SortActive);
            else
                queryViewModel = queryViewModel.OrderBy2(filter.SortActive);

            int resultSize = queryViewModel.Count();
            var resultData = queryViewModel.Skip(filter.PageSize * filter.PageIndex).Take(filter.PageSize).ToList();

            return new ResultWithMessage(new DataWithSize(resultSize, resultData), "");
        }

        public ResultWithMessage addCounter(CounterBindingModel model)
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
                TechnologyId = model.TechnologyId,
                GroupDeviceTypeId = model.GroupDeviceTypeId,
                GroupCategoryId = model.GroupCategoryId,
                GroupLevelId = model.GroupLevelId,
                SubsetId = model.SubsetId
            };

            try
            {
                _db.Add(counter);
                _db.SaveChanges();

                CounterViewModel counterViewModel = new()
                {
                    //Auto Mapper
                };

                return new ResultWithMessage(counterViewModel, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage updateCounter(CounterBindingModel model)
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
            counter.TechnologyId = model.TechnologyId;
            counter.GroupDeviceTypeId = model.GroupDeviceTypeId;
            counter.GroupCategoryId = model.GroupCategoryId;
            counter.GroupLevelId = model.GroupLevelId;
            counter.SubsetId = model.SubsetId;

            try
            {
                _db.Update(counter);
                _db.SaveChanges();

                CounterViewModel counterViewModel = new()
                {
                    //Auto Mapper
                };

                return new ResultWithMessage(counterViewModel, "");
            }

            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage deleteCounter(int id)
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