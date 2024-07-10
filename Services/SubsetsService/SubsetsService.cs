using AutoMapper;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Transactions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using Tenor.Services.DevicesService;
using Tenor.Services.DevicesService.ViewModels;
using Tenor.Services.SubsetsService.ViewModels;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
using static Tenor.Services.SharedService.ViewModels.SharedModels;

namespace Tenor.Services.SubsetsService
{
    public interface ISubsetsService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter2(object subsetfilter);
        ResultWithMessage getExtraFields();
        bool isSubsetExists(int subsetId);
        ResultWithMessage add(SubsetBindingModel model);
        ResultWithMessage edit(int id ,SubsetBindingModel subsetDto);
        ResultWithMessage delete(int id);
        FileBytesModel exportSubsetByFilter(object FilterM);
        ResultWithMessage getByFilter(SubsetFilterModel filter);
        ResultWithMessage validateSubset(int deviceId, string name);

        ResultWithMessage GetSubsetByDeviceId(int deviceid, string searchQuery, TenantDto authUser);

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

        private IQueryable<SubsetListViewModel> convertSubsetsToListViewModel(IQueryable<Subset> model) =>
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
              SummaryType = e.SummaryType,
              ExtraFields = _mapper.Map<List<SubsetExtraFieldValueViewModel>>(e.SubsetFieldValues)
          });

        private bool isSubsetExtraFieldIdExistsAndActive(int id) => _db.SubsetFields.Where(e => e.Id == id && e.IsActive).FirstOrDefault() is not null;

        private List<SubsetExtraFieldValueViewModel> getExtraFields(int subsetId)
        {
            List<SubsetExtraFieldValueViewModel> extraFields =
            _db.SubsetFieldValues
            .Where(e => e.SubsetId == subsetId)
            .Include(e => e.SubsetField)
            .ThenInclude(e => e.ExtraField)
            .Select(e => new SubsetExtraFieldValueViewModel()
            {
                Id = e.Id,
                FieldId = e.SubsetField.Id,
                Type = e.SubsetField.ExtraField.Type.ToString(),
                FieldName = e.SubsetField.ExtraField.Name,
                Value = e.FieldValue.Contains(',') ? Util.convertStringToList(e.FieldValue) : e.FieldValue
            })
            .ToList();

            return extraFields;
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

            if (subsetFilterModel.DeviceId != null && subsetFilterModel.DeviceId != 0)
            {
                query = query.Where(x => x.DeviceId == subsetFilterModel.DeviceId);

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

               // var pivotD = PivotData(result);
                //var response = MergData(pivotD, result);
                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

            else
            {
                int Count = queryViewModel.Count();
                var result = queryViewModel.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                .Take(kpiFilterModel.PageSize).ToList();

                //var pivotD = PivotData(result);
                //var response = MergData(pivotD, result);
                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

        }

        private string getValidaitingModelErrorMessage(SubsetBindingModel model)
        {
            if (model is null)
                return "Empty Model!!";

            if (!_devicesService.isDeviceExists(model.DeviceId))
                return $"Invalid device Id: {model.DeviceId}";

            foreach (ExtraFieldValue extraField in model.ExtraFields)
                if (!isSubsetExtraFieldIdExistsAndActive(extraField.FieldId))
                    return $"Invalid or InActive ExtrafieldId: {extraField.FieldId} ,value: {extraField.Value}";

            return string.Empty;
        }

        private bool addSubsetExtraFieldValues(List<ExtraFieldValue> extraFieldValues, int subsetId)
        {
            foreach (ExtraFieldValue extraField in extraFieldValues)
            {
                var extraFieldValue = Convert.ToString(extraField.Value);
                extraFieldValue = Util.cleanExtraFieldValue(extraFieldValue);

                SubsetFieldValue subsetFieldValue = new()
                {
                    SubsetId = subsetId,
                    SubsetFieldId = extraField.FieldId,
                    FieldValue = extraFieldValue
                };

                _db.Add(subsetFieldValue);
            }

            _db.SaveChanges();
            return true;
        }

        private bool isCounterExists(int counterId) => _db.Counters.Find(counterId) is not null;
        private List<IDictionary<string, Object>> PivotData(List<SubsetListViewModel> result)
        {
            List<string> ExtField = new List<string>();
            List<dynamic> convertedData = new List<dynamic>();
            List<IDictionary<string, Object>> pivotData = new List<IDictionary<string, Object>>();
            var expandoObject = new ExpandoObject() as IDictionary<string, Object>;
            //-----------------------Faltten Data-------------------------------------
            foreach (var v in result)
            {
                foreach (var r in v.ExtraFields)
                {
                    ExtField.Add(r.FieldName);
                    if (r.Type == "List" || r.Type == "MultiSelectList")
                    {
                        List<string> collection = (List<string>)r.Value;

                        string Val = string.Join(',', collection);
                        r.Value = Val;
                    }

                }
            }

            var dict = JsonHelper.DeserializeAndFlatten(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            foreach (var kvp in dict)
            {
                expandoObject.Add(kvp.Key, kvp.Value);
            }
            var pivotedData = expandoObject.ToList();

            for (int i = 0; i <= result.Count - 1; i++)
            {
                List<KeyValuePair<string, object>> idxData = new List<KeyValuePair<string, object>>();
                var tmp = new ExpandoObject() as IDictionary<string, Object>;
                if (i <= 9)
                {
                    idxData = pivotedData.Where(x => x.Key.StartsWith(i.ToString()) &&
                                   x.Key.Substring(0, 2) == i.ToString() + "."
                                   ).ToList();
                }
                if (i <= 99 && i > 9)
                {
                    idxData = pivotedData.Where(x => x.Key.StartsWith(i.ToString()) &&
                                                       x.Key.Substring(0, 3) == i.ToString() + "."
                                                       ).ToList();
                }
                if (i <= 999 && i > 99)
                {
                    idxData = pivotedData.Where(x => x.Key.StartsWith(i.ToString()) &&
                                                       x.Key.Substring(0, 4) == i.ToString() + "."
                                                       ).ToList();
                }
                if (i <= 9999 && i > 999)
                {
                    idxData = pivotedData.Where(x => x.Key.StartsWith(i.ToString()) &&
                                                       x.Key.Substring(0, 5) == i.ToString() + "."
                                                       ).ToList();
                }
                if (i <= 99999 && i > 10000)
                {
                    idxData = pivotedData.Where(x => x.Key.StartsWith(i.ToString()) &&
                                                       x.Key.Substring(0, 6) == i.ToString() + "."
                                                       ).ToList();
                }
                foreach (var kvp in idxData)
                {
                    int k = kvp.Key.LastIndexOf(".");
                    string key = (k > -1 ? kvp.Key.Substring(k + 1) : kvp.Key);
                    Match m = Regex.Match(kvp.Key, @"\.([0-100000]+)\.");
                    if (m.Success) key += m.Groups[1].Value;
                    tmp.Add(key, kvp.Value);
                }

                convertedData.Add(tmp);
            }

            //---------------------Pivot--------------------------
            foreach (var item in convertedData.Select((value, i) => new { i, value }))
            {
                var pivoTmp = new ExpandoObject() as IDictionary<string, Object>;

                pivoTmp.Add("Id", item.value.Id);
                pivoTmp.Add("SupplierId", item.value.SupplierId);
                pivoTmp.Add("Name", item.value.Name);
                pivoTmp.Add("Description", item.value.Description);
                pivoTmp.Add("RefTableName", item.value.RefTableName);
                pivoTmp.Add("SchemaName", item.value.SchemaName);
                pivoTmp.Add("RefSchema", item.value.RefSchema);
                pivoTmp.Add("MaxDataDate", item.value.MaxDataDate);
                pivoTmp.Add("IsLoad", item.value.IsLoad);
                pivoTmp.Add("DataTS", item.value.DataTS);
                pivoTmp.Add("IndexTS", item.value.IndexTS);
                pivoTmp.Add("DbLink", item.value.DbLink);
                pivoTmp.Add("RefDbLink", item.value.RefDbLink);
                pivoTmp.Add("GranularityPeriod", item.value.GranularityPeriod);
                pivoTmp.Add("SummaryType", item.value.SummaryType);
                pivoTmp.Add("DeviceId", item.value.DeviceId);
                pivoTmp.Add("DeviceName", item.value.DeviceName);

                foreach (var field in ExtField.Distinct().Select((value2, i2) => new { i2, value2 }))
                {
                    if((((IDictionary<String, Object>)item.value).ContainsKey("Value" + field.i2.ToString())))
                    {
                        pivoTmp.Add(field.value2, (((IDictionary<String, Object>)item.value)["Value" + field.i2.ToString()]) != null ? ((IDictionary<String, Object>)item.value)["Value" + field.i2.ToString()] : "NA");

                    }

                }
                pivotData.Add(pivoTmp);

            }

            return pivotData;

        }
        private List<IDictionary<string, Object>> MergData(List<IDictionary<string, Object>> pivotdata, List<SubsetListViewModel> datasort)
        {
            List<IDictionary<string, Object>> mergData = new List<IDictionary<string, Object>>();
            var props = typeof(SubsetListViewModel).GetProperties().Select(x => x.Name).ToList();
            var keys = pivotdata.Count!=0? pivotdata.FirstOrDefault().Keys.ToList() : new List<string>() ;
            var addProps = keys.Union(props).ToList();
            var resAndPiv = datasort.Zip(pivotdata, (p, d) => new { sortd = p, pivotd = d });

            foreach (var s in resAndPiv)
            {
                var mergTmp = new ExpandoObject() as IDictionary<string, Object>;
                foreach (var prop in addProps)
                {
                    if (props.Contains(prop))
                    {
                        if (prop == "ExtraFields")
                        {
                            foreach (var p in s.sortd.ExtraFields)
                            {
                                if (p.Type == "List" || p.Type == "MultiSelectList")
                                {
                                    p.Value = p.GetType().GetProperty("Value").GetValue(p).ToString().Split(',').ToList();

                                }
                                else
                                {
                                    p.Value = p.GetType().GetProperty("Value").GetValue(p);
                                }
                            }
                        }

                        mergTmp.Add(prop, s.sortd.GetType().GetProperty(prop).GetValue(s.sortd));

                    }
                    else
                    {
                        mergTmp.Add(prop, s.pivotd[prop]);

                    }

                }

                mergData.Add(mergTmp);
            }

            return mergData;
        }


        public ResultWithMessage getById(int id)
        {
            if (!isSubsetExists(id))
                return new ResultWithMessage(null, $"Invalid Subset Id: {id}");


            SubsetViewModel model = _db.Subsets.Include(x => x.SubsetFieldValues).ThenInclude(x => x.SubsetField).
                    ThenInclude(x => x.ExtraField).Include(e => e.Device)
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
                    DeviceName = e.Device.Name,
                    ExtraFields = _mapper.Map<List<SubsetExtraFieldValueViewModel>>(e.SubsetFieldValues)
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(model, "");
        }

        public ResultWithMessage getByFilter2(object subsetfilter)
        {
            try
            {
                //--------------------------Data Source---------------------------------------
                IQueryable<Subset> query = _db.Subsets.Include(x => x.SubsetFieldValues).ThenInclude(x => x.SubsetField).
                ThenInclude(x => x.ExtraField).AsQueryable();
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
                var queryViewModel = convertSubsetsToListViewModel(fiteredData);
                return sortAndPagination(subsetFilterModel, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }

        public ResultWithMessage getExtraFields()
        {
            var extraFields = _mapper.Map<List<SubsetExtraField>>(_db.SubsetFields.Where(x => x.IsActive).Include(x => x.ExtraField).ToList());
            return new ResultWithMessage(extraFields, null);
        }

        public bool isSubsetExists(int id) => _db.Subsets.Any(e => e.Id == id);

        public ResultWithMessage add(SubsetBindingModel model)
        {
            string validaitingModelErrorMessage = getValidaitingModelErrorMessage(model);

            if (!string.IsNullOrEmpty(validaitingModelErrorMessage))
                return new ResultWithMessage(null, validaitingModelErrorMessage);

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

                    if (model.ExtraFields!=null)
                        addSubsetExtraFieldValues(model.ExtraFields, subset.Id);

                    subset = _db.Subsets.Include(x => x.SubsetFieldValues).ThenInclude(x => x.SubsetField).
                    ThenInclude(x => x.ExtraField).Include(e => e.Device).Single(e => e.Id == subset.Id);

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
                        DeviceName = subset.Device.Name,
                        ExtraFields = _mapper.Map<List<SubsetExtraFieldValueViewModel>>(subset.SubsetFieldValues)
                    };

                    transaction.Complete();
                    return new ResultWithMessage(subsetViewModel, null);
                }

                catch (Exception ex)
                {
                    return new ResultWithMessage(null, ex.Message);
                }
            }
        }

        public ResultWithMessage edit(int id,SubsetBindingModel model)
        {
            model.Id = id;
            string validaitingModelErrorMessage = getValidaitingModelErrorMessage(model);

            if (!string.IsNullOrEmpty(validaitingModelErrorMessage))
                return new ResultWithMessage(null, validaitingModelErrorMessage);

            Subset subset = _db.Subsets.Find(model.Id);

            if (subset is null)
                return new ResultWithMessage(null, $"Not found Subset with id: {model.Id}");

            using (TransactionScope transaction = new())
            {
                try
                {
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

                    List<SubsetFieldValue> subsetFieldValuesToDelete = _db.SubsetFieldValues.Where(e => e.SubsetId == model.Id).ToList();
                    _db.RemoveRange(subsetFieldValuesToDelete);

                    _db.Update(subset);

                    if (model.ExtraFields!=null)
                        addSubsetExtraFieldValues(model.ExtraFields, subset.Id);

                    _db.SaveChanges();

                    subset = _db.Subsets.Include(x => x.SubsetFieldValues).ThenInclude(x => x.SubsetField).
                    ThenInclude(x => x.ExtraField).Include(e => e.Device).Single(e => e.Id == subset.Id);

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
                        DeviceName = subset.Device.Name,
                        ExtraFields = _mapper.Map<List<SubsetExtraFieldValueViewModel>>(subset.SubsetFieldValues)
                    };

                    transaction.Complete();
                    return new ResultWithMessage(subsetViewModel, "");
                }

                catch (Exception e)
                {
                    return new ResultWithMessage(model, e.Message);
                }
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

        public FileBytesModel exportSubsetByFilter(object subsetfilter)
        {
            //--------------------------Data Source---------------------------------------
            IQueryable<Subset> query = _db.Subsets.Include(x => x.SubsetFieldValues).ThenInclude(x => x.SubsetField).
                ThenInclude(x => x.ExtraField).AsQueryable();
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
            //-------------------------------Data Converted------------------------------------------
            var result = convertSubsetsToListViewModel(fiteredData);
            //-------------------Pivot data--------------------------------------
            var pivResult = PivotData(result.ToList());
            //-----------------------------------------------------------------------
            if (pivResult == null || pivResult.Count() == 0)
                return new FileBytesModel();

            FileBytesModel excelfile = new();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var stream = new MemoryStream();
            var package = new ExcelPackage(stream);
            var workSheet = package.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells.LoadFromCollection(pivResult, true);

            List<int> dateColumns = new();
            int datecolumn = 1;
            foreach (var PropertyInfo in pivResult.GetType().GetProperties())
            {
                if (PropertyInfo.PropertyType == typeof(DateTime) || PropertyInfo.PropertyType == typeof(DateTime?))
                {
                    dateColumns.Add(datecolumn);
                }
                datecolumn++;
            }
            dateColumns.ForEach(item => workSheet.Column(item).Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss AM/PM");
            package.Save();
            excelfile.Bytes = stream.ToArray();
            stream.Position = 0;
            stream.Close();
            string excelName = $"Posts-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excelfile.FileName = excelName;
            excelfile.ContentType = contentType;
            return excelfile;
        }

        public ResultWithMessage getByFilter(SubsetFilterModel filter)
        {
            try
            {
                //------------------------------Data source------------------------------------------------
                IQueryable<Subset> query = _db.Subsets.Include(x => x.SubsetFieldValues).
                    ThenInclude(x => x.SubsetField).
                    ThenInclude(x => x.ExtraField).AsQueryable();
                //------------------------------Data filter-----------------------------------------------------------

                if (!string.IsNullOrEmpty(filter.SearchQuery))
                {
                    query = query.Where(x => x.Name.ToLower().Contains(filter.SearchQuery.ToLower())
                                  || x.SupplierId.Contains(filter.SearchQuery)
                                  || x.TableName.Contains(filter.SearchQuery)
                                  || x.Id.ToString().Equals(filter.SearchQuery)
                                  );
                }

                if (filter.DeviceId!=null && filter.DeviceId!=0)
                {
                    query = query.Where(x => x.DeviceId == filter.DeviceId);

                }


                if (filter.ExtraFields != null)
                {
                    foreach (var s in filter.ExtraFields)
                    {
                        string strValue = string.Join(',', s.Value).ToString();
                        strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

                        if (!string.IsNullOrEmpty(strValue))
                        {
                            query = query.Where(x => x.SubsetFieldValues.Any(y => y.SubsetField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
                        }

                    }
                }

                //mapping wit DTO querable
                var queryViewModel = convertSubsetsToListViewModel(query);
                //Sort and paginition
                return sortAndPagination(filter, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }

        public ResultWithMessage validateSubset(int deviceId, string name)
        {

            Subset subset = _db.Subsets.SingleOrDefault(e => e.Id == deviceId || e.Name.Trim().ToLower() == name.Trim().ToLower());

            if (subset is not null)
                return new ResultWithMessage(null, $"The subset with Id: {subset.Id} and name: '{subset.Name}' is already exists");

            return new ResultWithMessage(true, string.Empty);
        }

        public ResultWithMessage GetSubsetByDeviceId(int deviceid, string searchQuery, TenantDto authUser)
        {
            IQueryable<Subset> query = null;
            if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
            {
                query = _db.Subsets.Where(e => e.DeviceId == deviceid);

            }
            else
            {
               query = _db.Subsets.Where(e => e.DeviceId == deviceid && authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains(e.Device.ParentId ?? 0));

            }

            if (query == null || query.Count() == 0)
            {
                return new ResultWithMessage(new DataWithSize(0, null), "Access denied to this Device or Device is invalid");

            }
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(x => x.Id.ToString() == searchQuery || x.Name.ToLower().Contains(searchQuery.ToLower()) || x.SupplierId.ToLower().Contains(searchQuery.ToLower())
                        || x.Counters.Any(z=>z.Id.ToString()==searchQuery || z.Name.ToLower().Contains(searchQuery.ToLower()) || z.Code.ToLower()==searchQuery.ToLower() || z.SupplierId.ToLower().Contains(searchQuery.ToLower()))
                        || x.Device.Name.ToLower().Contains(searchQuery.ToLower())
                      );

            }
            //var queryViewModel = convertSubsetsToListViewModel(query);
            //return new ResultWithMessage(new DataWithSize(queryViewModel.Count(), queryViewModel.ToList()), null);
            var result = query.Select(x => new TreeNodeViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = "subset",
                HasChild = x.Counters.Count() > 0
            }).ToList();
            return new ResultWithMessage(result, "");
        }
    }
}