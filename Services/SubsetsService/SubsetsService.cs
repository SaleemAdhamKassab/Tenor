using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.SubsetsService.ViewModels;
using static Tenor.Helper.Constant;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Services.SubsetsService
{
    public interface ISubsetsService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter(object subsetfilter);
        ResultWithMessage add(SubsetBindingModel model);
        ResultWithMessage edit(SubsetBindingModel subsetDto);
        ResultWithMessage delete(int id);
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
              SchemaName = e.SchemaName,
              DeviceId = e.DeviceId,
              DeviceName = e.Device.Name,
              GranularityPeriod = e.GranularityPeriod,
              SummaryType = e.SummaryType
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
                    LoadPriorety = e.LoadPriorety,
                    SummaryType = e.SummaryType,
                    IsDeleted = e.IsDeleted,
                    DeviceId = e.DeviceId,
                    DeviceName = e.Device.Name
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(Subset, "");
        }

        public ResultWithMessage getByFilter(object subsetfilter)
        {
            try
            {

                List<Filter> filters = new List<Filter>();
                IQueryable<Subset> query = _db.Subsets.Where(e => true);
                List<string> subsetFields = _db.SubsetFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
                List<string> mainFields = new List<string>() { "supplierId", "isLoad", "deviceId", "id", "tableName" };
                subsetFields.AddRange(mainFields);
                //--------------------------------------------------------------
                dynamic data = JsonConvert.DeserializeObject<dynamic>(subsetfilter.ToString());
                GeneralFilterModel subsetFilterModel = new GeneralFilterModel()
                {
                    SearchQuery = data["searchQuery"],
                    PageIndex = data["pageIndex"],
                    PageSize = data["pageSize"],
                    SortActive = data["sortActive"],
                    SortDirection = data["sortDirection"]

                };
                //--------------------------------------------------------------
                foreach (var s in subsetFields)
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

                if (!string.IsNullOrEmpty(subsetFilterModel.SearchQuery))
                {
                    query = query.Where(x => x.Name.ToLower().StartsWith(subsetFilterModel.SearchQuery.ToLower()));
                }

                if (filters.Count != 0)
                {
                    var expression = ExpressionUtils.BuildPredicate<Subset>(filters);
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
                            query = query.Where(x => x.SubsetFieldValues.Any(y => y.FieldValue.Contains(convertFields) && y.SubsetField.ExtraField.Name == f.key));
                        }
                    }
                }

                var queryViewModel = convertSubsetsToViewModel(query);

                if (!string.IsNullOrEmpty(subsetFilterModel.SortActive))
                {

                    var sortProperty = typeof(SubsetListViewModel).GetProperty(subsetFilterModel.SortActive);
                    if (sortProperty != null && subsetFilterModel.SortDirection == "asc")
                        queryViewModel = queryViewModel.OrderBy2(subsetFilterModel.SortActive);

                    else if (sortProperty != null && subsetFilterModel.SortDirection == "desc")
                        queryViewModel = queryViewModel.OrderByDescending2(subsetFilterModel.SortActive);

                    int Count = queryViewModel.Count();

                    var result = queryViewModel.Skip((subsetFilterModel.PageIndex) * subsetFilterModel.PageSize)
                    .Take(subsetFilterModel.PageSize).ToList();

                    return new ResultWithMessage(new DataWithSize(Count, result), "");
                }

                else
                {
                    int Count = queryViewModel.Count();

                    var result = queryViewModel.Skip((subsetFilterModel.PageIndex) * subsetFilterModel.PageSize)
                    .Take(subsetFilterModel.PageSize).ToList();

                    return new ResultWithMessage(new DataWithSize(Count, result), "");
                }

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }

        public ResultWithMessage add(SubsetBindingModel model)
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
                    Id = subset.Id,
                    SupplierId = subset.SupplierId,
                    Name = subset.Name,
                    Description = subset.Description,
                    TableName = subset.TableName,
                    RefTableName = subset.RefTableName,
                    SchemaName = subset.SchemaName,
                    RefSchema = subset.RefSchema,
                    MaxDataDate = subset.MaxDataDate,
                    IsLoad = subset.IsLoad,
                    DataTS = subset.DataTS,
                    IndexTS = subset.IndexTS,
                    DbLink = subset.DbLink,
                    RefDbLink = subset.RefDbLink,
                    GranularityPeriod = subset.GranularityPeriod,
                    DimensionTable = subset.DimensionTable,
                    JoinExpression = subset.JoinExpression,
                    StartChar = subset.StartChar,
                    FactDimensionReference = subset.FactDimensionReference,
                    LoadPriorety = subset.LoadPriorety,
                    SummaryType = subset.SummaryType,
                    IsDeleted = subset.IsDeleted,
                    DeviceId = subset.DeviceId,
                    DeviceName = subset.Device.Name
                };

                return new ResultWithMessage(subsetViewModel, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage edit(SubsetBindingModel model)
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
            subset.LoadPriorety = model.LoadPriorety;
            subset.SummaryType = model.SummaryType;
            subset.IsDeleted = model.IsDeleted;

            try
            {
                _db.Update(subset);
                _db.SaveChanges();

                SubsetViewModel subsetViewModel = new()
                {
                    Id = subset.Id,
                    SupplierId = subset.SupplierId,
                    Name = subset.Name,
                    Description = subset.Description,
                    TableName = subset.TableName,
                    RefTableName = subset.RefTableName,
                    SchemaName = subset.SchemaName,
                    RefSchema = subset.RefSchema,
                    MaxDataDate = subset.MaxDataDate,
                    IsLoad = subset.IsLoad,
                    DataTS = subset.DataTS,
                    IndexTS = subset.IndexTS,
                    DbLink = subset.DbLink,
                    RefDbLink = subset.RefDbLink,
                    GranularityPeriod = subset.GranularityPeriod,
                    DimensionTable = subset.DimensionTable,
                    JoinExpression = subset.JoinExpression,
                    StartChar = subset.StartChar,
                    FactDimensionReference = subset.FactDimensionReference,
                    LoadPriorety = subset.LoadPriorety,
                    SummaryType = subset.SummaryType,
                    IsDeleted = subset.IsDeleted,
                    DeviceId = subset.DeviceId,
                    DeviceName = subset.Device.Name
                };

                return new ResultWithMessage(subsetViewModel, "");
            }

            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage delete(int id)
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