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

namespace Tenor.Services.ReportService
{
	public interface IReportService
	{
		Task<ResultWithMessage> Add(CreateReport input, TenantDto authUser);
		Task<ResultWithMessage> getDimensionLevels(List<ReportMeasure> measures);
		Task<ResultWithMessage> HardDelete(int id, TenantDto authUser);
		Task<ResultWithMessage> SoftDelete(int id, TenantDto authUser);
		Task<ResultWithMessage> GetById(int id);
        Task<ResultWithMessage> GetByFilter(ReportListFilter input , TenantDto authUser);
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
                    report.ChildId= (report.ChildId==0 || report.ChildId==null) ? null:report.ChildId;
                    report.Measures = new List<ReportMeasure>();                
                    //-----------------Create Report Levels----------------
                    report.Levels = input.Levels.Select(x=> new ReportLevel()
                    {
                        Id=x.Id,
                        DisplayOrder=x.DisplayOrder,
                        SortDirection=x.SortDirection,
                        ReportId=report.Id,
                        LevelId=x.LevelId

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
							DimensionLevelId = y.DimensionLevelId
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
		public async Task<ResultWithMessage> getDimensionLevels(List<ReportMeasure> measures)
		{
			return new ResultWithMessage(null, string.Empty);
		}
		public async Task<ResultWithMessage> HardDelete(int id, TenantDto authUser)
		{
			var report = _db.Reports.Include(x=>x.Measures).FirstOrDefault(x=>x.Id==id && !x.IsDeleted);

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
			foreach(var s in report.Measures)
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
			var report = _db.Reports.Include(x=>x.Device).Include(x => x.Measures).ThenInclude(x=> x.Havings).ThenInclude(x=>x.Operator)
				.Include(x => x.Levels).ThenInclude(x=>x.Level).Include(x => x.FilterContainers).ThenInclude(x=>x.ReportFilters)
                .Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField)
				.FirstOrDefault(x => x.Id == id);

			if(report is null)
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
				Id=x.Id,
				DisplayOrder=x.DisplayOrder,
				SortDirection=x.SortDirection,
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
				Value = _sharedService.ConvertContentType( x.ReportField.ExtraField.Type.GetDisplayName() , x.FieldValue)

            }).ToList();
			result.ContainerOfFilters = report.FilterContainers.Select(x => new ContainerOfFilter()
			{
				Id = x.Id,
				LogicalOperator = x.LogicalOperator,
				LogicalOperatorName = x.LogicalOperator.GetDisplayName(),
				ReportFilters = x.ReportFilters.Select(y=> new ReportFilterDto()
				{
					Id = y.Id,
					LogicalOperator = y.LogicalOperator,
					LogicalOperatorName = y.LogicalOperator.GetDisplayName(),
					Value = y.Value!= null? y.Value.Split(',').ToArray() : null,
					DimensionLevelId = y.DimensionLevelId,
					IsMandatory = y.IsMandatory
				}).ToList()
			}).ToList();
			result.Measures = GetReportMeasureById(report.Measures.ToList());


            return new ResultWithMessage(result,null);


        }
		public async Task<ResultWithMessage> GetByFilter(ReportListFilter input , TenantDto authUser)
		{
            IQueryable<Report> query = null;
            //------------------------------Data source--------------------------------------------
            if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
            {
                query = _db.Reports.Where(x=> !x.IsDeleted).Include(x=>x.Device).Include(x => x.ReportFieldValues)
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
        private List<MeasureViewModel> GetReportMeasureById(List<ReportMeasure> reportMeasures)
        {

			List<MeasureViewModel> result = new List<MeasureViewModel>();

			foreach(var s in reportMeasures)
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

    }
}
