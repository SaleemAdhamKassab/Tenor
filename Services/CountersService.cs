using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Runtime.CompilerServices;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services
{
    public interface ICountersService
    {
        Task<List<CounterDto>> GetAsync(CounterFilterModel CounterFilterModel);
        Task<ResultWithMessage> GetAsync(int id);
        Task<ResultWithMessage> Add(CounterDto CounterDto);
        Task<ResultWithMessage> Update(int id, CounterDto CounterDto);
        Task<ResultWithMessage> Delete(int id);
    }

    public class CountersService : ICountersService
    {
        public CountersService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        private readonly TenorDbContext _db;



        public async Task<List<CounterDto>> GetAsync(CounterFilterModel CounterFilterModel)
        {
            IQueryable<Counter> Counters = _db.Counters;

            if (!String.IsNullOrEmpty(CounterFilterModel.SearchQuery))
                Counters = Counters.Where(s => s.Name.ToLower().Contains(CounterFilterModel.SearchQuery.Trim().ToLower()));

            if (String.IsNullOrEmpty(CounterFilterModel.SortDirection))
                Counters = Counters.OrderBy(s => s.Name);
            else
                Counters = Counters.OrderByDescending(s => s.Name);

            //Counters.Skip(CounterFilterModel.PageIndex).Take(CounterFilterModel.PageSize);
            Counters.Skip((CounterFilterModel.PageIndex - 1) * CounterFilterModel.PageSize).Take(CounterFilterModel.PageSize);

            return await Counters
               .Select(e => new CounterDto()
               {
                   Id = e.Id,
                   SupplierId = e.SupplierId,
                   Description = e.Description,
                   Name = e.Name,
                   IsDeleted = e.IsDeleted,

               })
               .ToListAsync();
        }

        public async Task<ResultWithMessage> GetAsync(int id)
        {
            CounterDto Counter = await _db.Counters
                .Select(e => new CounterDto()
                {
                    //Id = id,
                    //SupplierId = e.SupplierId,
                    //Name = e.Name,
                    //Description = e.Description,
                    //IsDeleted = e.IsDeleted,
                    //ParentId = e.ParentId
                })
                .SingleOrDefaultAsync(e => e.Id == id);

            return new ResultWithMessage(Counter, "");
        }


        public async Task<ResultWithMessage> Add(CounterDto counterDto)
        {
            Counter counter = new()
            {
                SupplierId = counterDto.SupplierId,
                Name = counterDto.Name,
                Code = counterDto.Code,
                ColumnName = counterDto.ColumnName,
                RefColumnName = counterDto.RefColumnName,
                Description = counterDto.Description,
                Aggregation = counterDto.Aggregation,
                IsDeleted = counterDto.IsDeleted,
                SubsetId = counterDto.SubsetId,
            };


            _db.Add(counter);
            _db.SaveChangesAsync();

            return new ResultWithMessage(counterDto, "");
        }

        public async Task<ResultWithMessage> Update(int id, CounterDto counterDto)
        {
            Counter counter = await _db.Counters.FindAsync(id);

            if (counter is null)
                return new ResultWithMessage(null, $"Not found Counter with id: {id}");

            counter.Id = counterDto.Id;
            counter.SupplierId = counterDto.SupplierId;
            counter.Name = counterDto.Name;
            counter.Code = counterDto.Code;
            counter.ColumnName = counterDto.ColumnName;
            counter.RefColumnName = counterDto.RefColumnName;
            counter.Description = counterDto.Description;
            counter.Aggregation = counterDto.Aggregation;
            counter.IsDeleted = counterDto.IsDeleted;
            counter.SubsetId = counterDto.SubsetId;


            _db.Update(counter);
            _db.SaveChangesAsync();

            return new ResultWithMessage(counterDto, "");
        }

        public async Task<ResultWithMessage> Delete(int id)
        {
            Counter counter = await _db.Counters.FindAsync(id);

            if (counter is null)
                return new ResultWithMessage(null, $"Not found Counter with id: {id}");

            counter.IsDeleted = true;
            _db.Update(counter);
            _db.SaveChanges();

            CounterDto counterDto = new()
            {
            };

            return new ResultWithMessage(counterDto, "");
        }
    }
}