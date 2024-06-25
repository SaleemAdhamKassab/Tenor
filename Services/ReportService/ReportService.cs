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
using static Tenor.Services.SharedService.ViewModels.SharedModels;
using Tenor.Services.DataServices;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using System.Composition;
using System.Threading.Tasks;

namespace Tenor.Services.ReportService
{
    public interface IReportService
    {
        Task<ResultWithMessage> Add(CreateReport input, TenantDto authUser);
        Task<ResultWithMessage> HardDelete(int id, TenantDto authUser);
        Task<ResultWithMessage> SoftDelete(int id, TenantDto authUser);
        Task<ResultWithMessage> GetById(int id, TenantDto authUser);
        Task<ResultWithMessage> GetByFilter(ReportListFilter input, TenantDto authUser);
        Task<ResultWithMessage> GetReportTreeUserNames(ReportTreeFilter input, TenantDto authUser);
        Task<ResultWithMessage> GetReportTreeDevicesByUserName(ReportTreeFilter input, TenantDto authUser);
        Task<ResultWithMessage> GetReportTreeByUserNameDevice(ReportTreeFilter input, TenantDto authUser);
        Task<ResultWithMessage> GetReportTreeByUserName(ReportTreeFilter input, TenantDto authUser);
        Task<ResultWithMessage> getDimensionLevels(List<ReportMeasureDto> reportMeasures);
        Task<ResultWithMessage> getDimensionFilters(List<ReportMeasureDto> reportMeasures);
        ResultWithMessage getFilterOptions(int levelId, string? searchQuery, int pageIndex, int pageSize);
        Task<ResultWithMessage> GetExtraFields(int? deviceId);
        ResultWithMessage ValidateReport(int? deviceId, string reportName);
        ResultWithMessage getReportDataByCreateReport(CreateReport report, int pageSize, int pageIndex);
        Task<ResultWithMessage> getReportDataById(int id, int pageSize, int pageIndex, List<ContainerOfFilter> filters, TenantDto authUser);
        Task<ResultWithMessage> Update(int id, CreateReport input, TenantDto authUser);
        Task<ResultWithMessage> GetReportRehearsal(int reportId, TenantDto authUser);
        public Task<byte[]> exportReportDataById(int id, List<ContainerOfFilter> filters, TenantDto authUser);

    }
    public class ReportService : IReportService
    {
        private readonly TenorDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ISharedService _sharedService;
        private readonly IMapper _mapper;
        private readonly IQueryBuilderService _queryBuilder;
        private readonly IDataProviderService _dataProvider;

        public ReportService(TenorDbContext db, IJwtService jwtService, ISharedService sharedService, IMapper mapper, IQueryBuilderService queryBuilder, IDataProviderService dataProvider)
        {
            _db = db;
            _jwtService = jwtService;
            _sharedService = sharedService;
            _mapper = mapper;
            _queryBuilder = queryBuilder;
            _dataProvider = dataProvider;
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
                    report.Levels = input.Levels.Select((x, i) => new ReportLevel()
                    {
                        Id = 0,
                        DisplayOrder = i,
                        SortDirection = x.SortDirection,
                        ReportId = report.Id,
                        LevelId = x.LevelId

                    }).ToList();
                    //-----------------Create Report Filters--------------
                    report.FilterContainers = input.ContainerOfFilters.Select(x => new ReportFilterContainer()
                    {
                        Id = 0,
                        ReportId = report.Id,
                        ReportFilters = x.ReportFilters.Select(y => new ReportFilter()
                        {
                            Id = 0,
                            LogicalOperator = y.LogicalOperator,
                            Value = _sharedService.ConvertListToString(y.Value),
                            FilterContainerId = x.Id,
                            LevelId = y.LevelId,
                            IsMandatory = y.IsMandatory,
                            IsVariable = y.IsVariable
                        }).ToList()

                    }).ToList();
                    //-----------------Create Report Measures-------------
                    foreach (ReportMeasureDto m in input.Measures)
                    {
                        //if (_sharedService.IsExist(0, null, null, m.DisplayName, null))
                        //{
                        //	return new ResultWithMessage(null, "This Measure name :  " + m.DisplayName + "  alraedy exsit");

                        //}
                        if (Convert.ToBoolean(_sharedService.CheckValidFormat(m.Operation).Data) == false)
                        {
                            return new ResultWithMessage(null, _sharedService.CheckValidFormat(m.Operation).Message);
                        }
                        m.Id = 0;
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
        public async Task<ResultWithMessage> GetById(int id, TenantDto authUser)
        {
            
            Report report = null;
            if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
            {
                report = _db.Reports.Include(x => x.Device).Include(x => x.Measures).ThenInclude(x => x.Havings).ThenInclude(x => x.Operator)
                .Include(x => x.Levels).ThenInclude(x => x.Level).Include(x => x.FilterContainers).ThenInclude(x => x.ReportFilters).ThenInclude(x => x.Level)
                .Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField)
                .FirstOrDefault(x => x.Id == id);
            }
            else
            {
                report = _db.Reports.Where(x => !x.IsDeleted && x.CreatedBy == authUser.userName && (x.IsPublic || authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains((int)x.DeviceId)))
                .Include(x => x.Device).Include(x => x.Measures).ThenInclude(x => x.Havings).ThenInclude(x => x.Operator)
               .Include(x => x.Levels).ThenInclude(x => x.Level).Include(x => x.FilterContainers).ThenInclude(x => x.ReportFilters).ThenInclude(x => x.Level)
               .Include(x => x.ReportFieldValues).ThenInclude(x => x.ReportField).ThenInclude(x => x.ExtraField)
               .FirstOrDefault(x => x.Id == id);

               
            }

            if (report is null)
            {
                return new ResultWithMessage(null, "Cannot find report");

            }
            var result = ConvertToViewModel(report);
            result.CanEdit = checkEditValidation(authUser, report.DeviceId, report, _jwtService);


            //--------------------------Build Mapping---------------------------------

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

                var xx = _db.Reports.ToList();
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

            if (input.DeviceId != null && input.DeviceId != 0)
            {
                query = query.Where(x => x.DeviceId == input.DeviceId);

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
            var queryViewModel =query.Select(  x =>  new ReportDto()
            {
                Id = x.Id,
                Name = x.Name,
                DeviceId = x.DeviceId,
                DeviceName = x.Device.Name,
                IsPublic = x.IsPublic,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
               // CanEdit = checkEditValidation(authUser,x.DeviceId,x,_jwtService)
            });

           

            return sortAndPagination(input, queryViewModel ,authUser);


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
                CanEdit = checkEditValidation(authUser, x.DeviceId, x, _jwtService),
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
                CanEdit = checkEditValidation(authUser, x.DeviceId, x, _jwtService),
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
        private ResultWithMessage sortAndPagination(ReportListFilter FilterModel, IQueryable<ReportDto> queryViewModel , TenantDto authUser)
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
        public async Task<ResultWithMessage> getDimensionLevels(List<ReportMeasureDto> reportMeasures)
        {
            List<int> deviceIds = [];
            foreach (ReportMeasureDto measure in reportMeasures)
            {
                deviceIds.AddRange(_sharedService.getOperationSubqueryModel(measure.Operation).Select(x => x.DeviceId));
            }
            deviceIds = deviceIds.Where(x => x != 0).Distinct().ToList();
            // 3- return shared levels between devices 
            var result = _db.Dimensions
                    .Where(x => deviceIds.Contains(x.DeviceId.Value))
                    .Select(d => d.DimensionLevels.Where(dl => dl.Level.IsLevel).Select(dl => new
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
        public async Task<ResultWithMessage> getDimensionFilters(List<ReportMeasureDto> reportMeasures)
        {
            List<int> deviceIds = [];
            foreach (ReportMeasureDto measure in reportMeasures)
            {
                deviceIds.AddRange(_sharedService.getOperationSubqueryModel(measure.Operation).Select(x => x.DeviceId));
            }
            deviceIds = deviceIds.Where(x => x != 0).Distinct().ToList();
            // 3- return shared levels between devices 
            var result = _db.Dimensions
                    .Where(x => deviceIds.Contains(x.DeviceId.Value))
                    .Select(d => d.DimensionLevels.Where(dl => dl.Level.IsFilter).Select(dl => new
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
        public ResultWithMessage getFilterOptions(int levelId, string? searchQuery, int pageIndex, int pageSize)
        {
            var query = _queryBuilder.getFilterOptionsQuery(levelId, searchQuery, pageIndex, pageSize);
            var data = _dataProvider.fetchFilterOptionsByQuery(query);
            //var data = new List<string> { "A", "B", "C" };
            return new ResultWithMessage(data, "");
        }
        public async Task<ResultWithMessage> GetExtraFields(int? deviceId)
        {

            var dbResult = deviceId != null && deviceId != 0 ?
                _db.ReportFields.Where(x => x.IsActive && x.ExtraField.DeviceId == deviceId) :
                _db.ReportFields.Where(x => x.IsActive);
            var result = dbResult.Select(x => new KpiExtraField
            {
                Id = x.Id,
                Type = x.ExtraField.Type.GetDisplayName(),
                Name = x.ExtraField.Name,
                IsMandatory = x.ExtraField.IsMandatory,
                Content = ConvertContentType(x.ExtraField.Type.GetDisplayName(), x.ExtraField.Content),
                Url = x.ExtraField.Url,
                DeviceId = x.ExtraField.DeviceId,
                DeviceName = x.ExtraField.Device.Name
            }).ToList();
            return new ResultWithMessage(result, null);
        }
        private static dynamic ConvertContentType(string contenttype, string content)
        {
            if ((contenttype == "List" && !string.IsNullOrEmpty(content) ? !content.Contains(",") : true) && contenttype != "MultiSelectList")
            {
                return content;
            }

            return !string.IsNullOrEmpty(content) ? content.Split(',').ToList() : null;

        }
        public ResultWithMessage ValidateReport(int? deviceId, string reportName)
        {
            if (_sharedService.IsExist(0, deviceId, null, null, reportName))
            {
                return new ResultWithMessage(false, "This Report name is already exsist on this device.");
            }
            return new ResultWithMessage(true, null);
        }
        public ResultWithMessage getReportDataByCreateReport(CreateReport report, int pageSize, int pageIndex)
        {
            var sql = _queryBuilder.getReportQueryByCreateReport(report, pageSize, pageIndex);
            return new ResultWithMessage(sql, "");
        }
        public async Task<ResultWithMessage> Update(int id, CreateReport input, TenantDto authUser)
        {
            if (id != input.Id)
            {
                return new ResultWithMessage(null, "Miss match in report Id");
            }
            //---------------------General Validations---------------------------------
            string accessResult = _jwtService.checkUserTenantPermission(authUser, input.DeviceId);
            if (accessResult == enAccessType.denied.GetDisplayName())
            {
                return new ResultWithMessage(null, "Access Denied");
            }
            if (_sharedService.IsExist(input.Id, input.DeviceId, null, null, input.Name))
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

            //-------------------------Edit Report with relations--------------------------
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    Report oldReport = null;
                    if (accessResult == enAccessType.allOnlyMe.GetDisplayName())
                    {
                        oldReport = _db.Reports.AsNoTracking().Include(x => x.Measures).Include(x => x.Levels).Include(x => x.FilterContainers)
                            .Include(x => x.ReportFieldValues).FirstOrDefault(x => x.Id == input.Id && x.CreatedBy == authUser.userName);

                    }
                    else
                    {
                        oldReport = _db.Reports.AsNoTracking().Include(x => x.Measures).Include(x => x.Levels).Include(x => x.FilterContainers)
                            .Include(x => x.ReportFieldValues).FirstOrDefault(x => x.Id == input.Id);

                    }
                    if (oldReport == null)
                    {
                        return new ResultWithMessage(null, "This Id is invalid or Access denied");
                    }
                    //------------------------Edit report prop----------------------------------
                    input.CreatedBy = oldReport.CreatedBy;
                    input.CreatedDate = oldReport.CreatedDate;
                    Report report = new Report(input, authUser.userName, DateTime.Now);
                    report.ChildId = (report.ChildId == 0 || report.ChildId == null) ? null : report.ChildId;
                    report.Measures = new List<ReportMeasure>();
                    //-----------------Edit Report Levels----------------
                    _db.ReportLevels.RemoveRange(oldReport.Levels);

                    report.Levels = input.Levels.Select((x, i) => new ReportLevel()
                    {
                        Id = 0,
                        DisplayOrder = i,
                        SortDirection = x.SortDirection,
                        ReportId = report.Id,
                        LevelId = x.LevelId

                    }).ToList();
                    //-----------------Edit Report Filters--------------
                    _db.ReportFilterContainers.RemoveRange(oldReport.FilterContainers);

                    report.FilterContainers = input.ContainerOfFilters.Select(x => new ReportFilterContainer()
                    {
                        Id = 0,
                        ReportId = report.Id,
                        LogicalOperator = x.LogicalOperator,
                        ReportFilters = x.ReportFilters.Select(y => new ReportFilter()
                        {
                            Id = 0,
                            LogicalOperator = y.LogicalOperator,
                            Value = _sharedService.ConvertListToString(y.Value),
                            FilterContainerId = x.Id,
                            LevelId = y.LevelId,
                            IsMandatory = y.IsMandatory,
                            IsVariable = y.IsVariable
                        }).ToList()

                    }).ToList();
                    //-----------------Edit Report Measures-------------
                    _db.ReportMeasures.RemoveRange(oldReport.Measures);

                    foreach (var s in oldReport.Measures)
                    {
                        _sharedService.deleteOperation(s.OperationId);
                    }

                    foreach (ReportMeasureDto m in input.Measures)
                    {
                        //if (_sharedService.IsExist(0, null, null, m.DisplayName, null))
                        //{
                        //	return new ResultWithMessage(null, "This Measure name :  " + m.DisplayName + "  alraedy exsit");

                        //}
                        if (Convert.ToBoolean(_sharedService.CheckValidFormat(m.Operation).Data) == false)
                        {
                            return new ResultWithMessage(null, _sharedService.CheckValidFormat(m.Operation).Message);
                        }

                        ReportMeasure measure = _mapper.Map<ReportMeasure>(m);
                        measure.ReportId = report.Id;
                        measure.Id = 0;
                        report.Measures.Add(measure);

                    }
                    //------------------------Extra Fields--------------------------
                    _db.ReportFieldValues.RemoveRange(oldReport.ReportFieldValues);
                    if (input.ReportFields != null)
                    {
                        bool isCorrectSave = _sharedService.AddExtraFields(null, (int)report.Id, input.ReportFields);
                        if (!isCorrectSave)
                        {
                            return new ResultWithMessage(null, "Mandatory Field error");
                        }
                    }
                    _db.Entry(oldReport).State = EntityState.Detached;
                    _db.Update(report);

                    _db.SaveChanges();
                    transaction.Complete();
                    return new ResultWithMessage(report, null);

                }
                catch (Exception ex)
                {
                    return new ResultWithMessage(null, ex.Message);

                }
            }


        }
        private ReportViewModel ConvertToViewModel(Report report)
        {
            ReportViewModel result = new ReportViewModel();
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
                    LevelName = y.Level.Name,
                    IsMandatory = y.IsMandatory,
                    Type = y.Level.DataType,
                    IsVariable = y.IsVariable
                }).ToList()
            }).ToList();
            result.Measures = GetReportMeasureById(report.Measures.ToList());
            return result;
        }

        public async Task<ResultWithMessage> GetReportRehearsal(int id, TenantDto authUser)
        {

            Report x = null;
            if (authUser.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
            {
                x = await _db.Reports.Where(x => !x.IsDeleted).Include(x => x.Device)
                .Include(x => x.Measures)
                .Include(x => x.Levels).ThenInclude(x => x.Level)
                .Include(x => x.FilterContainers).ThenInclude(x => x.ReportFilters).ThenInclude(x => x.Level).FirstOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                x = await _db.Reports.Where(x => !x.IsDeleted && x.CreatedBy == authUser.userName && (x.IsPublic || authUser.deviceAccesses.Select(x => x.DeviceId).ToList().Contains((int)x.DeviceId)))
                .Include(x => x.Device)
                .Include(x => x.Measures)
                .Include(x => x.Levels).ThenInclude(x => x.Level)
                .Include(x => x.FilterContainers).ThenInclude(x => x.ReportFilters).ThenInclude(x => x.Level)
               .FirstOrDefaultAsync(x => x.Id == id);
            }


            if (x == null)
            {
                return new ResultWithMessage(null, "Cannot find report");
            }
            var reportRehearsal = new ReportRehearsalModel
            {
                Name = x.Name,
                canEdit = checkEditValidation(authUser, x.DeviceId, x, _jwtService),
                Columns = x.Levels.OrderBy(l => l.DisplayOrder).Select(l => new ReportPreviewColumnModel
                {
                    Name = l.Level.Name,
                    Type = l.Level.DataType == "Date" ? "Date" : "String"
                }).Concat(
                        x.Measures.Select(m => new ReportPreviewColumnModel
                        {
                            Name = m.DisplayName,
                            Type = "Number"
                        })
                        ).ToList(),
                ContainerOfFilters = x.FilterContainers.Select(fc => new ContainerOfFilter
                {
                    Id = fc.Id,
                    LogicalOperator = fc.LogicalOperator,
                    LogicalOperatorName = fc.LogicalOperator.GetDisplayName(),
                    ReportFilters = fc.ReportFilters.Select(f => new ReportFilterDto
                    {
                        Id = f.Id,
                        IsMandatory = f.IsMandatory,
                        IsVariable = f.IsVariable,
                        LevelId = f.LevelId,
                        LevelName = f.Level.Name,
                        LogicalOperator = f.LogicalOperator,
                        LogicalOperatorName = f.LogicalOperator.GetDisplayName(),
                        Type = f.Level.DataType,
                        Value = f.Value == null ? null : f.Value.Split(',').ToArray()
                    }).ToList()
                }).ToList()
            };
            return new ResultWithMessage(reportRehearsal, "");
        }

        public async Task<ResultWithMessage> getReportDataById(int id, int pageSize, int pageIndex, List<ContainerOfFilter> filters, TenantDto authUser)
        {
            var report = (ReportViewModel)(await GetById(id,authUser)).Data;
            var queryWithSize = _queryBuilder.getReportQueryByViewModel(report, pageSize, pageIndex, filters);
            var data =  _dataProvider.fetchData(queryWithSize.Sql);
            var count = _dataProvider.fetchCount(queryWithSize.CountSql);
            return new ResultWithMessage(new DataWithSize(count, data), "");
        }
        public async Task<byte[]> exportReportDataById(int id, List<ContainerOfFilter> filters, TenantDto authUser)
        {
            var report = (ReportViewModel)(await GetById(id, authUser)).Data;
            var queryWithSize = _queryBuilder.getReportQueryByViewModel(report, int.MaxValue, 0, filters);
            var data = _dataProvider.fetchData(queryWithSize.Sql);
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(data);
                }
                return memoryStream.ToArray();   
            }
        }

        private static  bool checkEditValidation(TenantDto authUser , int deviceId , Report report, IJwtService _jwtService)
        {
            string accessResult = _jwtService.checkUserTenantPermission(authUser, deviceId);
            if (accessResult == enAccessType.denied.GetDisplayName() || accessResult == enAccessType.viewOnlyMe.GetDisplayName())
            {
                return false;
            }

            if (accessResult == enAccessType.allOnlyMe.GetDisplayName())
            {
                if(authUser.userName == report.CreatedBy)
                {
                    return true;

                }
                return false;
            }
            return false;
        }



    }
}