using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using System.Text.Json;
using System.Data;
using System.Transactions;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
using Tenor.Helper;
using Infrastructure.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using Tenor.Services.CountersService.ViewModels;
using static Azure.Core.HttpHeader;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tenor.Services.SubsetsService.ViewModels;
using Microsoft.IdentityModel.Tokens;
using Tenor.Services.DevicesService.ViewModels;
using OfficeOpenXml;
using static OfficeOpenXml.ExcelErrorValue;
using System.Security.Policy;
using OfficeOpenXml.Drawing.Style.Coloring;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Net.Quic;
using System;
using System.Runtime.InteropServices;
using static Tenor.Helper.Constant;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;

namespace Tenor.Services.KpisService
{
    public interface IKpisService
    {
        ResultWithMessage GetListAsync(object kpiFilterM);
        Task<ResultWithMessage> GetByIdAsync(int id);
        Task<ResultWithMessage> Add(CreateKpi kpiModel);
        Task<ResultWithMessage> Update(int id, CreateKpi Kpi);
        Task<ResultWithMessage> Delete(int id);
        Task<ResultWithMessage> GetExtraFields();
        ResultWithMessage GetOperators();
        ResultWithMessage GetFunctions();
        FileBytesModel exportKpiByFilter(object kpiFilterM);
        ResultWithMessage getByFilter(KpiFilterModel filter, TenantDto authUser);
        Task<ResultWithMessage> GetKpiQuery(int kpiid);
        FileBytesModel exportKpiByFilter2(KpiFilterModel filter);
        string GetQeuryExpress(OperationDto opt, string? tag,int? voidChildCount);
        ResultWithMessage ValidateKpi(int? deviceid, string kpiname);
        ResultWithMessage CheckValidFormat(CreateKpi input);


    }

    public class KpisService : IKpisService
    {
        private readonly TenorDbContext _db;
        private readonly IMapper _mapper;
        private string query = "";
        private string joinExpression = "";
        private bool checkResult = true;
        private int voidIdx = 0;

        public KpisService(TenorDbContext tenorDbContext, IMapper mapper)
        {
            _db = tenorDbContext;
            _mapper = mapper;
        }
        //Not Used
        public ResultWithMessage GetListAsync(object kpiFilterM)
        {
            try
            {
                //--------------------------Data Source---------------------------------------
                IQueryable<Kpi> query = _db.Kpis.Include(x => x.KpiFieldValues).ThenInclude(x => x.KpiField).
                 ThenInclude(x => x.ExtraField).AsQueryable();
                List<string> kpiFields = _db.KpiFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
                //------------------------------Conver Dynamic filter--------------------------
                dynamic data = JsonConvert.DeserializeObject<dynamic>(kpiFilterM.ToString());
                KpiFilterModel kpiFilterModel = new KpiFilterModel()
                {
                    SearchQuery = kpiFilterM.ToString().Contains("SearchQuery") ? data["SearchQuery"] : data["searchQuery"],
                    PageIndex = kpiFilterM.ToString().Contains("PageIndex") ? data["PageIndex"] : data["pageIndex"],
                    PageSize = kpiFilterM.ToString().Contains("PageSize") ? data["PageSize"] : data["pageSize"],
                    SortActive = kpiFilterM.ToString().Contains("SortActive") ? data["SortActive"] : data["sortActive"],
                    SortDirection = kpiFilterM.ToString().Contains("SortDirection") ? data["SortDirection"] : data["sortDirection"],
                    DeviceId = kpiFilterM.ToString().Contains("DeviceId") ? data["DeviceId"] : data["deviceId"],


                };
                //--------------------------------Filter and conver data to VM----------------------------------------------
                IQueryable<Kpi> fiteredData = getFilteredData(data, query, kpiFilterModel, kpiFields);
                //-------------------------------Data sorting and pagination------------------------------------------
                var queryViewModel = fiteredData.Select(x => new KpiListViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    DeviceId = x.DeviceId,
                    DeviceName = x.Device.Name,
                    ExtraFields = _mapper.Map<List<KpiFieldValueViewModel>>(x.KpiFieldValues)
                });

                return sortAndPagination(kpiFilterModel, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }
        public async Task<ResultWithMessage> GetByIdAsync(int id)
        {
            var kpi = _db.Kpis.Include(x => x.Device).Include(x => x.Operation)
                .Include(x => x.KpiFieldValues)
                 .ThenInclude(x => x.KpiField).ThenInclude(x => x.ExtraField)
                 .FirstOrDefault(x => x.Id == id);

            if (kpi == null)
            {
                return new ResultWithMessage(null, "This KPI Id is invalid");

            }
            GetSelfRelation(kpi.OperationId);
            var kpiMap = _mapper.Map<KpiViewModel>(kpi);
            return new ResultWithMessage(kpiMap, null);
        }
        public async Task<ResultWithMessage> Add(CreateKpi kpi)
        {
           
            if (IsKpiExist(0, kpi.DeviceId, kpi.Name))
            {
                return new ResultWithMessage(null, "This Kpi name alraedy exsit on the same device");

            }

            if(Convert.ToBoolean(CheckValidFormat(kpi).Data)==false)
            {
                return new ResultWithMessage(null, CheckValidFormat(kpi).Message);

            }
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    Kpi newKpi = _mapper.Map<Kpi>(kpi);
                    _db.Kpis.Add(newKpi);
                    _db.SaveChanges();

                    if (kpi.KpiFields != null)
                    {
                        AddExtraFields(newKpi.Id, kpi.KpiFields);
                    }
                    transaction.Complete();
                    return new ResultWithMessage(_mapper.Map<KpiViewModel>(newKpi), null);
                }
                catch (Exception ex)
                {

                    return new ResultWithMessage(null, ex.Message);

                }
            }
        }
        public async Task<ResultWithMessage> Update(int id, CreateKpi Kpi)
        {

            if (id != Kpi.Id)
            {
                return new ResultWithMessage(null, "Invalid KPI Id");
            }
            if (IsKpiExist(Kpi.Id, Kpi.DeviceId, Kpi.Name))
            {
                return new ResultWithMessage(null, "This Kpi name alraedy exsit on the same device");

            }
            if (Convert.ToBoolean(CheckValidFormat(Kpi).Data) == false)
            {
                return new ResultWithMessage(null, CheckValidFormat(Kpi).Message);

            }
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    Kpi oldKpi = _db.Kpis.AsNoTracking().FirstOrDefault(x => (x.Id == Kpi.Id && x.CreatedBy==Kpi.ModifyBy) || x.IsPublic);
                    if (oldKpi == null)
                    {
                        return new ResultWithMessage(null, "This Id is invalid");
                    }
                    Kpi.CreatedBy = oldKpi.CreatedBy;
                    Kpi.CreationDate = oldKpi.CreationDate;
                    Kpi updatedKpi = _mapper.Map<Kpi>(Kpi);
                    _db.Update(updatedKpi);
                    _db.SaveChanges();
                    //Remove old childs relation
                    deleteOperation(oldKpi.OperationId);
                    //Update Kpi field values
                    if (Kpi.KpiFields != null)
                    {
                        var KpiFieldValues = _db.KpiFieldValues.Where(x => x.KpiId == Kpi.Id).ToList();
                        _db.KpiFieldValues.RemoveRange(KpiFieldValues);
                        _db.SaveChanges();
                        AddExtraFields(Kpi.Id, Kpi.KpiFields);
                    }
                    transaction.Complete();
                    return new ResultWithMessage(_mapper.Map<KpiViewModel>(updatedKpi), null);
                }
                catch (Exception ex)
                {
                    return new ResultWithMessage(null, ex.Message);

                }
            }
        }
        public async Task<ResultWithMessage> Delete(int id)
        {
            var currentKpi = _db.Kpis.FirstOrDefault(x => x.Id == id);
            if(currentKpi == null)
            {
                return new ResultWithMessage(null, "Cannot find KPI");
            }
            var hasLinkedOperation = _db.Operations.Any(x => x.KpiId == id);
            if(hasLinkedOperation)
            {
                return new ResultWithMessage(false, "This KPI is linked to other operations and cannot be deleted");
            }
            deleteOperation(currentKpi.OperationId);
            _db.Kpis.Remove(currentKpi);
            _db.SaveChanges();
            return new ResultWithMessage(true, "");
        }
        public async Task<ResultWithMessage> GetExtraFields()
        {
            var extraFields = _mapper.Map<List<KpiExtraField>>(_db.KpiFields.Where(x => x.IsActive).Include(x => x.ExtraField).ToList());
            return new ResultWithMessage(extraFields, null);

        }
        public ResultWithMessage GetOperators()
        {
            var opt = _db.Operators.ToList();
            return new ResultWithMessage(opt, null);

        }
        public ResultWithMessage GetFunctions()
        {
            var func = _db.Functions.Where(x=>x.Id!=3).ToList();
            return new ResultWithMessage(func, null);

        }
        public FileBytesModel exportKpiByFilter(object kpiFilterM)
        {
            //---------------------------------Data source----------------------------------
            IQueryable<Kpi> query = _db.Kpis.Include(x => x.KpiFieldValues).ThenInclude(x => x.KpiField).
                  ThenInclude(x => x.ExtraField).AsQueryable();
            List<string> kpiFields = _db.KpiFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
            //------------------------------Conver Dynamic filter--------------------------
            dynamic data = JsonConvert.DeserializeObject<dynamic>(kpiFilterM.ToString());
            KpiFilterModel kpiFilterModel = new KpiFilterModel()
            {
                SearchQuery = kpiFilterM.ToString().Contains("SearchQuery") ? data["SearchQuery"] : data["searchQuery"],
                PageIndex = kpiFilterM.ToString().Contains("PageIndex") ? data["PageIndex"] : data["pageIndex"],
                PageSize = kpiFilterM.ToString().Contains("PageSize") ? data["PageSize"] : data["pageSize"],
                SortActive = kpiFilterM.ToString().Contains("SortActive") ? data["SortActive"] : data["sortActive"],
                SortDirection = kpiFilterM.ToString().Contains("SortDirection") ? data["SortDirection"] : data["sortDirection"],
                DeviceId = kpiFilterM.ToString().Contains("DeviceId") ? data["DeviceId"] : data["deviceId"],


            };
            //--------------------------------Filter and conver data to VM-----------------
            IQueryable<Kpi> list = getFilteredData(data, query, kpiFilterModel, kpiFields);
            //-------------------------------Data Convertint-------------------
            var result = list.Select(x => new KpiListViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                DeviceId = x.DeviceId,
                DeviceName = x.Device.Name,
                ExtraFields = _mapper.Map<List<KpiFieldValueViewModel>>(x.KpiFieldValues)
            }).ToList();
            //-------------------Pivot data--------------------------------------
            var pivResult = PivotData(result);
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
            foreach (var PropertyInfo in pivResult.FirstOrDefault().GetType().GetProperties())
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
        public ResultWithMessage getByFilter(KpiFilterModel filter, TenantDto authUser)
        {
            try
            {
                //------------------------------Data source------------------------------------------------

                IQueryable<Kpi>  query = _db.Kpis.Where(x => x.CreatedBy == authUser.userName || x.IsPublic || authUser.deviceAccesses.Select(x=>x.DeviceId).ToList().Contains((int)x.DeviceId))
                    .Include(x => x.KpiFieldValues).ThenInclude(x => x.KpiField)
                    .ThenInclude(x => x.ExtraField).AsQueryable();
                              
                //------------------------------Data filter-----------------------------------------------------------

                if (!string.IsNullOrEmpty(filter.SearchQuery))
                {
                    query = query.Where(x => x.Name.ToLower().Contains(filter.SearchQuery.ToLower())
                                  || x.DeviceId.ToString().Equals(filter.SearchQuery)
                                  || x.Id.ToString().Equals(filter.SearchQuery)
                                  );
                }
                if (filter.DeviceId != null && filter.DeviceId != 0)
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
                            query = query.Where(x => x.KpiFieldValues.Any(y => y.KpiField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
                        }

                    }
                }

                //mapping wit DTO querable
                var queryViewModel = query.Select(x => new KpiListViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    DeviceId = x.DeviceId,
                    DeviceName = x.Device.Name,
                    CreatedBy=x.CreatedBy,
                    CreationDate=x.CreationDate,
                    IsPublic=x.IsPublic,
                    ModifyBy=x.ModifyBy,
                    ModifyDate=x.ModifyDate,
                    ExtraFields = _mapper.Map<List<KpiFieldValueViewModel>>(x.KpiFieldValues)
                });
                //Sort and paginition
                return sortAndPagination(filter, queryViewModel);

            }
            catch (Exception ex)
            {
                return new ResultWithMessage(new DataWithSize(0, null), ex.Message);

            }
        }
        public async Task<ResultWithMessage> GetKpiQuery(int kpiid)
        {
            var response = GetKpiForm(kpiid);
            var tabNames = GetTablesNameRegExp(response.Result.Data.ToString()).ToList();
            string fullQuery = GetOracleQuery(response.Result.Data.ToString(), tabNames);

            return new ResultWithMessage(fullQuery, null);

        }
        public FileBytesModel exportKpiByFilter2(KpiFilterModel filter)
        {
            //------------------------------Data source------------------------------------------------
            IQueryable<Kpi> query = _db.Kpis.Include(x => x.KpiFieldValues).ThenInclude(x => x.KpiField).
              ThenInclude(x => x.ExtraField).AsQueryable();
            //------------------------------Data filter-----------------------------------------------------------

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(filter.SearchQuery.ToLower())
                              || x.DeviceId.ToString().Equals(filter.SearchQuery)
                              || x.Id.ToString().Equals(filter.SearchQuery)
                              );
            }
            if (filter.DeviceId != null && filter.DeviceId != 0)
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
                        query = query.Where(x => x.KpiFieldValues.Any(y => y.KpiField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
                    }

                }
            }

            //mapping wit DTO querable
            var queryViewModel = query.Select(x => new KpiListViewModel()
            {
                Id = x.Id,
                Name = x.Name,
                DeviceId = x.DeviceId,
                DeviceName = x.Device.Name,
                ExtraFields = _mapper.Map<List<KpiFieldValueViewModel>>(x.KpiFieldValues)
            });
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
            foreach (var PropertyInfo in pivResult.FirstOrDefault().GetType().GetProperties())
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
        public string GetQeuryExpress(OperationDto opt, string? tag,int? voidChildCount)
        {
            ConvertKpiForm kpiFormat = new ConvertKpiForm(_db, _mapper);
            string pointerTag = "tag";
            string funcTag = "func";
            QueryExpress qe = new QueryExpress();
            IDictionary<string, string> funcDic = new Dictionary<string, string>()
            {
                {"func0","Case When" },{"func1","Then" },{"func2","else" }
            };
            if (opt.Type == enOPerationTypes.voidFunction)
            {

                if (!string.IsNullOrEmpty(tag) || query.Contains(pointerTag))
                {
                    voidIdx = opt.Childs.Count();

                    qe.LeftSide = "("; qe.Inside = pointerTag; qe.RightSide = ")";
                    string chageStr = qe.LeftSide + qe.Inside + qe.RightSide;
                    if (!string.IsNullOrEmpty(tag))
                    {
                        query = query.Replace(tag, chageStr);

                    }
                    else
                    {
                        query = query.Replace(pointerTag, chageStr);

                    }

                    if (opt.Childs.Count != 0)
                    {
                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {
                            GetQeuryExpress(c.value, null, voidIdx);

                        }
                    }
                }
                else
                {

                    qe.LeftSide = "("; qe.Inside = pointerTag; qe.RightSide = ")";
                    query += qe.LeftSide + qe.Inside + qe.RightSide;
                    if (opt.Childs.Count != 0)
                    {
                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {

                            GetQeuryExpress(c.value, null, voidIdx);

                        }
                    }
                }

            }
            if (opt.Type == enOPerationTypes.number)
            {

                qe.LeftSide = ""; qe.Inside = opt.Value; qe.RightSide = "";
                string ChangStr = qe.LeftSide + qe.Inside + qe.RightSide;
                if (query.Contains(pointerTag))
                {
                    query = query.Replace(pointerTag, ChangStr);

                }
                else
                {
                    if(voidChildCount>=1)
                    {
                        voidIdx -= 1;
                        query = query.Insert(query.Length - 2, ChangStr);

                    }
                    else
                    {
                        query = query.Insert(query.Length - 1, ChangStr);

                    }
                }

            }
            if (opt.Type == enOPerationTypes.opt)
            {

                qe.LeftSide = ""; qe.Inside = opt.OperatorName; qe.RightSide = "";
                string changeStr = qe.LeftSide + qe.Inside + qe.RightSide;
                if (query.Contains(pointerTag))
                {
                    query = query.Replace(pointerTag, changeStr);
                }
                else
                {
                    if(voidChildCount>=1)
                    {
                        voidIdx -= 1;

                        query = query.Insert(query.Length - 2, changeStr);

                    }
                    else
                    {
                        query = query.Insert(query.Length - 1, changeStr);

                    }

                }

            }
            if (opt.Type == enOPerationTypes.kpi)
            {
                string kpiNewFormat = kpiFormat.GetKpiFomat((int)opt.KpiId);
                qe.LeftSide = "(";
                qe.Inside = opt.Aggregation == enAggregation.na ? kpiNewFormat : opt.Aggregation + "(" + kpiNewFormat + ")";
                qe.RightSide = ")";
                string kpiState = qe.LeftSide + qe.Inside + qe.RightSide;
                if (!query.Contains(pointerTag))
                {
                    if(voidChildCount>=1)
                    {
                        voidIdx -= 1;
                        query = query.Insert(query.Length - 2, kpiState);

                    }
                    else
                    {
                        voidIdx -= 1;

                        query = query.Insert(query.Length - 1, kpiState);

                    }

                }
                else
                {
                    query = query.Replace(pointerTag, kpiState);

                }

            }
            if (opt.Type == enOPerationTypes.counter)
            {
                qe.LeftSide = "";
                qe.Inside = opt.Aggregation == enAggregation.na ? opt.TableName+"."+opt.ColumnName : opt.Aggregation + "(" + opt.TableName + "." + opt.ColumnName + ")";
                qe.RightSide = "";
                string chageStr = qe.LeftSide + qe.Inside + qe.RightSide;
                if (!query.Contains(pointerTag))
                {
                    if(voidChildCount>=1)
                    {
                        voidIdx -= 1;

                        query = query.Insert(query.Length > 0 ? query.Length - 2 : 0, chageStr);

                    }
                    else
                    {
                        query = query.Insert(query.Length > 0 ? query.Length - 1 : 0, chageStr);

                    }


                }
                else
                {
                    voidIdx -= 1;

                    query = query.Replace(pointerTag, chageStr);

                }
               

            }
            if (opt.Type == enOPerationTypes.function)
            {

                var func = _db.Functions.FirstOrDefault(f => f.Id == opt.FunctionId);
                if (func.Name.ToLower() == "if")
                {
                    qe.Inside = "Case when func0 then func1 else func2  end";
                    qe.LeftSide = "";
                    qe.RightSide = "";
                    string Chang = qe.LeftSide + qe.Inside + qe.RightSide;
                    if (query.Contains(pointerTag))
                    {
                        query = query.Replace(pointerTag, Chang);

                    }
                    else
                    {
                        if(voidChildCount>=1)
                        {
                            voidIdx -= 1;

                            query = query.Insert(query.Length > 0 ? query.Length - 2 : 0, Chang);

                        }
                        else
                        {
                            query = query.Insert(query.Length > 0 ? query.Length - 1 : 0, Chang);

                        }

                    }

                    if (opt.Childs.Count != 0)
                    {

                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {
                            kpiFormat = new ConvertKpiForm(_db, _mapper);
                            string repFunc = kpiFormat.GetQeuryExpress(c.value, null, voidIdx);
                            query = query.Replace(funcTag + c.i, repFunc);
                        }
                    }
                }

                else
                {
                    for (int i = 0; i <= func.ArgumentsCount - 1; i++)
                    {
                        if (opt.Aggregation == enAggregation.na)
                        {
                            qe.Inside += i < func.ArgumentsCount - 1 ? funcTag + i + "," : funcTag + i;

                        }
                        else
                        {
                            qe.Inside += i < func.ArgumentsCount - 1 ? opt.Aggregation + "(" + funcTag + i + ")" + "," : opt.Aggregation + "(" + funcTag + i + ")";

                        }

                    }

                    qe.LeftSide = opt.FunctionName + "(";
                    qe.RightSide = ")";
                    string Chang = qe.LeftSide + qe.Inside + qe.RightSide;
                    if (query.Contains(pointerTag))
                    {
                        query = query.Replace(pointerTag, Chang);

                    }
                    else
                    {
                        if(voidChildCount>=1)
                        {
                            voidIdx -= 1;

                            query = query.Insert(query.Length - 2, Chang);

                        }
                        else
                        {
                            query = query.Insert(query.Length - 1, Chang);

                        }

                    }


                    if (opt.Childs.Count != 0)
                    {

                        foreach (var c in opt.Childs.Select((value, i) => new { i, value }))
                        {
                            kpiFormat = new ConvertKpiForm(_db, _mapper);
                            string repFunc = kpiFormat.GetQeuryExpress(c.value, null, voidIdx);
                            query = query.Replace(funcTag + c.i, repFunc);
                        }
                    }
                }
               

            }

            return query;

        }
        public ResultWithMessage ValidateKpi(int? deviceid, string kpiname)
        {
            if(IsKpiExist(0, deviceid, kpiname))
            {
                return new ResultWithMessage(false, "This KPI name is already exsist on this device.");
            }
            return new ResultWithMessage(true,null);
        }
        public async Task<ResultWithMessage> GetKpiForm(int kpiId)
        {
            var kpi = _db.Kpis.Include(x => x.Device).Include(x => x.Operation).Include(x => x.KpiFieldValues)
                .ThenInclude(x => x.KpiField).ThenInclude(x => x.ExtraField)
                .FirstOrDefault(x => x.Id == kpiId);

            if (kpi == null)
            {
                return new ResultWithMessage(null, "This KPI Is invalid");

            }
            GetSelfRelation(kpi.OperationId);
            var kpiMap = _mapper.Map<KpiViewModel>(kpi);

            var convertedKpiData = AddNoZeroToKpi(kpiMap.Operations);
            kpiMap.Operations = convertedKpiData;
            //-------------------------------------------
            string queryBuilder = GetQeuryExpress(kpiMap.Operations, null, kpiMap.Operations.Childs.Count());

            return new ResultWithMessage(queryBuilder, null);
        }
        public ResultWithMessage CheckValidFormat(CreateKpi input)
        {
            //var convertedKpiData = AddNoZeroToKpi(input.Operation);
           // input.Operation = convertedKpiData;
            if (input.Operation.Type.GetDisplayName()!= "voidFunction")
            {
                return new ResultWithMessage(false, "KPI format is invalid");

            }
            if(input.Operation.Childs.Count == 0)
            {
                return new ResultWithMessage(false, "Please assign KPI formula");
            }
            if (input.Operation.Childs[0].Type.GetDisplayName()== "opt")
            {
                return new ResultWithMessage(false, "KPI format is invalid");

            }
            else
            {
                if (IsFormatValid(input.Operation))
                {
                    return new ResultWithMessage(true, null);
                }
            }

            return new ResultWithMessage(false, "KPI format is invalid");

        }
        private bool AddExtraFields(int kpiId, List<ExtraFieldValue> extFields)
        {
            foreach (var s in extFields)
            {

                var extField = _db.KpiFields.Include(x => x.ExtraField).FirstOrDefault(x => x.Id == s.FieldId && x.IsActive);
                if (extField != null)
                {
                    if (extField.ExtraField.Type.GetDisplayName() == "List" || extField.ExtraField.Type.GetDisplayName() == "MultiSelectList")
                    {
                        string fileds = Convert.ToString(string.Join(",", s.Value));
                        string convertFields = fileds.Replace("\",\"", ",").Replace("[\"", "").Replace("\"]", "");
                        var ListValue = new KpiFieldValue(kpiId, s.FieldId, convertFields);
                        _db.KpiFieldValues.Add(ListValue);
                        _db.SaveChanges();

                    }
                    else
                    {
                        var StringValue = new KpiFieldValue(kpiId, s.FieldId, Convert.ToString(s.Value));
                        _db.KpiFieldValues.Add(StringValue);
                        _db.SaveChanges();

                    }

                }

            }
            return true;
        }
        private List<OperationDto> GetSelfRelation(int optid)
        {
            List<OperationDto> result = new List<OperationDto>();
            var opt = _db.Operations.Include(x => x.Function).Include(x => x.Counter).ThenInclude(x=>x.Subset)
                .Include(x => x.Kpi).Include(x => x.Operator)
                .Include(x => x.Childs).FirstOrDefault(x => x.Id == optid);

            result.Add(_mapper.Map<OperationDto>(opt));
            if (opt.Childs.Count != 0)
            {
                foreach (var s in opt.Childs)
                {
                    GetSelfRelation(s.Id);
                }

            }

            return result;
        }
        private IQueryable<Kpi> getFilteredData(dynamic data, IQueryable<Kpi> query, KpiFilterModel kpiFilterModel, List<string> counterFields)
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

            // Applay filter on data query
            if (filters.Count != 0)
            {

                foreach (var f in filters)
                {
                    if (typeof(Kpi).GetProperty(f.key) != null)
                    {
                        var expression = ExpressionUtils.BuildPredicate<Kpi>(f.key, "==", f.values.ToString());
                        query = query.Where(expression);

                    }

                    else

                    {

                        string fileds = Convert.ToString(string.Join(",", f.values));
                        string convertFields = fileds.Replace("\",\"", ",").Replace("[", "").Replace("]", "").Replace("\"", "");
                        if (!string.IsNullOrEmpty(convertFields))
                        {
                            query = query.Where(x => x.KpiFieldValues.Any(y => convertFields.Contains(y.FieldValue) && y.KpiField.ExtraField.Name.ToLower() == f.key.ToLower()));

                        }

                    }
                }
            }

            if (!string.IsNullOrEmpty(kpiFilterModel.SearchQuery))
            {
                query = query.Where(x => x.Name.ToLower().Contains(kpiFilterModel.SearchQuery.ToLower())
                              || x.DeviceId.ToString().Equals(kpiFilterModel.SearchQuery)
                              || x.Id.ToString().Equals(kpiFilterModel.SearchQuery)
                              );
            }
            if (kpiFilterModel.DeviceId != null && kpiFilterModel.DeviceId != 0)
            {
                query = query.Where(x => x.DeviceId == kpiFilterModel.DeviceId);

            }
            return query;
        }
        private ResultWithMessage sortAndPagination(KpiFilterModel kpiFilterModel, IQueryable<KpiListViewModel> queryViewModel)
        {
            if (!string.IsNullOrEmpty(kpiFilterModel.SortActive))
            {

                var sortProperty = typeof(CounterListViewModel).GetProperty(char.ToUpper(kpiFilterModel.SortActive[0]) + kpiFilterModel.SortActive.Substring(1));
                if (sortProperty != null && kpiFilterModel.SortDirection == "asc")
                    queryViewModel = queryViewModel.OrderBy2(kpiFilterModel.SortActive);

                else if (sortProperty != null && kpiFilterModel.SortDirection == "desc")
                    queryViewModel = queryViewModel.OrderByDescending2(kpiFilterModel.SortActive);

                int Count = queryViewModel.Count();

                var result = queryViewModel.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                .Take(kpiFilterModel.PageSize).ToList();

                //var pivotD = PivotData(result);
                //var response = MergData(pivotD, result);
                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

            else
            {
                int Count = queryViewModel.Count();
                var result = queryViewModel.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                .Take(kpiFilterModel.PageSize).ToList();

                //var pivotD = PivotData(result);
                // var response = MergData(pivotD, result);
                return new ResultWithMessage(new DataWithSize(Count, result), "");
            }

        }
        private List<IDictionary<string, Object>> PivotData(List<KpiListViewModel> model)
        {
            List<string> ExtField = new List<string>();
            List<dynamic> convertedData = new List<dynamic>();
            List<IDictionary<string, Object>> pivotData = new List<IDictionary<string, Object>>();
            var expandoObject = new ExpandoObject() as IDictionary<string, Object>;
            //-----------------------Faltten Data-------------------------------------
            foreach (var v in model)
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

            var dict = JsonHelper.DeserializeAndFlatten(Newtonsoft.Json.JsonConvert.SerializeObject(model));
            foreach (var kvp in dict)
            {
                expandoObject.Add(kvp.Key, kvp.Value);
            }
            var pivotedData = expandoObject.ToList();

            for (int i = 0; i <= model.Count - 1; i++)
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
                pivoTmp.Add("Name", item.value.Name);
                pivoTmp.Add("DeviceName", item.value.DeviceName);
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
        private List<IDictionary<string, Object>> MergData(List<IDictionary<string, Object>> pivotdata, List<KpiListViewModel> datasort)
        {
            List<IDictionary<string, Object>> mergData = new List<IDictionary<string, Object>>();
            var props = typeof(KpiListViewModel).GetProperties().Select(x => x.Name).ToList();
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
        private bool IsKpiExist(int id, int? deviceid, string kpiname)
        {
            bool isExist = _db.Kpis.Any(x => x.DeviceId == deviceid && x.Name == kpiname && x.Id != id);
            return isExist;
        }
        private bool IsFormatValid(OperationBinding input)
        {
           
            try
            {
                if (input.Type.GetDisplayName() == "voidFunction")
                {
                    List<OperationBinding> data = input.Childs.ToList();
                    var levelType = data.Select(x => new { x.Type, x.Order,x.OperatorId,x.FunctionId }).ToList();
                    for (int i = 1; i < levelType.Count; i++)
                    {
                        //if (levelType[i].Type.GetDisplayName() == "opt" && levelType[i].OperatorId == 4)
                        //{
                        //    if (!(levelType[i+1].Type.GetDisplayName()== "function" && levelType[i+1].FunctionId==3))
                        //    {
                        //        checkResult = false;
                        //        return checkResult;
                        //    }
                           
                        //}

                        if (levelType[i].Type == levelType[i - 1].Type)
                        {
                            checkResult = false;
                            return checkResult;
                        }

                        if (levelType[i].Type.GetDisplayName()!= "opt" && levelType[i-1].Type.GetDisplayName()!= "opt")
                        {
                            checkResult = false;
                            return checkResult;
                        }

                        if (levelType[levelType.Count()-1].Type.GetDisplayName()== "opt")
                        {
                            checkResult = false;
                            return checkResult;
                        }
                    }

                }

                if (input.Type.GetDisplayName() == "function")
                {
                    Function func = _db.Functions.FirstOrDefault(x => x.Id == (int)input.FunctionId);
                    if (func.ArgumentsCount != input.Childs.Count())
                    {
                        checkResult = false;
                        return checkResult;

                    }
                }

                if(input.Childs!=null && input.Childs.Count()>0)
                {
                    foreach(var c in input.Childs)
                    {
                        IsFormatValid(c);

                    }
                }
                return checkResult;

            }
            catch (Exception ex)
            {
                checkResult = false;
                return checkResult;

            }

        }    
        private OperationDto AddNoZeroToKpi(OperationDto input)
        {
            List<OperationDto> data = input.Childs.ToList();
            for (int i = 0; i < data.Count()-1; i++)
            {
                if ((data[i].Type == enOPerationTypes.opt && data[i].OperatorId == 4) && 
                    !(data[i+1].Type== enOPerationTypes.function && data[i + 1].FunctionId==3))
                {
                    OperationDto convertData =new OperationDto();
                    OperationDto convertChildData = new OperationDto();

                    convertData.Type = enOPerationTypes.function;
                    convertData.Order = data[i].Order;
                    convertData.FunctionId = 3;
                    convertData.FunctionName = "NoZero";
                    convertData.Childs = new List<OperationDto>();
                    //------------------Add void to function--------------------
                    convertChildData.Type = enOPerationTypes.voidFunction;
                    convertChildData.Order = 1;
                    convertChildData.Childs = data.GetRange(i + 1, data.Count()-(i+1));
                    //------------------------------------------------------
                    convertData.Childs.Add(convertChildData);
                    data[i+1] = convertData;

                    break;

                }
                if (data[i].Childs.Count() > 0)
                {
                    foreach (var c in data[i].Childs)
                    {
                        AddNoZeroToKpi(c);

                    }
                }

            }

            input.Childs=data;
            return input;
        }
        private IEnumerable<string> GetTablesNameRegExp(string query)
        {
            List<string> tablesName = new List<string>();
            string pattern = @"\b[tech4_]\w+";
            Regex rg = new Regex(pattern,RegexOptions.IgnoreCase);
            MatchCollection matchedTables = rg.Matches(query);
            for (int i = 0; i < matchedTables.Count; i++)
            {
                if ((matchedTables[i].Value.ToLower().Contains("tech")))
                {
                    tablesName.Add(matchedTables[i].Value);

                }
            }
            return tablesName.Distinct();
        }
        private string GetOracleQuery(string query,List<string> tablesName)
        {
            query = "select " + query;
            string formExp = "";
            string joinExp = "";
            string whereExp = "";

            string lastDate = DateTime.Now.Year.ToString() +
                              DateTime.Now.ToString("MM") +
                           Convert.ToInt32(Convert.ToString(Convert.ToInt32(DateTime.Now.ToString("dd"))-1)).ToString("00");

            formExp += " from " + tablesName[0] + " " + tablesName[0];

            if(tablesName.Count()>1)
            {
                foreach (var item in tablesName.Select((value, i) => new { i, value }))
                {
                    if (item.i > 0)
                    {
                        joinExp += " join " + item.value + " " + item.value + " on " + item.value + ".Cell_Id=" +
                            item.value[item.i - 1] + ".Cell_Id";
                    }
                }
            }
            
            whereExp += " where " + tablesName[0] + ".c_date between " + lastDate+"00" + " and " + lastDate+"23";
            query +=  formExp + joinExp + whereExp;
            return query;




        }
        private void deleteOperation(int id)
        {
            var operation = _db.Operations.Include(x => x.Childs).FirstOrDefault(x => x.Id == id);
            foreach (var childOp in operation.Childs)
            {
                deleteOperation(childOp.Id);
            }
            _db.Remove(operation);
        }
    }
}
    
 