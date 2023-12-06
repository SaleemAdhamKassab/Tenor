using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.SubsetsService.ViewModels;
using static Tenor.Helper.Constant;

namespace Tenor.Services.SubsetsService
{
    public interface ISubsetsService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter(SubsetFilterModel filter);
        ResultWithMessage addSubset(SubsetBindingModel model);
        ResultWithMessage updateSubset(SubsetBindingModel subsetDto);
        ResultWithMessage deleteSubset(int id);
    }

    public class SubsetsService : ISubsetsService
    {
        public SubsetsService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        private readonly TenorDbContext _db;


        private IQueryable<Subset> getSubsetsData(SubsetFilterModel filter)
        {
            IQueryable<Subset> qeury = _db.Subsets.Where(e => true);

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                qeury = qeury.Where(e => e.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()));

            return qeury;
        }

        private IQueryable<SubsetListViewModel> convertSubsetsToViewModel(IQueryable<Subset> model) =>
          model.Select(e => new SubsetListViewModel
          {
              Id = e.Id,
              SupplierId = e.SupplierId,
              Name = e.Name,
              Description = e.Description,
              TableName = e.TableName,
              RefTableName = e.RefTableName,
              IsLoad = e.IsLoad,
              DataTS = e.DataTS,
              DbLink = e.DbLink,
              IndexTS = e.IndexTS,
              IsDeleted = e.IsDeleted,
              MaxDataDate = e.MaxDataDate,
              RefDbLink = e.RefDbLink,
              RefSchema = e.RefSchema,
              SchemaName = e.SchemaName
          });


        public ResultWithMessage getById(int id)
        {
            SubsetViewModel Subset = _db.Subsets
                .Select(e => new SubsetViewModel()
                {
                    Id = e.Id,
                    SupplierId = e.SupplierId,
                    Name = e.Name,
                    Description = e.Description,
                    TableName = e.TableName,
                    RefTableName = e.RefTableName,
                    SchemaName = e.SchemaName,
                    RefSchema = e.RefSchema,
                    MaxDataDate = e.MaxDataDate,
                    IsLoad = e.IsLoad,
                    DataTS = e.DataTS,
                    IndexTS = e.IndexTS,
                    DbLink = e.DbLink,
                    RefDbLink = e.RefDbLink,
                    GranularityPeriod = e.GranularityPeriod,
                    DimensionTable = e.DimensionTable,
                    JoinExpression = e.JoinExpression,
                    StartChar = e.StartChar,
                    FactDimensionReference = e.FactDimensionReference,
                    TechnologyId = e.TechnologyId,
                    LoadPriorety = e.LoadPriorety,
                    SummaryType = e.SummaryType,
                    IsDeleted = e.IsDeleted,
                    DeviceId = e.DeviceId,
                    DeviceName = e.Device.Name
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(Subset, "");
        }

        public ResultWithMessage getByFilter(SubsetFilterModel filter)
        {
            var query = getSubsetsData(filter);
            var queryViewModel = convertSubsetsToViewModel(query);

            filter.SortActive = filter.SortActive == string.Empty ? "Id" : filter.SortActive;

            if (filter.SortDirection == enSortDirection.desc.ToString())
                queryViewModel = queryViewModel.OrderByDescending2(filter.SortActive);
            else
                queryViewModel = queryViewModel.OrderBy2(filter.SortActive);

            int resultSize = queryViewModel.Count();
            var resultData = queryViewModel.Skip(filter.PageSize * filter.PageIndex).Take(filter.PageSize).ToList();

            return new ResultWithMessage(new DataWithSize(resultSize, resultData), "");
        }

        public ResultWithMessage addSubset(SubsetBindingModel model)
        {
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            Subset subset = new()
            {
                Id = model.Id,
                SupplierId = model.SupplierId,
                Name = model.Name,
                Description = model.Description,
                TableName = model.TableName,
                RefTableName = model.RefTableName,
                SchemaName = model.SchemaName,
                RefSchema = model.RefSchema,
                MaxDataDate = model.MaxDataDate,
                IsLoad = model.IsLoad,
                DataTS = model.DataTS,
                IndexTS = model.IndexTS,
                DbLink = model.DbLink,
                RefDbLink = model.RefDbLink,
                GranularityPeriod = model.GranularityPeriod,
                DimensionTable = model.DimensionTable,
                JoinExpression = model.JoinExpression,
                StartChar = model.StartChar,
                FactDimensionReference = model.FactDimensionReference,
                TechnologyId = model.TechnologyId,
                LoadPriorety = model.LoadPriorety,
                SummaryType = model.SummaryType,
                IsDeleted = model.IsDeleted,
                DeviceId = model.DeviceId
            };


            try
            {
                _db.Add(subset);
                _db.SaveChanges();

                SubsetViewModel subsetViewModel = new()
                {
                    //Auto mapper
                };

                return new ResultWithMessage(subsetViewModel, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage updateSubset(SubsetBindingModel model)
        {
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            Subset subset = _db.Subsets.Find(model.Id);

            if (subset is null)
                return new ResultWithMessage(null, $"Not found Subset with id: {model.Id}");

            subset.Id = model.Id;
            subset.SupplierId = model.SupplierId;
            subset.Name = model.Name;
            subset.Description = model.Description;
            subset.TableName = model.TableName;
            subset.RefTableName = model.RefTableName;
            subset.SchemaName = model.SchemaName;
            subset.MaxDataDate = model.MaxDataDate;
            subset.IsLoad = model.IsLoad;
            subset.DataTS = model.DataTS;
            subset.IndexTS = model.IndexTS;
            subset.DbLink = model.DbLink;
            subset.RefDbLink = model.RefDbLink;
            subset.GranularityPeriod = model.GranularityPeriod;
            subset.DimensionTable = model.DimensionTable;
            subset.JoinExpression = model.JoinExpression;
            subset.StartChar = model.StartChar;
            subset.FactDimensionReference = model.FactDimensionReference;
            subset.TechnologyId = model.TechnologyId;
            subset.LoadPriorety = model.LoadPriorety;
            subset.SummaryType = model.SummaryType;
            subset.IsDeleted = model.IsDeleted;

            try
            {
                _db.Update(subset);
                _db.SaveChanges();

                SubsetViewModel subsetViewModel = new()
                {
                    //Auto mapper
                };

                return new ResultWithMessage(subsetViewModel, "");
            }

            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage deleteSubset(int id)
        {
            Subset subset = _db.Subsets.Find(id);

            if (subset is null)
                return new ResultWithMessage(null, $"Not found Subset with id: {id}");

            try
            {
                subset.IsDeleted = true;
                _db.Update(subset);
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