using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.DevicesService.ViewModels;
using static Tenor.Helper.Constant;

namespace Tenor.Services.DevicesService
{
    public interface IDevicesService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter(DeviceFilterModel filter);
        ResultWithMessage getSubsets();
        ResultWithMessage addDevice(DeviceBindingModel model);
        ResultWithMessage updateDevice(DeviceBindingModel subsetDto);
        ResultWithMessage deleteDevice(int id);
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

        private IQueryable<Device> getDevicesData(DeviceFilterModel filter)
        {
            IQueryable<Device> qeury = _db.Devices.Where(e => true);

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                qeury = qeury.Where(e => e.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()));

            return qeury;
        }

        private IQueryable<DeviceListViewModel> convertDevicesToViewModel(IQueryable<Device> model) =>
          model.Select(e => new DeviceListViewModel
          {
              Id = e.Id,
              Name = e.Name,
              Description = e.Description,
              IsDeleted = e.IsDeleted,
              SupplierId = e.SupplierId,
              ParentId = e.ParentId
          });



        public ResultWithMessage getById(int id)
        {
            DeviceViewModel device = _db.Devices
                .Select(e => new DeviceViewModel()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    IsDeleted = e.IsDeleted,
                    SupplierId = e.SupplierId,
                    ParentId = e.ParentId,
                    ParentName = e.Parent.Name
                })
                .First(e => e.Id == id);

            return new ResultWithMessage(device, "");
        }


        public ResultWithMessage getByFilter(DeviceFilterModel filter)
        {
            //1- Apply Filters just search query
            var query = getDevicesData(filter);

            //2- Generate List View Model
            var queryViewModel = convertDevicesToViewModel(query);

            //3- Sorting using our extension
            filter.SortActive = filter.SortActive == string.Empty ? "Id" : filter.SortActive;

            if (filter.SortDirection == enSortDirection.desc.ToString())
                queryViewModel = queryViewModel.OrderByDescending(filter.SortActive);
            else
                queryViewModel = queryViewModel.OrderBy(filter.SortActive);


            //4- pagination
            int resultSize = queryViewModel.Count();
            var resultData = queryViewModel.Skip(filter.PageSize * filter.PageIndex).Take(filter.PageSize).ToList();

            //5- return 
            return new ResultWithMessage(new DataWithSize(resultSize, resultData), "");
        }

        public ResultWithMessage getSubsets()
        {
            var devicesWithChilds = _recSetChilds(_db.Devices.Include(x => x.Subsets).Where(x => x.ParentId == null).ToList());
            return new ResultWithMessage(devicesWithChilds, "");
        }

        public ResultWithMessage addDevice(DeviceBindingModel model)
        {
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            Device device = new()
            {
                SupplierId = model.SupplierId,
                Name = model.Name,
                Description = model.Description,
                IsDeleted = model.IsDeleted,
                ParentId = model.ParentId == 0 ? null : model.ParentId
            };


            try
            {
                _db.Add(device);
                _db.SaveChanges();

                DeviceViewModel deviceViewModel = new()
                {
                    Id = device.Id,
                    Name = device.Name,
                    Description = device.Description,
                    IsDeleted = device.IsDeleted,
                    ParentId = device.ParentId,
                    ParentName = device.Parent.Name,
                    SupplierId = device.SupplierId
                };


                return new ResultWithMessage(deviceViewModel, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage updateDevice(DeviceBindingModel model)
        {
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            Device device = _db.Devices.Find(model.Id);

            if (device is null)
                return new ResultWithMessage(null, $"Not found device with id: {model.Id}");

            device.Name = model.Name;
            device.Description = model.Description;
            device.SupplierId = model.SupplierId;
            device.ParentId = model.ParentId == 0 ? null : model.ParentId;
            device.IsDeleted = model.IsDeleted;


            try
            {
                _db.Update(device);
                _db.SaveChanges();

                DeviceViewModel deviceViewModel = new()
                {
                    Id = device.Id,
                    Name = device.Name,
                    Description = device.Description,
                    IsDeleted = device.IsDeleted,
                    ParentId = device.ParentId,
                    ParentName = device.Parent.Name,
                    SupplierId = device.SupplierId
                };

                return new ResultWithMessage(deviceViewModel, "");
            }

            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }

        public ResultWithMessage deleteDevice(int id)
        {
            Device device = _db.Devices.Find(id);

            if (device is null)
                return new ResultWithMessage(null, $"Not found device with id: {id}");

            try
            {
                device.IsDeleted = true;
                _db.Update(device);
                _db.SaveChanges();

                return new ResultWithMessage(null, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(null, e.Message);
            }
        }
    }
}