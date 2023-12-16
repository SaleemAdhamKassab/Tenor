using AutoMapper;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Runtime.Remoting;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Transactions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using Tenor.Services.DevicesService;
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
        ResultWithMessage GetExtraFields();
    }

    public class SubsetsService : ISubsetsService
    {
        private readonly TenorDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDevicesService _devicesService;

        public SubsetsService(TenorDbContext tenorDbContext, IMapper mapper, IDevicesService devicesService)
        {
            _db = tenorDbContext;
            _mapper = mapper;
            _devicesService = devicesService;
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
                //--------------------------Data Source---------------------------------------
                IQueryable<Subset> query = _db.Subsets.Where(e => true);
                List<string> subsetFields = _db.SubsetFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
                //------------------------------Conver Dynamic filter--------------------------
                dynamic data = JsonConvert.DeserializeObject<dynamic>(subsetfilter.ToString());
                SubsetFilterModel subsetFilterModel = new SubsetFilterModel()
                {
                    SearchQuery = subsetfilter.ToString().Contains("SearchQuery") ? data["SearchQuery"] : data["searchQuery"],
                    PageIndex = subsetfilter.ToString().Contains("PageIndex") ? data["PageIndex"] : data["pageIndex"],
                    PageSize = subsetfilter.ToString().Contains("PageSize") ? data["PageSize"] : data["pageSize"],
                    SortActive = subsetfilter.ToString().Contains("SortActive") ? data["SortActive"] : data["sortActive"],
                    SortDirection = subsetfilter.ToString().Contains("SortDirection") ? data["SortDirection"] : data["sortDirection"],
                    DeviceId = subsetfilter.ToString().Contains("DeviceId") ? data["DeviceId"] : data["deviceId"]

                };
                //--------------------------------Filter and conver data to VM----------------------------------------------
                IQueryable<Subset> fiteredData = getFilteredData(data, query, subsetFilterModel, subsetFields);
                //-------------------------------Data sorting and pagination------------------------------------------
                var queryViewModel = convertSubsetsToViewModel(fiteredData);
                return sortAndPagination(subsetFilterModel, queryViewModel);

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

            if (!_devicesService.isDeviceExists(model.DeviceId))
                return new ResultWithMessage(null, $"Invalid device Id: {model.DeviceId}");

            using (TransactionScope transaction = new())
            {
                try
                {
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
                        DeviceId = model.DeviceId,
                    };

                    _db.Add(subset);
                    _db.SaveChanges();


                    if (model.ExtraFields.Count != 0)
                    {
                        foreach (ExtraFieldValue extraField in model.ExtraFields)
                        {
                            //check if extraField.FieldId is active and correct

                            var extraFieldValue = Convert.ToString(extraField.Value);
                            extraFieldValue = Util.cleanExtraFieldValue(extraFieldValue);


                            SubsetFieldValue subsetFieldValue = new()
                            {
                                SubsetId = subset.Id,
                                SubsetFieldId = extraField.FieldId,
                                FieldValue = extraFieldValue
                            };

                            _db.Add(subsetFieldValue);
                        }
                    }

                    _db.SaveChanges();

                    subset = _db.Subsets.Include(e => e.Device).Single(e => e.Id == subset.Id);

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

                    transaction.Complete();
                    return new ResultWithMessage(subset, null);
                }

                catch (Exception ex)
                {
                    return new ResultWithMessage(null, ex.Message);
                }
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
        public ResultWithMessage GetExtraFields()
        {
            var extraFields = _mapper.Map<List<SubsetExtraField>>(_db.SubsetFields.Where(x => x.IsActive).Include(x => x.ExtraField).ToList());
            return new ResultWithMessage(extraFields, null);

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
        private IQueryable<Subset> getFilteredData(dynamic data, IQueryable<Subset> query, SubsetFilterModel subsetFilterModel, List<string> subsetFields)
        {
            //Build filter for extra field
            List<Filter> filters = new List<Filter>();
            foreach (var s in subsetFields)
            {
                Filter filter = new Filter();
                object property = data[s] != null ? data[s] : data[char.ToLower(s[0]) + s.Substring(1)];
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

            // Applay filter on data query
            if (filters.Count != 0)
            {

                foreach (var f in filters)
                {
                    if (typeof(Subset).GetProperty(f.key) != null)
                    {

                        var expression = ExpressionUtils.BuildPredicate<Subset>(f.key, "==", f.values.ToString());
                        query = query.Where(expression);

                    }

                    else

                    {

                        string fileds = Convert.ToString(string.Join(",", f.values));
                        string convertFields = fileds.Replace("\",\"", ",").Replace("[", "").Replace("]", "").Replace("\"", "");
                        if (!string.IsNullOrEmpty(convertFields))
                        {
                            query = query.Where(x => x.SubsetFieldValues.Any(y => convertFields.Contains(y.FieldValue) && y.SubsetField.ExtraField.Name.ToLower() == f.key.ToLower()));

                        }

                    }
                }
            }

            if (!string.IsNullOrEmpty(subsetFilterModel.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(subsetFilterModel.SearchQuery.ToLower())
                              || x.SupplierId.Contains(subsetFilterModel.SearchQuery)
                              || x.TableName.Contains(subsetFilterModel.SearchQuery)
                              || x.Id.ToString().Equals(subsetFilterModel.SearchQuery)
                              );
            }

            if (!string.IsNullOrEmpty(subsetFilterModel.DeviceId))
            {
                query = query.Where(x => x.DeviceId.ToString() == subsetFilterModel.DeviceId);

            }

            return query;
        }
        private ResultWithMessage sortAndPagination(SubsetFilterModel kpiFilterModel, IQueryable<SubsetListViewModel> queryViewModel)
        {
            if (!string.IsNullOrEmpty(kpiFilterModel.SortActive))
            {

                var sortProperty = typeof(SubsetListViewModel).GetProperty(char.ToUpper(kpiFilterModel.SortActive[0]) + kpiFilterModel.SortActive.Substring(1));
                if (sortProperty != null && kpiFilterModel.SortDirection == "asc")
                    queryViewModel = queryViewModel.OrderBy2(kpiFilterModel.SortActive);

                else if (sortProperty != null && kpiFilterModel.SortDirection == "desc")
                    queryViewModel = queryViewModel.OrderByDescending2(kpiFilterModel.SortActive);

                int Count = queryViewModel.Count();

                var result = queryViewModel.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                .Take(kpiFilterModel.PageSize).ToList();

                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

            else
            {
                int Count = queryViewModel.Count();
                var result = queryViewModel.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                .Take(kpiFilterModel.PageSize).ToList();

                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

        }

    }
}