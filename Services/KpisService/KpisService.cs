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

namespace Tenor.Services.KpisService
{
    public interface IKpisService
    {
        Task<DataWithSize> GetListAsync(object kpiFilterM);
        Task<ResultWithMessage> GetByIdAsync(int id);
        Task<ResultWithMessage> Add(CreateKpi kpiModel);
        Task<ResultWithMessage> Update(CreateKpi Kpi);
        Task<ResultWithMessage> Delete(int id);
        Task<ResultWithMessage> GetExtraFields();
        ResultWithMessage GetOperators();
        ResultWithMessage GetFunctions();

    }

    public class KpisService : IKpisService
    {
        private readonly TenorDbContext _db;
        private readonly IMapper _mapper;
        public KpisService(TenorDbContext tenorDbContext, IMapper mapper)
        {
            _db = tenorDbContext;
            _mapper = mapper;
        }
        public async Task<DataWithSize> GetListAsync(object kpiFilterM)
        {
            try
            {

                List<Filter> filters = new List<Filter>();
                var kpis = _db.Kpis.Include(x => x.KpiFieldValues).ThenInclude(x=>x.KpiField).
                    ThenInclude(x=>x.ExtraField).AsQueryable();
                List<string> kpiFields = _db.KpiFields.Include(x => x.ExtraField).Select(x => x.ExtraField.Name).ToList();
                List<string> mainFields = new List<string>() {"id", "deviceId" };
                kpiFields.AddRange(mainFields);                //--------------------------------------------------------------
                dynamic data = JsonConvert.DeserializeObject<dynamic>(kpiFilterM.ToString());
                KpiFilterModel kpiFilterModel = new KpiFilterModel()
                {
                    SearchQuery= data["searchQuery"],
                    PageIndex = data["pageIndex"],
                    PageSize = data["pageSize"],
                    SortActive = data["sortActive"],
                    SortDirection = data["sortDirection"]

                };
                //--------------------------------------------------------------
                foreach (var s in kpiFields)
                {
                    Filter filter = new Filter();
                    object property = data[s];
                    if (property!=null)
                    {
                        filter.key = s;
                        filter.values= Regex.Replace(property.ToString().Replace("{", "").Replace("}", ""), @"\t|\n|\r|\s+", "");
                        filters.Add(filter);
                    }
                }
              
                if (!string.IsNullOrEmpty(kpiFilterModel.SearchQuery))
                {
                    kpis = kpis.Where(x => x.Name.ToLower().StartsWith(kpiFilterModel.SearchQuery.ToLower()));
                }
                
                if (filters.Count != 0)
                {
                    var expression = ExpressionUtils.BuildPredicate<Kpi>(filters);
                    if (expression != null)
                    {
                        kpis = kpis.Where(expression);
                    }
                    else
                    {
                        foreach (var f in filters)
                        {                        
                          string fileds = Convert.ToString(string.Join(",", f.values));
                          string convertFields = fileds.Replace("\",\"", ",").Replace("[", "").Replace("]", "").Replace("\"","");
                          kpis = kpis.Where(x => x.KpiFieldValues.Any(y => y.FieldValue.Contains(convertFields) && y.KpiField.ExtraField.Name == f.key));                     
                        }
                    }
                }
                               
                var kpiList = kpis.Select(x => new KpiListViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    DeviceId = x.DeviceId,
                    DeviceName = x.Device.Name,
                    KpiFileds = _mapper.Map<List<KpiFieldValueViewModel>>(x.KpiFieldValues)
                });

                if (!string.IsNullOrEmpty(kpiFilterModel.SortActive))
                {

                    var sortProperty = typeof(KpiListViewModel).GetProperty(kpiFilterModel.SortActive);
                    if (sortProperty != null && kpiFilterModel.SortDirection == "asc")
                        kpiList = kpiList.OrderBy2(kpiFilterModel.SortActive);

                    else if (sortProperty != null && kpiFilterModel.SortDirection == "desc")
                        kpiList = kpiList.OrderByDescending2(kpiFilterModel.SortActive);

                    int Count = kpiList.Count();

                    var result = kpiList.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                    .Take(kpiFilterModel.PageSize).ToList();

                    return new DataWithSize(Count,result);
                }

                else
                {
                    int Count = kpiList.Count();

                    var result = kpiList.Skip((kpiFilterModel.PageIndex) * kpiFilterModel.PageSize)
                    .Take(kpiFilterModel.PageSize).ToList();

                    return new DataWithSize(Count, result);
                }

            }
            catch (Exception ex)
            {
                return new DataWithSize(0,ex.Message);

            }
        }

        public async Task<ResultWithMessage> GetByIdAsync(int id)
        {
            var kpi = _db.Kpis.Include(x=>x.Device).Include(x => x.Operation).Include(x => x.KpiFieldValues)
                 .ThenInclude(x => x.KpiField).ThenInclude(x => x.ExtraField)
                 .FirstOrDefault(x => x.Id == id);

            if(kpi==null)
            {
                return new ResultWithMessage(null, "This KPI Id is invalid");

            }
            GetSelfRelation(kpi.OperationId);
            var kpiMap = _mapper.Map<KpiViewModel>(kpi);
            return new ResultWithMessage(kpiMap, null);
        }

        public async Task<ResultWithMessage> Add(CreateKpi kpi)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    Kpi newKpi = _mapper.Map<Kpi>(kpi);
                    _db.Kpis.Add(newKpi);
                    _db.SaveChanges();

                    if (kpi.KpiFields.Count != 0)
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

        public async Task<ResultWithMessage> Update(CreateKpi Kpi)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    Kpi oldKpi = _db.Kpis.AsNoTracking().FirstOrDefault(x => x.Id == Kpi.Id);
                    if (oldKpi == null)
                    {
                        return new ResultWithMessage(null, "This Id is invalid");
                    }
                    Kpi updatedKpi = _mapper.Map<Kpi>(Kpi);
                    _db.Update(updatedKpi);
                    _db.SaveChanges();
                    //Remove old childs relation
                    DeleteSelfRelation(oldKpi.OperationId, null);
                    //Update Kpi field values
                    if (Kpi.KpiFields.Count != 0)
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
            return new ResultWithMessage(null, "");
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
            var func = _db.Functions.ToList();
            return new ResultWithMessage(func, null);

        }
        private bool DeleteSelfRelation(int? parentid, List<int> childid)
        {
            List<Operation> childOpt = new List<Operation>();
            //remove main parent
            if (parentid != null)
            {
                var OldRelation = _db.Operations.FirstOrDefault(x => x.Id == parentid);
                _db.Operations.Remove(OldRelation);
                childOpt = _db.Operations.Where(x => x.ParentId == parentid).ToList();
            }
            else
            {
                childOpt = _db.Operations.Where(x => childid.Contains((int)x.ParentId)).ToList();
            }

            //-------------------remove cascade
            if (childOpt.Count == 0)
            {
                _db.SaveChanges();
                return true;
            }
            foreach (var s in childOpt)
            {
                _db.Operations.Remove(s);
            }
            _db.SaveChanges();
            DeleteSelfRelation(null, childOpt.Select(x => x.Id).ToList());
            return true;
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
            var opt = _db.Operations.Include(x=>x.Function).Include(x => x.Counter)
                .Include(x => x.Kpi).Include(x => x.Operator)
                .Include(x=>x.Childs).FirstOrDefault(x=>x.Id==optid);

            result.Add(_mapper.Map<OperationDto>(opt));
            if(opt.Childs.Count!=0)
            {
                foreach(var s in opt.Childs)
                {
                    GetSelfRelation(s.Id);
                }

            }

            return result;
        }

    }
}