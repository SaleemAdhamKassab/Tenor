using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services
{
    public interface IKpisService
    {
        Task<List<Kpi>> GetAsync(KpiFilterModel kpiFilterModel);
        Task<ResultWithMessage> GetAsync(int id);
        Task<ResultWithMessage> Add(CreateKpi kpiModel);
        Task<ResultWithMessage> Update(UpdateKpi Kpi);
        Task<ResultWithMessage> Delete(int id);
    }

    public class KpisService : IKpisService
    {
        private readonly TenorDbContext _db;
        private readonly IMapper _mapper;
        public KpisService(TenorDbContext tenorDbContext, IMapper mapper)
        {
            _db = tenorDbContext;
            _mapper=mapper;
        }
        public async Task<List<Kpi>> GetAsync(KpiFilterModel kpiFilterModel)
        {
            return await _db.Kpis
               .Select(e => new Kpi()
               {

               })
               .ToListAsync();
        }

        public async Task<ResultWithMessage> GetAsync(int id)
        {
            return new ResultWithMessage(null, "");
        }


        public async Task<ResultWithMessage> Add(CreateKpi kpi)
        {
            try
            {
                Kpi newKpi = _mapper.Map<Kpi>(kpi);
                _db.Kpis.Add(newKpi);
                _db.SaveChanges();
                return new ResultWithMessage(kpi, "");
            }
            catch(Exception ex)
            {
                return new ResultWithMessage(null, ex.Message);

            }

        }

        public async Task<ResultWithMessage> Update(UpdateKpi Kpi)
        {
            try
            {
                Kpi oldKpi = _db.Kpis.AsNoTracking().FirstOrDefault(x => x.Id == Kpi.Id);
                if (oldKpi == null)
                {
                    return new ResultWithMessage(null, "This Id is invalid");
                }
                //----------------remove relation
                
                Kpi updatedKpi = _mapper.Map<Kpi>(Kpi);
                _db.Update(updatedKpi);
                _db.SaveChanges();
                //Remove old childs relation
                DeleteSelfRelation(oldKpi.OperationId, null);
                return new ResultWithMessage(updatedKpi, null);
            }
            catch(Exception ex)
            {
                return new ResultWithMessage(null,ex.Message);

            }
        }

        public async Task<ResultWithMessage> Delete(int id)
        {
            return new ResultWithMessage(null, "");
        }

        private bool DeleteSelfRelation(int? parentid,List<int> childid)
        {
            List<Operation> childOpt = new List<Operation>();
            //remove main parent
            if (parentid!=null)
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
           if (childOpt.Count==0)
           {
             _db.SaveChanges();
                return true;
           }
            foreach(var s in childOpt)
            {
              _db.Operations.Remove(s);
            }
            _db.SaveChanges();
            DeleteSelfRelation(null,childOpt.Select(x=>x.Id).ToList());
            return true;
        }

    }
}