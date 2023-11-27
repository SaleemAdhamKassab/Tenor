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
        Task<ResultWithMessage> Add(Kpi kpiModel);
        Task<ResultWithMessage> Update(int id, Kpi Kpi);
        Task<ResultWithMessage> Delete(int id);
    }

    public class KpisService : IKpisService
    {
        public KpisService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        private readonly TenorDbContext _db;



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


        public async Task<ResultWithMessage> Add(Kpi kpi)
        {
            _db.Kpis.Add(kpi);
            _db.SaveChanges();

            return new ResultWithMessage(kpi, "");
        }

        public async Task<ResultWithMessage> Update(int id, Kpi Kpi)
        {
            return new ResultWithMessage(null, "");
        }

        public async Task<ResultWithMessage> Delete(int id)
        {
            return new ResultWithMessage(null, "");
        }
    }
}