using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.DevicesService.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Tenor.Helper.Constant;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;

namespace Tenor.Services.DevicesService
{
    public interface IDevicesService
    {
        ResultWithMessage getById(int id);
        ResultWithMessage getByFilter(DeviceFilterModel filter, TenantDto authUser);
        ResultWithMessage getSubsets();
        bool isDeviceExists(int id);
        ResultWithMessage add(DeviceBindingModel model);
        ResultWithMessage edit(int id, DeviceBindingModel subsetDto);
        ResultWithMessage delete(int id);
        FileBytesModel exportDevicesByFilter(DeviceFilterModel filter, TenantDto authUser);
        ResultWithMessage validateDevice(int deviceId, string name);
        ResultWithMessage GetDeviceByParentId(int parentid,string searchQuery, TenantDto authUser);
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
        private IQueryable<Device> getDevicesData(DeviceFilterModel filter, TenantDto authUser)
        {
            IQueryable<Device> qeury = _db.Devices.Include(e => e.Parent).Where(e=> authUser.deviceAccesses.Select(x=>x.DeviceId).ToList().Contains(e.Id));

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                qeury = qeury.Where
                    (e => e.Id.ToString().Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                     e.SupplierId.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                     e.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                     e.ParentId.ToString().Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                     e.Parent.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower())
                );

            return qeury;
        }
        private IQueryable<DeviceListViewModel> convertDevicesToListViewModel(IQueryable<Device> model) =>
          model.Select(e => new DeviceListViewModel
          {
              Id = e.Id,
              Name = e.Name,
              Description = e.Description,
              IsDeleted = e.IsDeleted,
              SupplierId = e.SupplierId,
              ParentId = e.ParentId,
              ParentName = e.Parent.Name
          });
        private string getValidatingModelErrorMessage(DeviceBindingModel model, bool isUpdateMode = false)
        {
            if (model is null)
                return "Empty Model!";

            if (model.ParentId != 0 && !_db.Devices.Any(e => e.Id == model.ParentId))
                return $"Invalid Parent Id: {model.ParentId}!";

            if (!isUpdateMode && _db.Devices.Any(e => e.Name.Trim().ToLower() == model.Name.Trim().ToLower()))
                return $"The Device Name [{model.Name}] is already used!";

            return string.Empty;
        }


        public ResultWithMessage getById(int id)
        {
            Device device = _db.Devices.Find(id);

            if (device is null)
                return new ResultWithMessage(null, $"No Device found with Id: {id}");


            DeviceViewModel deviceViewModel = _db.Devices
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

            return new ResultWithMessage(deviceViewModel, "");
        }
        public ResultWithMessage getByFilter(DeviceFilterModel filter, TenantDto authUser)
        {
            //1- Apply Filters just search query
            var query = getDevicesData(filter, authUser);

            //2- Generate List View Model
            var queryViewModel = convertDevicesToListViewModel(query);

            //3- Sorting using our extension
            filter.SortActive = filter.SortActive == string.Empty ? "Id" : filter.SortActive;

            if (filter.SortDirection == enSortDirection.desc.ToString())
                queryViewModel = queryViewModel.OrderByDescending2(filter.SortActive);
            else
                queryViewModel = queryViewModel.OrderBy2(filter.SortActive);


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
        public bool isDeviceExists(int id) => _db.Devices.Any(e => e.Id == id);
        public ResultWithMessage add(DeviceBindingModel model)
        {
            string validatingModelErrorMessage = getValidatingModelErrorMessage(model);

            if (!string.IsNullOrEmpty(validatingModelErrorMessage))
                return new ResultWithMessage(null, validatingModelErrorMessage);

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

                device = _db.Devices.Include(e => e.Parent).Single(e => e.Id == device.Id);

                DeviceViewModel deviceViewModel = new()
                {
                    Id = device.Id,
                    Name = device.Name,
                    Description = device.Description,
                    IsDeleted = device.IsDeleted,
                    ParentId = device.ParentId,
                    ParentName = device.ParentId is null ? null : device.Parent.Name,
                    SupplierId = device.SupplierId
                };

                return new ResultWithMessage(deviceViewModel, "");
            }
            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }
        public ResultWithMessage edit(int id, DeviceBindingModel model)
        {
            model.Id = id;
            if (model is null)
                return new ResultWithMessage(null, "Empty Model!!");

            string validatingModelErrorMessage = getValidatingModelErrorMessage(model, true);

            if (!string.IsNullOrEmpty(validatingModelErrorMessage))
                return new ResultWithMessage(null, validatingModelErrorMessage);

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

                device = _db.Devices.Include(e => e.Parent).Single(e => e.Id == device.Id);


                DeviceViewModel deviceViewModel = new()
                {
                    Id = device.Id,
                    Name = device.Name,
                    Description = device.Description,
                    IsDeleted = device.IsDeleted,
                    ParentId = device.ParentId,
                    ParentName = device.ParentId is null ? null : device.Parent.Name,
                    SupplierId = device.SupplierId
                };

                return new ResultWithMessage(deviceViewModel, "");
            }

            catch (Exception e)
            {
                return new ResultWithMessage(model, e.Message);
            }
        }
        public ResultWithMessage delete(int id)
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
        //Export Data With Excel
        public FileBytesModel exportDevicesByFilter(DeviceFilterModel filter, TenantDto authUser)
        {
            var list = getDevicesData(filter, authUser);
            var result = convertDevicesToListViewModel(list);

            if (result == null || result.Count() == 0)
                return new FileBytesModel();

            FileBytesModel excelfile = new();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var stream = new MemoryStream();
            var package = new ExcelPackage(stream);
            var workSheet = package.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells.LoadFromCollection(result, true);

            List<int> dateColumns = new();
            int datecolumn = 1;
            foreach (var PropertyInfo in result.FirstOrDefault().GetType().GetProperties())
            {
                if (PropertyInfo.PropertyType == typeof(DateTime) || PropertyInfo.PropertyType == typeof(DateTime?))
                {
                    dateColumns.Add(datecolumn);
                }
                datecolumn++;
            }
            dateColumns.ForEach(item => workSheet.Column(item).Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss AM/PM");
            package.Save();
            excelfile.Bytes = stream.ToArray();
            stream.Position = 0;
            stream.Close();
            string excelName = $"Posts-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excelfile.FileName = excelName;
            excelfile.ContentType = contentType;
            return excelfile;
        }
        public ResultWithMessage validateDevice(int deviceId, string name)
        {

            Device device = _db.Devices.SingleOrDefault(e => e.Id == deviceId || e.Name.Trim().ToLower() == name.Trim().ToLower());

            if (device is not null)
                return new ResultWithMessage(null, $"The Device with Id: {device.Id} and name: '{device.Name}' is already exists");

            return new ResultWithMessage(true,string.Empty);
        }
        public ResultWithMessage GetDeviceByParentId(int parentid, string searchQuery, TenantDto authUser)
        {

            IQueryable<Device> query = _db.Devices.Include(e => e.Parent).Where(e => e.ParentId == parentid && authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains(parentid));

            if(query==null || query.Count()==0)
            {
                return new ResultWithMessage(new DataWithSize(0, null), "Access denied to this device or device is invalid");

            }
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(x => x.Id.ToString() == searchQuery || x.Name.ToLower().Contains(searchQuery.ToLower())
                      || x.Subsets.Any(y => y.Id.ToString() == searchQuery || y.Name.ToLower().Contains(searchQuery.ToLower()))
                      || x.Subsets.Any(z => z.Counters.Any(e => e.Id.ToString() == searchQuery || e.Name.ToLower().Contains(searchQuery.ToLower()) || e.Code.ToLower() == searchQuery.ToLower()))
                      );
            }
            //var queryViewModel = convertDevicesToListViewModel(query);
            //return new ResultWithMessage(new DataWithSize(queryViewModel.Count(), queryViewModel.ToList()),null);
            var result = query.Select(x => new TreeNodeViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = "device",
                HasChild = x.Subsets.Count() > 0
            }).ToList();
            return new ResultWithMessage(result, "");

        }
    }
}