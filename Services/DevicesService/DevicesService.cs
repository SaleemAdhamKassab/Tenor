﻿using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq;
using System.Resources;
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
		ResultWithMessage GetDeviceByParentId(int parentid, string searchQuery, TenantDto authUser);
		ResultWithMessage getAllDevices(TenantDto authUser);
		ResultWithMessage getDevicesWithRelations(int ? parentid, string searchQuery, TenantDto authUser);

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
			IQueryable<Device> qeury = null;
			if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
			{
				qeury = _db.Devices.Include(e => e.Parent).Where(e => e.Parent == null);

			}
			else
			{
				qeury = _db.Devices.Include(e => e.Parent).Where(e => e.Parent == null && authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains(e.Id));

			}


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
		private List<Device> getDeviceChilds(ref List<Device> devices, int parentId)
		{
			List<Device> innerDevices = devices;

			return devices.Where(e => e.ParentId == parentId).Select(e => new Device()
			{
				Id = e.Id,
				Name = e.Name,
				ParentId = parentId,
				Description = e.Description,
				SupplierId = e.SupplierId,
				Childs = innerDevices.Where(x => x.ParentId == e.Id).Count() > 0 ? getDeviceChilds(ref innerDevices, e.Id) : null
			}).ToList();

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

			return new ResultWithMessage(true, string.Empty);
		}
		public ResultWithMessage GetDeviceByParentId(int parentid, string searchQuery, TenantDto authUser)
		{
			IQueryable<Device> query = null;
			if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
			{
				query = _db.Devices.Include(e => e.Parent).Where(e => e.ParentId == parentid);

			}
			else
			{
				query = _db.Devices.Include(e => e.Parent).Where(e => e.ParentId == parentid && authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains(parentid));

			}


			if (!string.IsNullOrEmpty(searchQuery))
			{
				query = query.Where(x => x.Id.ToString() == searchQuery || x.Name.ToLower().Contains(searchQuery.ToLower()) || x.SupplierId.ToLower().Contains(searchQuery.ToLower())
					  || x.Subsets.Any(y => y.Id.ToString() == searchQuery || y.Name.ToLower().Contains(searchQuery.ToLower()) || y.SupplierId.ToLower().Contains(searchQuery.ToLower()))
					  || x.Subsets.Any(z => z.Counters.Any(e => e.Id.ToString() == searchQuery || e.Name.ToLower().Contains(searchQuery.ToLower()) || e.Code.ToLower() == searchQuery.ToLower() || e.SupplierId.ToLower().Contains(searchQuery.ToLower())))
					  );
			}
			var result = query.Select(x => new TreeNodeViewModel
			{
				Id = x.Id,
				Name = x.Name,
				Type = "device",
				HasChild = x.Subsets.Count() > 0
			}).ToList();
			return new ResultWithMessage(result, "");

		}

		public ResultWithMessage getAllDevices(TenantDto authUser)
		{
			List<Device> devices = new();

			if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
				devices = _db.Devices.Where(e => !e.IsDeleted).ToList();
			else
			{
				List<string> userDevices = authUser.deviceAccesses.Select(e => e.DeviceName).ToList();
				devices = _db.Devices.Where(e => userDevices.Contains(e.Name)).ToList();
			}

			List<Device> rootDevices = devices.Where(e => string.IsNullOrEmpty(e.ParentId.ToString())).ToList();
			List<Device> subList = new();
			List<Device> result = new();

			foreach (Device device in rootDevices)
			{
				subList = rootDevices.Where(x => x.Id == device.Id).Select(x => new Device()
				{
					Id = x.Id,
					Name = x.Name,
					ParentId = x.ParentId,
					Description = x.Description,
					SupplierId = x.SupplierId,
					IsDeleted = x.IsDeleted,
					Childs = getDeviceChilds(ref devices, x.Id)
				}).ToList();



				result.AddRange(subList);
			}

			return new ResultWithMessage(result, string.Empty);
		}

		public ResultWithMessage getDevicesWithRelations(int? parentid, string ? searchQuery, TenantDto authUser)
		{

            IQueryable<Device> query = null;
			IQueryable<Subset> subsetQuery = null;
			List<TreeNodeViewModel> resultDevice = new List<TreeNodeViewModel>();
            List<TreeNodeViewModel> resultSubset = new List<TreeNodeViewModel>();

			query = _db.Devices
				.Where(e => (((e.Parent == null || !e.Parent.IsHidden) && e.ParentId == parentid)
				|| (e.Parent.IsHidden && e.Parent.ParentId == parentid))
				&& !e.IsDeleted && !e.IsHidden);

            if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
            {
                query = query;

            }
            else
            {
                query = query.Where(e => authUser.deviceAccesses.Select(x=>x.DeviceId).ToList().Contains(e.Id) ||
                authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains(e.Parent.Id) ||
                authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Any(z => e.Childs.Select(x=>x.Id).Contains(z))
                );

            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
				query = query.Where(x => x.Id.ToString() == searchQuery || x.Name.ToLower().Contains(searchQuery.ToLower()) || x.Parent.Name.ToLower().Contains(searchQuery.ToLower()) || x.SupplierId.ToLower().Contains(searchQuery.ToLower())
					  || x.Subsets.Any(y => y.Id.ToString() == searchQuery ||
							y.Name.ToLower().Contains(searchQuery.ToLower()) || 
							y.SupplierId.ToLower().Contains(searchQuery.ToLower()) ||
							y.SetId.ToString().Contains(searchQuery.ToLower()) ||
							y.SetName.ToLower().Contains(searchQuery.ToLower())
							)
					  || x.Subsets.Any(z => z.Counters.Any(e => e.Id.ToString() == searchQuery || e.Name.ToLower().Contains(searchQuery.ToLower()) || e.Code.ToLower() == searchQuery.ToLower() || e.SupplierId.ToLower().Contains(searchQuery.ToLower())))
					  || x.Childs.Any(y => y.Name.ToLower().Contains(searchQuery.ToLower()) || y.Parent.Name.ToLower().Contains(searchQuery.ToLower()) || y.SupplierId.ToLower().Contains(searchQuery.ToLower())
                         || y.Subsets.Any(z => z.Name.ToLower().Contains(searchQuery.ToLower()) ||
							z.SupplierId.ToLower().Contains(searchQuery.ToLower()) ||
							z.SetId.ToString().Contains(searchQuery.ToLower()) ||
							z.SetName.ToLower().Contains(searchQuery.ToLower())
							)
						 || y.Subsets.Any(d => d.Counters.Any(b => b.Name.ToLower().Contains(searchQuery.ToLower()) || b.SupplierId.ToLower().Contains(searchQuery.ToLower()) || b.SupplierId.ToLower().Contains(searchQuery.ToLower())))
						 
                         || y.Childs.Any(k => k.Name.ToLower().Contains(searchQuery.ToLower()) || k.Parent.Name.ToLower().Contains(searchQuery.ToLower()) || k.SupplierId.ToLower().Contains(searchQuery.ToLower())
                         || k.Subsets.Any(z => z.Name.ToLower().Contains(searchQuery.ToLower()) ||
							z.SupplierId.ToLower().Contains(searchQuery.ToLower()) ||
                            z.SetId.ToString().Contains(searchQuery.ToLower()) ||
                            z.SetName.ToLower().Contains(searchQuery.ToLower())
                            )
                         || k.Subsets.Any(d => d.Counters.Any(b => b.Name.ToLower().Contains(searchQuery.ToLower()) || b.SupplierId.ToLower().Contains(searchQuery.ToLower())))
						 
						 || k.Childs.Any(q => q.Name.ToLower().Contains(searchQuery.ToLower()) || q.Parent.Name.ToLower().Contains(searchQuery.ToLower()) || q.SupplierId.ToLower().Contains(searchQuery.ToLower())
                         || q.Subsets.Any(z => z.Name.ToLower().Contains(searchQuery.ToLower()) ||
							z.SupplierId.ToLower().Contains(searchQuery.ToLower()) ||
                            z.SetId.ToString().Contains(searchQuery.ToLower()) ||
                            z.SetName.ToLower().Contains(searchQuery.ToLower())
                            )
                         || q.Subsets.Any(d => d.Counters.Any(b => b.Name.ToLower().Contains(searchQuery.ToLower()) || b.SupplierId.ToLower().Contains(searchQuery.ToLower())))))
						  
						 )
                      );
					  
					  					 										  								                    
            }

			
		
                 resultDevice = query.Select(x => new TreeNodeViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = "device",
                    HasChild = x.Childs.Where(c => !c.IsHidden).Count() > 0 || x.Subsets.Count() > 0
								|| x.Childs.Where(c => c.IsHidden).SelectMany(s => s.Subsets).Count() > 0,

                }).ToList();
            
           

			if(parentid!=null && parentid!=0)
			{
				if(!string.IsNullOrEmpty(searchQuery))
				{
                    subsetQuery = _db.Subsets.Where(x => ((!x.Device.IsHidden && x.DeviceId == parentid ) || (x.Device.IsHidden && x.Device.ParentId == parentid))
					&& 
					((x.Name.ToLower().Contains(searchQuery.ToLower()) ||
					x.SupplierId.ToLower().Contains(searchQuery.ToLower())||
					x.SetId.ToString().Contains(searchQuery.ToLower()) ||
					x.SetName.ToLower().Contains(searchQuery.ToLower())) ||
					x.Counters.Any(y=>y.Name.ToLower().Contains(searchQuery.ToLower()) || y.SupplierId.ToLower().Contains(searchQuery.ToLower()))			
					|| x.Device.Parent.Name.ToLower().Contains(searchQuery.ToLower())
					|| x.SupplierId.ToLower().Contains(searchQuery.ToLower())));

                }
                else
				{
                    subsetQuery = _db.Subsets.Where(x => ((!x.Device.IsHidden && x.DeviceId == parentid) || (x.Device.IsHidden && x.Device.ParentId == parentid)));

                }
                resultSubset = subsetQuery.Where(x => x.SetId == null).Select(x => new TreeNodeViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = "subset",
                    HasChild = x.Counters.Count() > 0,

                }).ToList();
				var resultSets = subsetQuery.Where(x => x.SetId != null)
					.GroupBy(x => new {x.SetId, x.SetName })
					.Select(g => new TreeNodeViewModel()
                {
                    Id = g.Key.SetId.Value,
                    Name = g.Key.SetName,
                    Type = "set",
                    HasChild = g.Count() > 0,
                }).ToList();

                return new ResultWithMessage(resultDevice.Union(resultSubset).Union(resultSets), "");

            }

            return new ResultWithMessage(resultDevice, "");


        }

    }
}