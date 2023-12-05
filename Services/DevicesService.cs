using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;

namespace Tenor.Services
{
    public interface IDevicesService
    {
        //Task<List<DeviceDto>> GetAsync(DeviceFilterModel deviceFilterModel);
        public ResultWithMessage getDevicesByFilter(DeviceFilterModel deviceFilterModel);
        Task<ResultWithMessage> GetAsync(int id);
        Task<ResultWithMessage> GetSubsetsAsync();
        Task<ResultWithMessage> Add(DeviceDto subsetDto);
        Task<ResultWithMessage> Update(int id, DeviceDto subsetDto);
        Task<ResultWithMessage> Delete(int id);
    }

    public class DevicesService : IDevicesService
    {
        public DevicesService(TenorDbContext tenorDbContext) => _db = tenorDbContext;

        private readonly TenorDbContext _db;
        private IEnumerable<object> _recSetChilds(List<Device> devices)
        {
            if (devices.Count() == 0)
                return null;

            var result = devices.Select(s => new
            {
                s.Id,
                s.Name,
                Subsets = s.Subsets.Select(x => new
                {
                    x.Id,
                    x.Name
                }),
                childs = _recSetChilds(_db.Devices.Include(x => x.Subsets).Where(x => x.ParentId == s.Id).ToList())
            });
            return result.ToList();
        }


        //public async Task<List<DeviceDto>> GetAsync(DeviceFilterModel deviceFilterModel)
        //{
        //    IQueryable<Device> devices = _db.Devices;

        //    if (!String.IsNullOrEmpty(deviceFilterModel.SearchQuery))
        //        devices = devices.Where(s => s.Name.Trim().ToLower().Contains(deviceFilterModel.SearchQuery.Trim().ToLower()));

        //    if (!String.IsNullOrEmpty(deviceFilterModel.SortDirection))
        //        devices = devices.OrderBy(s => s.Name);
        //    else
        //        devices = devices.OrderByDescending(s => s.Name);

        //    return await devices
        //       .Select(e => new DeviceDto()
        //       {
        //           Id = e.Id,
        //           SupplierId = e.SupplierId,
        //           Description = e.Description,
        //           Name = e.Name,
        //           IsDeleted = e.IsDeleted,
        //           ParentId = e.ParentId
        //       })
        //       .Skip((deviceFilterModel.PageIndex - 1) * deviceFilterModel.PageSize)
        //       .Take(deviceFilterModel.PageSize)
        //       .ToListAsync();
        //}

        private IQueryable<Device> getDevicesData(DeviceFilterModel filter)
        {
            // 1- sortActive
            filter.SortActive = filter.SortActive == null ? "Id" : "here";

            //2- searchquery
            IQueryable<Device> qeury = _db.Devices.Where(e => true);

            if (!string.IsNullOrEmpty(filter.Name))
                qeury = qeury.Where(e => e.Name.Trim().ToLower().Contains(filter.Name.Trim().ToLower()));


            if (filter.IsDeleted)
                qeury = qeury.Where(e => e.IsDeleted);
            else
                qeury = qeury.Where(e => !e.IsDeleted);

            return qeury;
        }

        private IQueryable<DeviceListViewModel> convertDevicesToViewModel(IQueryable<Device> model) =>
          model.Select(e => new DeviceListViewModel
          {
              Id = e.Id,
              Name = e.Name,
              Description = e.Description,
              IsDeleted = e.IsDeleted
          });

        public ResultWithMessage getDevicesByFilter(DeviceFilterModel filter)
        {
            //1- Apply Filters
            var query = getDevicesData(filter);

            //2- Generate List View Model
            var queryViewModel = convertDevicesToViewModel(query);

            //3- Sorting using our extension



            //4- pagination
            int resultSize = queryViewModel.Count();
            var resultData = queryViewModel.Skip(filter.PageSize * filter.PageIndex).Take(filter.PageSize).ToList();

            //5- return 
            return new ResultWithMessage(new DataWithSize(resultSize, resultData), "");
        }

        public async Task<ResultWithMessage> GetAsync(int id)
        {
            DeviceDto device = await _db.Devices
                .Select(e => new DeviceDto()
                {
                    Id = id,
                    SupplierId = e.SupplierId,
                    Name = e.Name,
                    Description = e.Description,
                    IsDeleted = e.IsDeleted,
                    ParentId = e.ParentId
                })
                .SingleOrDefaultAsync(e => e.Id == id);

            return new ResultWithMessage(device, "");
        }

        public async Task<ResultWithMessage> GetSubsetsAsync()
        {
            var devicesWithChilds = _recSetChilds(await _db.Devices.Include(x => x.Subsets).Where(x => x.ParentId == null).ToListAsync());
            return new ResultWithMessage(devicesWithChilds, "");
        }

        public async Task<ResultWithMessage> Add(DeviceDto deviceDto)
        {
            Device device = new()
            {
                SupplierId = deviceDto.SupplierId,
                Name = deviceDto.Name,
                Description = deviceDto.Description,
                IsDeleted = deviceDto.IsDeleted,
                ParentId = deviceDto.ParentId
            };

            _db.Add(device);
            _db.SaveChangesAsync();

            return new ResultWithMessage(deviceDto, "");
        }

        public async Task<ResultWithMessage> Update(int id, DeviceDto deviceDto)
        {
            Device device = await _db.Devices.FindAsync(id);

            if (device is null)
                return new ResultWithMessage(null, $"Not found device with id: {id}");

            device.SupplierId = deviceDto.SupplierId;
            device.Name = deviceDto.Name;
            device.Description = deviceDto.Description;
            device.IsDeleted = deviceDto.IsDeleted;
            device.ParentId = deviceDto.ParentId;
            deviceDto.Id = device.Id;

            _db.Update(device);
            _db.SaveChangesAsync();

            return new ResultWithMessage(deviceDto, "");
        }

        public async Task<ResultWithMessage> Delete(int id)
        {
            Device device = await _db.Devices.FindAsync(id);

            if (device is null)
                return new ResultWithMessage(null, $"Not found device with id: {id}");

            device.IsDeleted = true;
            _db.Update(device);
            _db.SaveChanges();

            DeviceDto deviceDto = new()
            {
                Id = device.Id,
                SupplierId = device.SupplierId,
                Name = device.Name,
                Description = device.Description,
                IsDeleted = device.IsDeleted,
                ParentId = device.ParentId
            };

            return new ResultWithMessage(deviceDto, "");
        }
    }
}