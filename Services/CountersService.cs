using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services
{
    public interface ICounterService
    {
        Task<ResultWithMessage> GetAllSets();
    }
    public class CountersService : ICounterService
    {
        private readonly TenorDbContext _db;
        public CountersService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        public async Task<ResultWithMessage> GetAllSets()
        {
            //var sets = await _db.MainSets.SelectMany(e => e.Childs.SelectMany(s => s.Subsets.Select(c=>c.Counters)))

            //    //.SelectMany(e => e.Subsets.SelectMany(e => e.Counters))

            //    .ToListAsync();


            //var sets = await _db.MainSets
            //    .Include(e => e.Subsets).
            //    ThenInclude(e => e.Counters)
            //    .ToListAsync();


            var sets = recSetChilds(_db.MainSets.Where(x => x.ParentId == null)).ToList();


            return new ResultWithMessage(sets, "");
        }

        private  IQueryable<object> recSetChilds(IQueryable<MainSet> sets)
        {
            var result = sets.Select(s => new
            {
                s.Id,
                s.Name,
                s.Subsets,
                childs = s.Childs != null && s.Childs.Count() > 0 ? recSetChilds(_db.MainSets.Where(x => x.ParentId == s.Id)).ToList() : null
            });
            return result;
        }
        //private static object recSetChilds(ICollection<MainSet> sets)
        //{
        //    return sets.Select(s => new
        //    {
        //        s.Id,
        //        s.Name,
        //        s.Subsets,
        //        childs = s.Childs != null && s.Childs.Count() > 0 ? recSetChilds(s.Childs) : new List<MainSet>()
        //    });
        //}
    }
}
