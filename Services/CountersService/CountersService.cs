using AutoMapper;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Collections;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Transactions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.CountersService.ViewModels;
using Tenor.Services.DevicesService.ViewModels;
using Tenor.Services.SubsetsService;
using Tenor.Services.SubsetsService.ViewModels;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
using static Tenor.Services.SharedService.ViewModels.SharedModels;

namespace Tenor.Services.CountersService
{
    public interface ICountersService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilterDyn(object filter);
        ResultWithMessage getExtraFields();
        ResultWithMessage add(CounterBindingModel model);
        ResultWithMessage edit(CounterBindingModel CounterDto);
        ResultWithMessage delete(int id);
        FileBytesModel exportCounterByFilter(CounterFilterModel filter);
        ResultWithMessage getByFilter(CounterFilterModel filter);
        ResultWithMessage validateCounter(int subsetId, string name);
        ResultWithMessage GetCounterBySubsetId(int subsetid, string searchQuery, TenantDto authUser);

    }

    public class CountersService : ICountersService
    {
        private readonly TenorDbContext _db;
        private readonly ISubsetsService _subsetsService;
        private readonly IMapper _mapper;

        public CountersService(TenorDbContext tenorDbContext, IMapper mapper, ISubsetsService subsetsService)
        {
            _db = tenorDbContext;
            _mapper = mapper;
            _subsetsService = subsetsService;
        }

        private IQueryable<Counter> getFilteredData(dynamic data, IQueryable<Counter> query, CounterFilterModel counterFilterModel, List<string> counterFields)
        {
            //Build filter for extra field
            List<Filter> filters = new List<Filter>();
            foreach (var s in counterFields)
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
                    if (typeof(Counter).GetProperty(f.key) != null)
                    {
                        var expression = ExpressionUtils.BuildPredicate<Counter>(f.key, "==", f.values.ToString());
                        query = query.Where(expression);

                    }

                    else

                    {

                        string fileds = Convert.ToString(string.Join(",", f.values));
                        string convertFields = fileds.Replace("\",\"", ",").Replace("[", "").Replace("]", "").Replace("\"", "");
                        if (!string.IsNullOrEmpty(convertFields))
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

        private ResultWithMessage sortAndPagination(CounterFilterModel counterFilterModel, IQueryable<CounterListViewModel> queryViewModel)
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


               // var pivotD = PivotData(result);
               // var response = MergData(pivotD, result);

                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

            else
            {
                int Count = queryViewModel.Count();
                var result = queryViewModel.Skip((counterFilterModel.PageIndex) * counterFilterModel.PageSize)
                .Take(counterFilterModel.PageSize).ToList();

               // var pivotD = PivotData(result);
                //var response = MergData(pivotD, result);
                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

        }

        private IQueryable<CounterListViewModel> convertCountersToListViewModel(IQueryable<Counter> model) =>
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
                      SubsetName = e.Subset.Name,
                      ExtraFields = _mapper.Map<List<CounterExtraFieldValueViewModel>>(e.CounterFieldValues)
                  });


        private bool isCounterExists(int counterId) => _db.Counters.Find(counterId) is not null;

       
        private bool isCounterExtraFieldIdExistsAndActive(int id) => _db.CounterFields.Where(e => e.Id == id && e.IsActive).FirstOrDefault() is not null;

        private string getValidaitingModelErrorMessage(CounterBindingModel model)
        {
            if (model is null)
                return "Empty Model!!";

            if (!_subsetsService.isSubsetExists(model.SubsetId))
                return $"Invalid Subset Id: {model.SubsetId}";

            foreach (ExtraFieldValue extraField in model.ExtraFields)
                if (!isCounterExtraFieldIdExistsAndActive(extraField.FieldId))
                    return $"Invalid or InActive ExtrafieldId: {extraField.FieldId} ,value: {extraField.Value}";

            return string.Empty;
        }

        private bool addCounterExtraFieldValues(List<ExtraFieldValue> extraFieldValues, int countertId)
        {
            foreach (ExtraFieldValue extraField in extraFieldValues)
            {
                var extraFieldValue = Convert.ToString(extraField.Value);
                extraFieldValue = Util.cleanExtraFieldValue(extraFieldValue);

                CounterFieldValue counterFieldValue = new()
                {
                    CounterId = countertId,
                    CounterFieldId = extraField.FieldId,
                    FieldValue = extraFieldValue
                };

                _db.Add(counterFieldValue);
            }

            _db.SaveChanges();
            return true;
        }

        private List<IDictionary<string, Object>> PivotData(List<CounterListViewModel> result)
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
                if(i<=9)
                {
                    idxData = pivotedData.Where(x => x.Key.StartsWith(i.ToString()) &&
                                   x.Key.Substring(0, 2) == i.ToString() + "."
                                   ).ToList();
                }

                if(i<=99 && i>9)
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
                    Match m = Regex.Match(kvp.Key, @"\.([0-99999]+)\.");
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
                pivoTmp.Add("Code", item.value.Code);
                pivoTmp.Add("ColumnName", item.value.ColumnName);
                pivoTmp.Add("RefColumnName", item.value.RefColumnName);
                pivoTmp.Add("Description", item.value.Description);
                pivoTmp.Add("Aggregation", item.value.Aggregation);
                pivoTmp.Add("SubsetName", item.value.SubsetName);

                foreach (var field in ExtField.Distinct().Select((value2, i2) => new { i2, value2 }))
                {
                    if ((((IDictionary<String, Object>)item.value).ContainsKey("Value" + field.i2.ToString())))
                    {
                        pivoTmp.Add(field.value2, (((IDictionary<String, Object>)item.value)["Value" + field.i2.ToString()]) != null ? ((IDictionary<String, Object>)item.value)["Value" + field.i2.ToString()] : "NA");

                    }
                }
                pivotData.Add(pivoTmp);

            }

            return pivotData;

        }
        private List<IDictionary<string, Object>> MergData(List<IDictionary<string, Object>> pivotdata, List<CounterListViewModel> datasort)
        {
            List<IDictionary<string, Object>> mergData = new List<IDictionary<string, Object>>();
            var props = typeof(CounterListViewModel).GetProperties().Select(x => x.Name).ToList();
            var keys = pivotdata.Count != 0 ? pivotdata.FirstOrDefault().Keys.ToList() : new List<string>();
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
            if (!isCounterExists(id))
                return new ResultWithMessage(null, $"Invalid Counter Id: {id}");

            CounterViewModel counterViewModel = _db.Counters.Include(x => x.CounterFieldValues)
                .ThenInclude(x => x.CounterField).
                    ThenInclude(x => x.ExtraField)
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
                    SubsetName = e.Subset.Name,
                    ExtraFields = _mapper.Map<List<CounterExtraFieldValueViewModel>>(e.CounterFieldValues)
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(counterViewModel, "");
        }

        public ResultWithMessage getByFilterDyn(object counterFilter)
        {
            try
            {
                //--------------------------Data Source---------------------------------------
                IQueryable<Counter> query = _db.Counters.Include(x => x.CounterFieldValues)
                   .ThenInclude(x => x.CounterField).
                    ThenInclude(x => x.ExtraField).Include(e => e.Subset).
                    ThenInclude(e => e.Device).Where(e => true);
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
                var queryViewModel = convertCountersToListViewModel(fiteredData);
                return sortAndPagination(counterFilterModel, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }

        public ResultWithMessage getExtraFields()
        {
            var extraFields = _mapper.Map<List<CounterExtraField>>(_db.CounterFields.Where(x => x.IsActive).Include(x => x.ExtraField).ToList());
            return new ResultWithMessage(extraFields, null);
        }

        public ResultWithMessage add(CounterBindingModel model)
        {
            string validaitingModelErrorMessage = getValidaitingModelErrorMessage(model);

            if (!string.IsNullOrEmpty(validaitingModelErrorMessage))
                return new ResultWithMessage(null, validaitingModelErrorMessage);

            using (TransactionScope transaction = new())
            {
                try
                {
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


                    _db.Add(counter);
                    _db.SaveChanges();

                    if (model.ExtraFields!=null)
                        addCounterExtraFieldValues(model.ExtraFields, counter.Id);

                    counter = _db.Counters.Include(x => x.CounterFieldValues)
                   .ThenInclude(x => x.CounterField).
                    ThenInclude(x => x.ExtraField).Include(e => e.Subset).
                    ThenInclude(e => e.Device).Single(e => e.Id == counter.Id);

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
                        SubsetName = counter.Subset.Name,
                        DeviceId = counter.Subset.Device.Id,
                        DeviceName = counter.Subset.Device.Name,
                        ExtraFields = _mapper.Map<List<CounterExtraFieldValueViewModel>>(counter.CounterFieldValues)
                    };

                    transaction.Complete();
                    return new ResultWithMessage(counterViewModel, "");
                }
                catch (Exception e)
                {
                    return new ResultWithMessage(model, e.Message);
                }
            }

        }

        public ResultWithMessage edit(CounterBindingModel model)
        {
            string validaitingModelErrorMessage = getValidaitingModelErrorMessage(model);

            if (!string.IsNullOrEmpty(validaitingModelErrorMessage))
                return new ResultWithMessage(null, validaitingModelErrorMessage);

            Counter counter = _db.Counters.Find(model.Id);

            if (counter is null)
                return new ResultWithMessage(null, $"Not found Counter with id: {model.Id}");

            using (TransactionScope transaction = new())
            {
                try
                {
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

                    List<CounterFieldValue> counterFieldValuesToDelete = _db.CounterFieldValues.Where(e => e.CounterId == model.Id).ToList();
                    _db.RemoveRange(counterFieldValuesToDelete);

                    _db.Update(counter);
                    _db.SaveChanges();

                    if (model.ExtraFields!=null)
                        addCounterExtraFieldValues(model.ExtraFields, counter.Id);

                    _db.SaveChanges();

                    counter = _db.Counters.Include(x => x.CounterFieldValues)
                                       .ThenInclude(x => x.CounterField).
                                        ThenInclude(x => x.ExtraField).Include(e => e.Subset).
                                        ThenInclude(e => e.Device).Single(e => e.Id == counter.Id);
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
                        SubsetName = counter.Subset.Name,
                        DeviceId = counter.Subset.Device.Id,
                        DeviceName = counter.Subset.Device.Name,
                        ExtraFields = _mapper.Map<List<CounterExtraFieldValueViewModel>>(counter.CounterFieldValues)
                    };

                    transaction.Complete();

                    return new ResultWithMessage(counterViewModel, "");
                }

                catch (Exception e)
                {
                    return new ResultWithMessage(model, e.Message);
                }
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

        public FileBytesModel exportCounterByFilter(CounterFilterModel filter)
        {
            //------------------------------Data source------------------------------------------------
            IQueryable<Counter> query = _db.Counters.Include(x => x.CounterFieldValues)
                               .ThenInclude(x => x.CounterField).
                                ThenInclude(x => x.ExtraField).Include(e => e.Subset).
                                ThenInclude(e => e.Device).Where(e => true);
            //------------------------------Data filter-----------------------------------------------------------

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(filter.SearchQuery.ToLower())
                          || x.SupplierId.Contains(filter.SearchQuery)
                          || x.Id.ToString().Equals(filter.SearchQuery)
                          );
            }
            if (filter.DeviceId != null && filter.DeviceId != 0)
            {
                query = query.Where(x => x.Subset.DeviceId == filter.DeviceId);
            }
            if (!string.IsNullOrEmpty(filter.SubsetId))
            {
                query = query.Where(x => x.SubsetId.ToString() == filter.SubsetId);
            }
            if (filter.ExtraFields.Count() != 0)
            {
                foreach (var s in filter.ExtraFields)
                {
                    string strValue = string.Join(',', s.Value).ToString();
                    strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

                    if (!string.IsNullOrEmpty(strValue))
                    {
                        query = query.Where(x => x.CounterFieldValues.Any(y => y.CounterField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
                    }

                }
            }

            //mapping wit DTO querable
            var queryViewModel = convertCountersToListViewModel(query);
            //-------------------Pivot data--------------------------------------
            var pivResult = PivotData(queryViewModel.ToList());
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

        public ResultWithMessage getByFilter(CounterFilterModel filter)
        {
            try
            {
                //------------------------------Data source------------------------------------------------
                IQueryable<Counter> query = _db.Counters.Include(x => x.CounterFieldValues)
                                   .ThenInclude(x => x.CounterField).
                                    ThenInclude(x => x.ExtraField).Include(e => e.Subset).
                                    ThenInclude(e => e.Device).Where(e => true);
                //------------------------------Data filter-----------------------------------------------------------
           
                if (!string.IsNullOrEmpty(filter.SearchQuery))
                {
                    query = query.Where(x => x.Name.ToLower().Contains(filter.SearchQuery.ToLower())
                              || x.SupplierId.Contains(filter.SearchQuery)
                              || x.Id.ToString().Equals(filter.SearchQuery)
                              );
                }

                if (filter.DeviceId != null && filter.DeviceId != 0)
                {
                    query = query.Where(x => x.Subset.DeviceId == filter.DeviceId);
                }

                if (!string.IsNullOrEmpty(filter.SubsetId))
                {
                    query = query.Where(x => x.SubsetId.ToString() == filter.SubsetId);
                }


                if (filter.ExtraFields!=null)
                {
                    foreach(var s in filter.ExtraFields)
                    {
                        string strValue = string.Join(',', s.Value).ToString();
                        strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

                        if ( !string.IsNullOrEmpty(strValue))
                        {
                            query = query.Where(x => x.CounterFieldValues.Any(y => y.CounterField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
                        }
                       
                    }
                }

                //mapping wit DTO querable
                var queryViewModel = convertCountersToListViewModel(query);
                //Sort and paginition
                return sortAndPagination(filter, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }

        public ResultWithMessage validateCounter(int subsetId, string name)
        {

            Counter counter = _db.Counters.SingleOrDefault(e => e.SubsetId == subsetId || e.Name.Trim().ToLower() == name.Trim().ToLower());

            if (counter is not null)
                return new ResultWithMessage(null, $"The Counter with Subset Id: {counter.SubsetId} and name: '{counter.Name}' is already exists");

            return new ResultWithMessage(true, string.Empty);
        }

        public ResultWithMessage GetCounterBySubsetId(int subsetid, string searchQuery, TenantDto authUser)
        {
            IQueryable<Counter> query = null;
            if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
            {
                query = _db.Counters.Where(e => e.SubsetId == subsetid);

            }
            else
            {
                 query = _db.Counters.Where(e => e.SubsetId == subsetid && authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains(e.Subset.Device.ParentId ?? 0));

            }

           
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(x => x.Id.ToString() == searchQuery || x.Name.ToLower().Contains(searchQuery.ToLower())
                       ||x.Code.ToLower().Contains(searchQuery.ToLower())
                       ||x.Subset.Name.ToLower().Contains(searchQuery.ToLower())
                       ||x.Subset.Device.Name.ToLower().Contains(searchQuery.ToLower())
                       ||x.Subset.Device.Parent.Name.ToLower().Contains(searchQuery.ToLower())
                       ||x.Subset.SupplierId.Contains(searchQuery)
                       ||x.Subset.SetId.ToString().Contains(searchQuery.ToLower())
                       ||x.Subset.SetName.ToLower().Contains(searchQuery.ToLower())
                       ||x.SupplierId.ToLower().Contains(searchQuery.ToLower())
                       ||x.Subset.Device.Childs.Any(y=>y.Name.ToLower().Contains(searchQuery.ToLower()) || y.SupplierId.StartsWith(searchQuery)
                       || y.Childs.Any(z=>x.Name.ToLower().Contains(searchQuery.ToLower()) || z.SupplierId.StartsWith(searchQuery)
                       || z.Childs.Any(c=>c.Name.ToLower().Contains(searchQuery.ToLower()) || c.SupplierId.StartsWith(searchQuery))))
                       );
                       
            }
            var result = query.Select(x => new TreeNodeViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = "counter",
                HasChild = false,
                Aggregation = x.Aggregation,
                SupplierId = x.SupplierId
            }).ToList();
            return new ResultWithMessage(result, "");
        }
    }
}