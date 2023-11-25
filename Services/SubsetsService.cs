using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Runtime.CompilerServices;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services
{
    public interface ISubsetsService
    {
        Task<List<SubsetDto>> GetAsync(SubsetFilterModel SubsetFilterModel);
        Task<ResultWithMessage> GetAsync(int id);
        Task<ResultWithMessage> Add(SubsetDto subsetDto);
        Task<ResultWithMessage> Update(int id, SubsetDto subsetDto);
        Task<ResultWithMessage> Delete(int id);
    }

    public class SubsetsService : ISubsetsService
    {
        public SubsetsService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        private readonly TenorDbContext _db;



        public async Task<List<SubsetDto>> GetAsync(SubsetFilterModel SubsetFilterModel)
        {
            IQueryable<Subset> Subsets = _db.Subsets;

            if (!String.IsNullOrEmpty(SubsetFilterModel.SearchQuery))
                Subsets = Subsets.Where(s => s.Name.ToLower().Contains(SubsetFilterModel.SearchQuery.Trim().ToLower()));

            if (String.IsNullOrEmpty(SubsetFilterModel.SortDirection))
                Subsets = Subsets.OrderBy(s => s.Name);
            else
                Subsets = Subsets.OrderByDescending(s => s.Name);

            //Subsets.Skip(SubsetFilterModel.PageIndex).Take(SubsetFilterModel.PageSize);
            Subsets.Skip((SubsetFilterModel.PageIndex - 1) * SubsetFilterModel.PageSize).Take(SubsetFilterModel.PageSize);

            return await Subsets
               .Select(e => new SubsetDto()
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
            SubsetDto Subset = await _db.Subsets
                .Select(e => new SubsetDto()
                {
                    //Id = id,
                    //SupplierId = e.SupplierId,
                    //Name = e.Name,
                    //Description = e.Description,
                    //IsDeleted = e.IsDeleted,
                    //ParentId = e.ParentId
                })
                .SingleOrDefaultAsync(e => e.Id == id);

            return new ResultWithMessage(Subset, "");
        }


        public async Task<ResultWithMessage> Add(SubsetDto subsetDto)
        {
            Subset subset = new()
            {
                SupplierId = subsetDto.SupplierId,
                Name = subsetDto.Name,
                Description = subsetDto.Description,
                TableName = subsetDto.TableName,
                RefTableName = subsetDto.RefTableName,
                SchemaName = subsetDto.SchemaName,
                MaxDataDate = subsetDto.MaxDataDate,
                IsLoad = subsetDto.IsLoad,
                DataTS = subsetDto.DataTS,
                IndexTS = subsetDto.IndexTS,
                DbLink = subsetDto.DbLink,
                RefDbLink = subsetDto.RefDbLink,
                IsDeleted = subsetDto.IsDeleted,
                DeviceId = subsetDto.DeviceId
            };


            _db.Add(subset);
            _db.SaveChangesAsync();

            subsetDto.Id = subset.Id;
            return new ResultWithMessage(subsetDto, "");
        }

        public async Task<ResultWithMessage> Update(int id, SubsetDto subsetDto)
        {
            Subset subset = await _db.Subsets.FindAsync(id);

            if (subset is null)
                return new ResultWithMessage(null, $"Not found Subset with id: {id}");

            subset.Id = id;
            subset.SupplierId = subsetDto.SupplierId;
            subset.Name = subsetDto.Name;
            subset.Description = subsetDto.Description;
            subset.TableName = subsetDto.TableName;
            subset.RefTableName = subsetDto.RefTableName;
            subset.SchemaName = subsetDto.SchemaName;
            subset.MaxDataDate = subsetDto.MaxDataDate;
            subset.IsLoad = subsetDto.IsLoad;
            subset.DataTS = subsetDto.DataTS;
            subset.IndexTS = subsetDto.IndexTS;
            subset.DbLink = subsetDto.DbLink;
            subset.RefDbLink = subsetDto.RefDbLink;
            subset.IsDeleted = subsetDto.IsDeleted;
            subset.DeviceId = subsetDto.DeviceId;
            subsetDto.Id = id;

            _db.Update(subset);
            _db.SaveChangesAsync();

            return new ResultWithMessage(subsetDto, "");
        }

        public async Task<ResultWithMessage> Delete(int id)
        {
            Subset subset = await _db.Subsets.FindAsync(id);

            if (subset is null)
                return new ResultWithMessage(null, $"Not found Subset with id: {id}");

            subset.IsDeleted = true;
            _db.Update(subset);
            _db.SaveChanges();

            SubsetDto subsetDto = new()
            {
                Id = subset.Id,
                SupplierId = subset.SupplierId,
                Name = subset.Name,
                Description = subset.Description,
                TableName = subset.TableName,
                RefTableName = subset.RefTableName,
                SchemaName = subset.SchemaName,
                MaxDataDate = (int)subset.MaxDataDate,
                IsLoad = subset.IsLoad,
                DataTS = subset.DataTS,
                IndexTS = subset.IndexTS,
                DbLink = subset.DbLink,
                RefDbLink = subset.RefDbLink,
                IsDeleted = subset.IsDeleted,
                DeviceId = subset.DeviceId
            };

            return new ResultWithMessage(subset, "");
        }
    }
}