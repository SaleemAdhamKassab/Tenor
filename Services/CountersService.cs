using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services
{
    public interface ICounterService
    {
        Task<ResultWithMessage> GetAllSets();
        Task<ResultWithMessage> GetAllCounters(long subsetId);
    }
    public class CountersService : ICounterService
    {
        private readonly TenorDbContext _db;

        public CountersService(TenorDbContext tenorDbContext) => _db = tenorDbContext;


        private IEnumerable<object> recSetChilds(List<MainSet> sets)
        {
            if (sets.Count() == 0)
            {
                return null;
            }
            var result = sets.Select(s => new
            {
                s.Id,
                s.Name,
                Subsets = s.Subsets.Select(x => new
                {
                    x.Id,
                    x.Name
                }),
                childs = recSetChilds(_db.MainSets.Include(x => x.Subsets).Where(x => x.ParentId == s.Id).ToList())
            });
            return result.ToList();
        }
        public async Task<ResultWithMessage> GetAllSets()
        {
            var sets = recSetChilds(_db.MainSets.Include(x => x.Subsets).Where(x => x.ParentId == null).ToList());
            return new ResultWithMessage(sets.ToList(), "");
        }
        public async Task<ResultWithMessage> GetAllCounters(long subsetId)
        {
            List<Counter> counters = await _db.Counters.Where(e => e.SubsetId == subsetId).OrderBy(e => e.Id).ToListAsync();

            return new ResultWithMessage(counters, "");
        }


    }
}
