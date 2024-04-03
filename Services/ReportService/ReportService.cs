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

namespace Tenor.Services.ReportService
{
    public interface IReportService
    {
        Task<ResultWithMessage> Add(CreateReport input, TenantDto authUser);

    }
    public class ReportService : IReportService
    {
        private readonly TenorDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ISharedService _sharedService;
        private readonly IMapper _mapper;

        public ReportService(TenorDbContext db, IJwtService jwtService, ISharedService sharedService, IMapper mapper)
        {
            _db=db;
            _jwtService=jwtService;
            _sharedService=sharedService;
            _mapper=mapper;
        }

        public async Task<ResultWithMessage> Add(CreateReport input, TenantDto authUser)
        {
            //---------------------General Validations---------------------------------
            string accessResult = _jwtService.checkUserTenantPermission(authUser, input.DeviceId);
            if (accessResult == enAccessType.denied.GetDisplayName())
            {
                return new ResultWithMessage(null, "Access Denied");
            }

            if (_sharedService.IsExist(0,input.DeviceId,null,null,input.Name))
            {
                return new ResultWithMessage(null, "This Report name alraedy exsit on the same device");

            }
            foreach(var s in input.Measures)
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
                    _db.Reports.Add(report);
                    _db.SaveChanges();
                    if (input.ReportFields != null)
                    {
                        bool isCorrectSave = _sharedService.AddExtraFields(null,(int) report.Id, input.ReportFields);
                        if (!isCorrectSave)
                        {
                            return new ResultWithMessage(null, "Mandatory Field error");
                        }
                    }
                    //-----------------Create Report Levels----------------
                    foreach(ReportLevelDto s in input.Levels)
                    {
                        ReportLevel reportLevel = new ReportLevel(s.Id,s.DisplayOrder,s.SortDirection,report.Id,s.DimensionLevelId);
                        _db.ReportLevels.Add(reportLevel);
                        _db.SaveChanges();

                    }
                    //-----------------Create Report Filters--------------
                    foreach (ReportFilterDto r in input.Filters)
                    {

                        ReportFilter reportFilter = new ReportFilter(r.Id,r.LogicalOperator,_sharedService.ConvertListToString(r.Value),report.Id,r.DimensionLevelId);
                        _db.ReportFilters.Add(reportFilter);
                        _db.SaveChanges();


                    }
                    //-----------------Create Report Measures-------------
                    foreach (ReportMeasureDto m in input.Measures)
                    {
                        if (_sharedService.IsExist(0, null, null, m.DisplayName, null))
                        {
                            return new ResultWithMessage(null, "This Measure name :  "+m.DisplayName+"  alraedy exsit");

                        }
                        if (Convert.ToBoolean(_sharedService.CheckValidFormat(m.Operation).Data) == false)
                        {
                            return new ResultWithMessage(null,_sharedService.CheckValidFormat(m.Operation).Message);
                        }

                        ReportMeasure measure = _mapper.Map<ReportMeasure>(m);
                        measure.ReportId = report.Id;
                        _db.ReportMeasures.Add(measure);
                        _db.SaveChanges();

                        //----------------- Havings--------------

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

        
    }
}
