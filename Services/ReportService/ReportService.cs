using static Tenor.Services.AuthServives.ViewModels.AuthModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
using Tenor.Dtos;
using static Tenor.Services.ReportService.ViewModels.ReportModels;
using Tenor.Data;
using Tenor.Services.AuthServives;
using static Tenor.Helper.Constant;
using Tenor.Models;
using Microsoft.OpenApi.Extensions;
using Tenor.Services.SharedService;
using System.Transactions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Security.Cryptography;
using Tenor.Helper;
using Tenor.Services.CountersService.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Collections;

namespace Tenor.Services.ReportService
{
	public interface IReportService
	{
		Task<ResultWithMessage> Add(CreateReport input, TenantDto authUser);
		Task<ResultWithMessage> HardDelete(int id, TenantDto authUser);
		Task<ResultWithMessage> SoftDelete(int id, TenantDto authUser);
		Task<ResultWithMessage> GetById(int id);
		Task<ResultWithMessage> GetByFilter(ReportListFilter input, TenantDto authUser);
		Task<ResultWithMessage> GetReportTreeUserNames(ReportTreeFilter input, TenantDto authUser);
		Task<ResultWithMessage> GetReportTreeDevicesByUserName(ReportTreeFilter input, TenantDto authUser);
		Task<ResultWithMessage> GetReportTreeByUserNameDevice(ReportTreeFilter input, TenantDto authUser);
		Task<ResultWithMessage> GetReportTreeByUserName(ReportTreeFilter input, TenantDto authUser);
		Task<ResultWithMessage> getDimensionLevels(List<ReportMeasure> reportMeasures);
	}
	public class ReportService : IReportService
	{
		private readonly TenorDbContext _db;
		private readonly IJwtService _jwtService;
		private readonly ISharedService _sharedService;
		private readonly IMapper _mapper;

		public ReportService(TenorDbContext db, IJwtService jwtService, ISharedService sharedService, IMapper mapper)
		{
			_db = db;
			_jwtService = jwtService;
			_sharedService = sharedService;
			_mapper = mapper;
		}

		public async Task<ResultWithMessage> Add(CreateReport input, TenantDto authUser)
		{
			//---------------------General Validations---------------------------------
			string accessResult = _jwtService.checkUserTenantPermission(authUser, input.DeviceId);
			if (accessResult == enAccessType.denied.GetDisplayName())
			{
				return new ResultWithMessage(null, "Access Denied");
			}
			if (_sharedService.IsExist(0, input.DeviceId, null, null, input.Name))
			{
				return new ResultWithMessage(null, "This Report name alraedy exsit on the same device");

			}
			foreach (var s in input.Measures)
			{
				if (Convert.ToBoolean(_sharedService.CheckValidFormat(s.Operation).Data) == false)
				{
					return new ResultWithMessage(null, _sharedService.CheckValidFormat(s.Operation).Message);

				}

			}

			using (TransactionScope transaction = new TransactionScope())
			{
				try
				{
					//-----------------Create Report-----------------------
					Report report = new Report(input);
					report.ChildId = (report.ChildId == 0 || report.ChildId == null) ? null : report.ChildId;
					report.Measures = new List<ReportMeasure>();
					//-----------------Create Report Levels----------------
					report.Levels = input.Levels.Select(x => new ReportLevel()
					{
						Id = x.Id,
						DisplayOrder = x.DisplayOrder,
						SortDirection = x.SortDirection,
						ReportId = report.Id,
						LevelId = x.LevelId

					}).ToList();
					//-----------------Create Report Filters--------------
					report.FilterContainers = input.ContainerOfFilters.Select(x => new ReportFilterContainer()
					{
						Id = x.Id,
						ReportId = report.Id,
						ReportFilters = x.ReportFilters.Select(y => new ReportFilter()
						{
							Id = y.Id,
							LogicalOperator = y.LogicalOperator,
							Value = _sharedService.ConvertListToString(y.Value),
							FilterContainerId = x.Id,
							LevelId = y.LevelId
						}).ToList()

					}).ToList();

					//-----------------Create Report Measures-------------
					foreach (ReportMeasureDto m in input.Measures)
					{
						if (_sharedService.IsExist(0, null, null, m.DisplayName, null))
						{
							return new ResultWithMessage(null, "This Measure name :  " + m.DisplayName + "  alraedy exsit");

						}
						if (Convert.ToBoolean(_sharedService.CheckValidFormat(m.Operation).Data) == false)
						{
							return new ResultWithMessage(null, _sharedService.CheckValidFormat(m.Operation).Message);
						}

						ReportMeasure measure = _mapper.Map<ReportMeasure>(m);
						measure.ReportId = report.Id;
						report.Measures.Add(measure);

					}
					_db.Reports.Add(report);
					_db.SaveChanges();
					//------------------------Extra Fields--------------------------
					if (input.ReportFields != null)
					{
						bool isCorrectSave = _sharedService.AddExtraFields(null, (int)report.Id, input.ReportFields);
						if (!isCorrectSave)
						{
							return new ResultWithMessage(null, "Mandatory Field error");
						}
					}
					transaction.Complete();
					return new ResultWithMessage(report, null);

				}
				catch (Exception ex)
				{
					return new ResultWithMessage(null, ex.Message);

				}
			}


		}
		public async Task<ResultWithMessage> HardDelete(int id, TenantDto authUser)
		{
			var report = _db.Reports.Include(x => x.Measures).FirstOrDefault(x => x.Id == id && !x.IsDeleted);

			if (report == null)
			{
				return new ResultWithMessage(null, "Cannot find report");
			}
			string accessResult = _jwtService.checkUserTenantPermission(authUser, report.DeviceId);
			if (accessResult == enAccessType.denied.GetDisplayName())
			{
				return new ResultWithMessage(null, "Access Denied");
			}

			if (accessResult == enAccessType.allOnlyMe.GetDisplayName() && report.CreatedBy != authUser.userName)
			{
				return new ResultWithMessage(null, "Access Denied");

			}

			//Delete measures operation 
			foreach (var s in report.Measures)
			{
				_sharedService.deleteOperation(s.OperationId);

			}
			_db.Remove(report);
			_db.SaveChanges();
			return new ResultWithMessage(true, "");

		}
		public async Task<ResultWithMessage> SoftDelete(int id, TenantDto authUser)
		{
			var report = _db.Reports.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
			if (report == null)
			{
				return new ResultWithMessage(null, "Cannot find report");
			}
			string accessResult = _jwtService.checkUserTenantPermission(authUser, report.DeviceId);
			if (accessResult == enAccessType.denied.GetDisplayName())
			{
				return new ResultWithMessage(null, "Access Denied");
			}

			if (accessResult == enAccessType.allOnlyMe.GetDisplayName() && report.CreatedBy != authUser.userName)
			{
				return new ResultWithMessage(null, "Access Denied");

			}

			report.IsDeleted = true;
			_db.Update(report);
			_db.SaveChanges();

			return new ResultWithMessage(true, "");
		}
		public async Task<ResultWithMessage> GetById(int id)
		{
			ReportViewModel result = new ReportViewModel();
			var report = _db.Reports.Include(x => x.Device).Include(x => x.Measures).ThenInclude(x => x.Havings).ThenInclude(x => x.Operator)
				.Include(x => x.Levels).ThenInclude(x => x.Level).Include(x => x.FilterContainers).ThenInclude(x => x.ReportFilters)
				.Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField)
				.FirstOrDefault(x => x.Id == id);

			if (report is null)
			{
				return new ResultWithMessage(null, "Cannot find report");

			}

			//--------------------------Build Mapping---------------------------------
			result.Id = report.Id;
			result.Name = report.Name;
			result.DeviceId = report.DeviceId;
			result.DeviceName = report.Device.Name;
			result.IsPublic = report.IsPublic;
			result.CreatedBy = report.CreatedBy;
			result.CreatedDate = report.CreatedDate;
			result.ChildId = report.ChildId;
			result.Levels = report.Levels.Select(x => new ReportLevelViewModel()
			{
				Id = x.Id,
				DisplayOrder = x.DisplayOrder,
				SortDirection = x.SortDirection,
				SortDirectionName = x.SortDirection.GetDisplayName(),
				LevelId = x.LevelId,
				LevelName = x.Level.Name,
				IsFilter = x.Level.IsFilter,
				IsLevel = x.Level.IsLevel
			}).ToList();
			result.ReportFields = report.ReportFieldValues.Select(x => new ReportFieldValueViewModel()
			{
				Id = x.Id,
				FieldId = x.ReportFieldId,
				Type = x.ReportField.ExtraField.Type.GetDisplayName(),
				FieldName = x.ReportField.ExtraField.Name,
				Value = _sharedService.ConvertContentType(x.ReportField.ExtraField.Type.GetDisplayName(), x.FieldValue)

			}).ToList();
			result.ContainerOfFilters = report.FilterContainers.Select(x => new ContainerOfFilter()
			{
				Id = x.Id,
				LogicalOperator = x.LogicalOperator,
				LogicalOperatorName = x.LogicalOperator.GetDisplayName(),
				ReportFilters = x.ReportFilters.Select(y => new ReportFilterDto()
				{
					Id = y.Id,
					LogicalOperator = y.LogicalOperator,
					LogicalOperatorName = y.LogicalOperator.GetDisplayName(),
					Value = y.Value != null ? y.Value.Split(',').ToArray() : null,
					LevelId = y.LevelId,
					IsMandatory = y.IsMandatory
				}).ToList()
			}).ToList();
			result.Measures = GetReportMeasureById(report.Measures.ToList());


			return new ResultWithMessage(result, null);


		}
		public async Task<ResultWithMessage> GetByFilter(ReportListFilter input, TenantDto authUser)
		{
			IQueryable<Report> query = null;
			//------------------------------Data source--------------------------------------------
			if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
			{
				query = _db.Reports.Where(x => !x.IsDeleted).Include(x => x.Device).Include(x => x.ReportFieldValues)
					.ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField).AsQueryable();
			}
			else
			{
				query = _db.Reports.Where(x => !x.IsDeleted && (x.CreatedBy == authUser.userName || x.IsPublic || authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains((int)x.DeviceId)))
			   .Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField)
			   .ThenInclude(x => x.ExtraField).AsQueryable();
			}
			//------------------------------Data filter-------------------------------------------
			if (!string.IsNullOrEmpty(input.SearchQuery))
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.SearchQuery.ToLower())
							  || x.DeviceId.ToString().Equals(input.SearchQuery)
							  || x.Id.ToString().Equals(input.SearchQuery)
							  || x.CreatedBy.ToLower().Contains(input.SearchQuery)
							  || x.Device.Name.ToLower().Contains(input.SearchQuery)
							  );
			}

			if (input.deviceId != null && input.deviceId != 0)
			{
				query = query.Where(x => x.DeviceId == input.deviceId);

			}

			if (input.ExtraFields != null)
			{
				foreach (var s in input.ExtraFields)
				{
					string strValue = string.Join(',', s.Value).ToString();
					strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

					if (!string.IsNullOrEmpty(strValue))
					{
						query = query.Where(x => x.ReportFieldValues.Any(y => y.ReportField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
					}

				}
			}

			//mapping to DTO querable

			var queryViewModel = query.Select(x => new ReportDto()
			{
				Id = x.Id,
				Name = x.Name,
				DeviceId = x.DeviceId,
				DeviceName = x.Device.Name,
				IsPublic = x.IsPublic,
				CreatedBy = x.CreatedBy,
				CreatedDate = x.CreatedDate

			});

			return sortAndPagination(input, queryViewModel);


		}
		public async Task<ResultWithMessage> GetReportTreeUserNames(ReportTreeFilter input, TenantDto authUser)
		{
			IQueryable<Report> query = null;
			//------------------------------Data source--------------------------------------------
			if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
			{
				query = _db.Reports.Where(x => !x.IsDeleted).Include(x => x.Device).Include(x => x.ReportFieldValues)
					.ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField).AsQueryable();
			}
			else
			{
				query = _db.Reports.Where(x => !x.IsDeleted && (x.CreatedBy == authUser.userName || x.IsPublic || authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains((int)x.DeviceId)))
			   .Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField)
			   .ThenInclude(x => x.ExtraField).AsQueryable();
			}

			//------------------------------Data filter-------------------------------------------
			if (!string.IsNullOrEmpty(input.SearchQuery))
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.SearchQuery.ToLower())
							  || x.DeviceId.ToString().Equals(input.SearchQuery)
							  || x.Id.ToString().Equals(input.SearchQuery)
							  || x.CreatedBy.ToLower().Contains(input.SearchQuery)
							  || x.Device.Name.ToLower().Contains(input.SearchQuery)
							  );
			}

			if (input.deviceId != null && input.deviceId != 0)
			{
				query = query.Where(x => x.DeviceId == input.deviceId);

			}

			if (input.ExtraFields != null)
			{
				foreach (var s in input.ExtraFields)
				{
					string strValue = string.Join(',', s.Value).ToString();
					strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

					if (!string.IsNullOrEmpty(strValue))
					{
						query = query.Where(x => x.ReportFieldValues.Any(y => y.ReportField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
					}

				}
			}

			var res = query.GroupBy(
			 p => p.CreatedBy,
			 p => p.DeviceId,
			 (key, g) => new { CreatedBy = key, Devices = g.ToList() });

			var result = res.Select(x => new TreeReportViewModel()
			{
				Name = x.CreatedBy,
				Type = "userName",
				HasChild = x.Devices.Count() > 0

			}).ToList();


			return new ResultWithMessage(result, null);

		}
		public async Task<ResultWithMessage> GetReportTreeDevicesByUserName(ReportTreeFilter input, TenantDto authUser)
		{
			IQueryable<Report> query = null;
			//------------------------------Data source--------------------------------------------
			if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
			{
				query = _db.Reports.Where(x => !x.IsDeleted && x.CreatedBy == input.userName).Include(x => x.Device).Include(x => x.ReportFieldValues)
					.ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField).AsQueryable();
			}
			else
			{
				query = _db.Reports.Where(x => !x.IsDeleted && x.CreatedBy == input.userName && (x.IsPublic || authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains((int)x.DeviceId)))
			   .Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField)
			   .ThenInclude(x => x.ExtraField).AsQueryable();
			}
			//------------------------------Data filter-------------------------------------------
			if (!string.IsNullOrEmpty(input.SearchQuery))
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.SearchQuery.ToLower())
							  || x.DeviceId.ToString().Equals(input.SearchQuery)
							  || x.Id.ToString().Equals(input.SearchQuery)
							  || x.CreatedBy.ToLower().Contains(input.SearchQuery)
							  || x.Device.Name.ToLower().Contains(input.SearchQuery)
							  );
			}

			if (input.deviceId != null && input.deviceId != 0)
			{
				query = query.Where(x => x.DeviceId == input.deviceId);

			}

			if (input.ExtraFields != null)
			{
				foreach (var s in input.ExtraFields)
				{
					string strValue = string.Join(',', s.Value).ToString();
					strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

					if (!string.IsNullOrEmpty(strValue))
					{
						query = query.Where(x => x.ReportFieldValues.Any(y => y.ReportField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
					}

				}
			}


			var res = query.GroupBy(
			   p => p.Device,
			   p => p.Name,
			   (key, g) => new { Device = key, Name = g.ToList() });

			var result = res.Select(x => new TreeReportViewModel()
			{
				Id = x.Device.Id,
				Name = x.Device.Name,
				Type = "device",
				HasChild = x.Name.Count() > 0

			}).ToList();


			return new ResultWithMessage(result, null);

		}
		public async Task<ResultWithMessage> GetReportTreeByUserNameDevice(ReportTreeFilter input, TenantDto authUser)
		{
			IQueryable<Report> query = null;
			//------------------------------Data source--------------------------------------------
			query = _db.Reports.Where(x => !x.IsDeleted && x.CreatedBy == input.userName && x.DeviceId == input.deviceId).Include(x => x.Device).Include(x => x.ReportFieldValues)
					.ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField).AsQueryable();

			//------------------------------Data filter-------------------------------------------
			if (!string.IsNullOrEmpty(input.SearchQuery))
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.SearchQuery.ToLower())
							  || x.DeviceId.ToString().Equals(input.SearchQuery)
							  || x.Id.ToString().Equals(input.SearchQuery)
							  || x.CreatedBy.ToLower().Contains(input.SearchQuery)
							  || x.Device.Name.ToLower().Contains(input.SearchQuery)
							  );
			}


			if (input.ExtraFields != null)
			{
				foreach (var s in input.ExtraFields)
				{
					string strValue = string.Join(',', s.Value).ToString();
					strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

					if (!string.IsNullOrEmpty(strValue))
					{
						query = query.Where(x => x.ReportFieldValues.Any(y => y.ReportField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
					}

				}
			}


			var result = query.Select(x => new TreeReportViewModel()
			{
				Id = x.Id,
				Name = x.Name,
				Type = "report",

			}).ToList();


			return new ResultWithMessage(result, null);
		}
		public async Task<ResultWithMessage> GetReportTreeByUserName(ReportTreeFilter input, TenantDto authUser)
		{
			IQueryable<Report> query = null;
			//------------------------------Data source--------------------------------------------
			query = _db.Reports.Where(x => !x.IsDeleted && x.CreatedBy == input.userName).Include(x => x.Device).Include(x => x.ReportFieldValues)
					.ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField).AsQueryable();

			//------------------------------Data filter-------------------------------------------
			if (!string.IsNullOrEmpty(input.SearchQuery))
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.SearchQuery.ToLower())
							  || x.DeviceId.ToString().Equals(input.SearchQuery)
							  || x.Id.ToString().Equals(input.SearchQuery)
							  || x.CreatedBy.ToLower().Contains(input.SearchQuery)
							  || x.Device.Name.ToLower().Contains(input.SearchQuery)
							  );
			}


			if (input.ExtraFields != null)
			{
				foreach (var s in input.ExtraFields)
				{
					string strValue = string.Join(',', s.Value).ToString();
					strValue = strValue.Replace("[", "").Replace("]", "").Replace(@"\t|\n|\r|\s+", "").Replace("\"", "");

					if (!string.IsNullOrEmpty(strValue))
					{
						query = query.Where(x => x.ReportFieldValues.Any(y => y.ReportField.ExtraField.Name == s.Key.ToString() && strValue.Contains(y.FieldValue)));
					}

				}
			}

			var result = query.Select(x => new TreeReportViewModel()
			{
				Id = x.Id,
				Name = x.Name,
				Type = "report",

			}).ToList();


			return new ResultWithMessage(result, null);


		}
		private List<MeasureViewModel> GetReportMeasureById(List<ReportMeasure> reportMeasures)
		{

			List<MeasureViewModel> result = new List<MeasureViewModel>();

			foreach (var s in reportMeasures)
			{
				var measure = _db.ReportMeasures.Include(x => x.Report).Include(x => x.Operation)
				 .FirstOrDefault(x => x.Id == s.Id);

				if (measure == null)
				{
					return null;

				}
				_sharedService.GetSelfRelation(measure.OperationId);
				var measureMap = _mapper.Map<MeasureViewModel>(measure);
				result.Add(measureMap);

			}

			return result;
		}
		private ResultWithMessage sortAndPagination(ReportListFilter FilterModel, IQueryable<ReportDto> queryViewModel)
		{
			if (!string.IsNullOrEmpty(FilterModel.SortActive))
			{

				var sortProperty = typeof(ReportDto).GetProperty(char.ToUpper(FilterModel.SortActive[0]) + FilterModel.SortActive.Substring(1));
				if (sortProperty != null && FilterModel.SortDirection == "asc")
					queryViewModel = queryViewModel.OrderBy2(FilterModel.SortActive);

				else if (sortProperty != null && FilterModel.SortDirection == "desc")
					queryViewModel = queryViewModel.OrderByDescending2(FilterModel.SortActive);

				int Count = queryViewModel.Count();

				var result = queryViewModel.Skip((FilterModel.PageIndex) * FilterModel.PageSize)
				.Take(FilterModel.PageSize).ToList();

				return new ResultWithMessage(new DataWithSize(Count, result), "");
			}

			else
			{
				int Count = queryViewModel.Count();
				var result = queryViewModel.Skip((FilterModel.PageIndex) * FilterModel.PageSize)
				.Take(FilterModel.PageSize).ToList();

				return new ResultWithMessage(new DataWithSize(Count, result), "");
			}

		}






		///// SALEEM
		///////////////////////// private methods
		// 1- get operation childs
		private List<Operation> getMeasureOperations(int operationId)
		{
			List<Operation> operations = _db.Operations.ToList();
			List<Operation> rootOperations = operations.Where(e => e.Id == operationId).ToList();
			List<Operation> subList = new();
			List<Operation> result = new();

			foreach (Operation operation in rootOperations)
			{
				subList = rootOperations.Where(x => x.Id == operation.Id).Select(x => new Operation()
				{
					Id = x.Id,
					Order = x.Order,
					Childs = getOperationChilds(ref operations, x.Id)
				}).ToList();

				result.AddRange(subList);
			}

			return result;
		}
		private List<Operation> getOperationChilds(ref List<Operation> operations, int parentId)
		{
			List<Operation> innerOperations = operations;

			return operations.Where(e => e.ParentId == parentId).Select(e => new Operation()
			{
				Id = e.Id,
				Order = e.Order,
				CounterId = e.CounterId,
				Childs = innerOperations.Where(x => x.ParentId == e.Id).Count() > 0 ? getOperationChilds(ref innerOperations, e.Id) : null
			}).ToList();

		}

		// 2- get counters devices
		private List<int> getMeasureCounterIds(Operation operation, List<Operation> operations, List<int> prevCounterIds)
		{
			//https://stackoverflow.com/questions/73485400/c-sharp-recursively-loop-over-object-and-return-all-children
			List<int> counterIds = prevCounterIds;
			int? counterId = 0;

			if (operation.Childs != null)
			{
				foreach (Operation child in operation.Childs)
				{
					counterId = child.CounterId;

					if (!string.IsNullOrEmpty(counterId.ToString()))
						counterIds.Add(int.Parse(counterId.ToString()));

					getMeasureCounterIds(child, operations, counterIds);
				}
			}

			return counterIds;
		}
		private Device getCounterDevice(int counterId)
		{
			Device device = _db.Counters.Include(e => e.Subset)
							 .ThenInclude(e => e.Device)
							 .Where(e => e.Id == counterId)
							 .Select(e => new Device()
							 {
								 Id = e.Subset.Device.Id,
								 Name = e.Subset.Device.Name,
								 Description = e.Subset.Device.Description
							 }).First();
			return device;
		}
		private List<int> getMeasureDeviceIds(List<Operation> operations)
		{
			List<int> counterIds = getMeasureCounterIds(operations.ElementAt(0), operations, new List<int>());
			List<Device> devices = new();
			foreach (int counterId in counterIds)
				devices.Add(getCounterDevice(counterId));

			List<int> x = devices.Select(x => x.Id).Distinct().ToList();

			return x;
		}

		public async Task<ResultWithMessage> getDimensionLevels(List<ReportMeasure> reportMeasures)
		{
			// 1- get operation childs
			List<List<Operation>> measureOperations = [];
			foreach (ReportMeasure reportMeasure in reportMeasures)
				measureOperations.Add(getMeasureOperations(reportMeasure.OperationId));


			// 2- get counters devices
			List<int> deviceIds = [];
			foreach (List<Operation> operations in measureOperations)
				deviceIds.AddRange(getMeasureDeviceIds(operations));

			if (deviceIds.Count == 0)
				return new ResultWithMessage(null, "Empty device Ids");

			// 3- return shared levels between devices 
			var result = _db.Dimensions
					.Where(x => deviceIds.Contains(x.DeviceId.Value))
					.Select(d => d.DimensionLevels.Select(dl => new
					{
						DimensionName = d.Name,
						dl.LevelId,
						LevelName = dl.Level.Name,
						LevelType = dl.Level.DataType,
						dl.Level.IsFilter,
						dl.Level.IsLevel
					})).ToList()
				.SelectMany(x => x)
				.GroupBy(z => z)
				.Select(b => new { data = b.Key, Count = b.Count() })
				.Where(f => f.Count == deviceIds.Count)
				.GroupBy(f => f.data.DimensionName)
				.Select(g => new TreeNodeViewModel
				{
					Id = 0,
					Name = g.Key,
					HasChild = true,
					Type = "dimension",
					Childs = g.Select(z => new TreeNodeViewModel
					{
						Id = z.data.LevelId,
						Name = z.data.LevelName,
						HasChild = false,
						IsFilter = z.data.IsFilter,
						IsLevel = z.data.IsLevel,
						Type = z.data.LevelType
					}).ToList()
				});

			return new ResultWithMessage(result, "");
		}
	}
}
