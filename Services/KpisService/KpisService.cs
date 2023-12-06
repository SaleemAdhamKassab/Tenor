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
using System.DirectoryServices.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using Tenor.Helper;

namespace Tenor.Services.KpisService
{
    public interface IKpisService
    {
        Task<DataWithSize> GetListAsync(KpiFilterModel kpiFilterModel);
        Task<ResultWithMessage> GetByIdAsync(int id);
        Task<ResultWithMessage> Add(CreateKpi kpiModel);
        Task<ResultWithMessage> Update(CreateKpi Kpi);
        Task<ResultWithMessage> Delete(int id);
        Task<ResultWithMessage> GetExtraFields();

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
        public async Task<DataWithSize> GetListAsync(KpiFilterModel kpiFilterModel)
        {
            try
            {

                var kpis = _db.Kpis.Include(x => x.KpiFieldValues).ThenInclude(x=>x.KpiField).
                    ThenInclude(x=>x.ExtraField).AsQueryable();

                if (!string.IsNullOrEmpty(kpiFilterModel.SearchQuery))
                {
                    kpis = kpis.Where(x => x.Name.ToLower().StartsWith(kpiFilterModel.SearchQuery.ToLower()));
                }
                var kpiList = kpis.Select(x => new KpiListViewModel()
                {
                    Id=x.Id,
                    Name=x.Name,
                    DeviceId=x.DeviceId,
                    DeviceName=x.Device.Name,
                    KpiFileds=_mapper.Map<List<KpiFieldValueViewModel>>(x.KpiFieldValues)
                });

                if (kpiFilterModel.filters.Count != 0)
                {
                    foreach (var c in kpiFilterModel.filters)
                    {
                        var filterKeyProperty = typeof(KpiListViewModel).GetProperty(c.key);
                       
                        if (filterKeyProperty.PropertyType == typeof(string))
                        {
                            string dataValues = Convert.ToString(c.values);

                            kpiList = kpiList.Where(x => filterKeyProperty.GetValue(x).ToString().ToLower().StartsWith(dataValues.ToLower()));

                          
                        }
                        else
                        {
                            string dataValues = Convert.ToString(string.Join(",", c.values));
                            kpiList = kpiList.Where(x => dataValues.Contains(filterKeyProperty.GetValue(x).ToString()));

                        }

                    }
                }

                if (!string.IsNullOrEmpty(kpiFilterModel.SortActive))
                {

                    var sortProperty = typeof(KpiViewModel).GetProperty(kpiFilterModel.SortActive);
                    if (sortProperty != null && kpiFilterModel.SortDirection == "asc")
                        kpiList = kpiList.OrderBy2(kpiFilterModel.SortActive);

                    else if (sortProperty != null && kpiFilterModel.SortDirection == "desc")
                        kpiList = kpiList.OrderByDescending2(kpiFilterModel.SortActive);

                    int Count = kpiList.Count();

                    var result = kpiList.Skip((kpiFilterModel.PageIndex - 1) * kpiFilterModel.PageSize)
                    .Take(kpiFilterModel.PageSize).ToList();

                    return new DataWithSize(Count,result);
                }


                else
                {
                    int Count = kpiList.Count();

                    var result = kpiList.Skip((kpiFilterModel.PageIndex - 1) * kpiFilterModel.PageSize)
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
                    return new ResultWithMessage(kpi, null);
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
                    return new ResultWithMessage(updatedKpi, null);
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