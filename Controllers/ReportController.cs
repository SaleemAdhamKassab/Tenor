using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Composition;
using System.Drawing.Printing;
using Tenor.ActionFilters;
using Tenor.Models;
using Tenor.Services.AuthServives;
using Tenor.Services.AuthServives.ViewModels;
using Tenor.Services.ReportService;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;
using static Tenor.Services.ReportService.ViewModels.ReportModels;

namespace Tenor.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReportController : BaseController
	{
		private readonly IReportService _reportService;
		public ReportController(IHttpContextAccessor contextAccessor, IJwtService jwtService,
			IReportService reportService) : base(contextAccessor, jwtService)
		{
			_reportService = reportService;
		}

		[HttpPost("add")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

		public async Task<IActionResult> Post(CreateReport input)
		{
			input.CreatedBy = AuthUser().userName;
			input.CreatedDate = DateTime.Now;
			var authData = AuthUser();
			return _returnResult(await _reportService.Add(input, authData));

		}

		[HttpDelete("softDelete")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

		public async Task<IActionResult> softDelete(int id)
		{
			var authData = AuthUser();
			return _returnResult(await _reportService.SoftDelete(id, authData));
		}

		[HttpDelete("hardDelete")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

		public async Task<IActionResult> hardDelete(int id)
		{
			var authData = AuthUser();
			return _returnResult(await _reportService.HardDelete(id, authData));
		}

		[HttpGet("getById")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public async Task<IActionResult> GetById(int id)
		{
			return _returnResult(await _reportService.GetById(id));

		}

		[HttpPost("getByFilter")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public async Task<IActionResult> getKpiByFilter(ReportListFilter filter)
		{

			var authData = AuthUser();
			return _returnResult(await _reportService.GetByFilter(filter, authData));

		}

		[HttpPost("GetReportTreeUserNames")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public async Task<IActionResult> GetReportTreeUserNames(ReportTreeFilter? input)
		{

			var authData = AuthUser();
			return _returnResult(await _reportService.GetReportTreeUserNames(input, authData));

		}

		[HttpPost("GetReportTreeDevicesByUserName")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public async Task<IActionResult> GetReportTreeDevicesByUserName(ReportTreeFilter? input)
		{

			var authData = AuthUser();
			return _returnResult(await _reportService.GetReportTreeDevicesByUserName(input, authData));

		}


		[HttpPost("GetReportTreeByUserNameDevice")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public async Task<IActionResult> GetReportTreeByUserNameDevice(ReportTreeFilter? input)
		{

			var authData = AuthUser();
			return _returnResult(await _reportService.GetReportTreeByUserNameDevice(input, authData));

		}

		[HttpPost("GetReportTreeByUserName")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public async Task<IActionResult> GetReportTreeByUserName(ReportTreeFilter? input)
		{

			var authData = AuthUser();
			return _returnResult(await _reportService.GetReportTreeByUserName(input, authData));

		}

		[HttpPost("getDimensionLevels")]
		public async Task<IActionResult> getDimensionLevels(List<ReportMeasureDto> reportMeasures)
		{
			return _returnResult(await _reportService.getDimensionLevels(reportMeasures));
		}
        [HttpPost("getDimensionFilters")]
        public async Task<IActionResult> getDimensionFilters(List<ReportMeasureDto> reportMeasures)
        {
            return _returnResult(await _reportService.getDimensionFilters(reportMeasures));
        }
        [HttpGet("getFilterOptions")]
		public IActionResult getFilterOptions(int levelId, string? searchQuery, int pageIndex, int pageSize)
		{
			return _returnResult(_reportService.getFilterOptions(levelId, searchQuery, pageIndex, pageSize));
		}

        [HttpGet("GetExtraFields")]

        public async Task<IActionResult> GetExtraFields(int? deviceId)
        {
            return _returnResult(await _reportService.GetExtraFields(deviceId));

        }

        [HttpGet("ValidateReport")]
        public IActionResult ValidateKpi(int? deviceid, string reportname)
        {
            var result = _reportService.ValidateReport(deviceid, reportname);
            return Ok(result.Data);

        }
		[HttpGet]
		public async Task<IActionResult> getReportRehearsal(int id)
        {
            return _returnResult(await _reportService.GetReportRehearsal(id));
        }
        [HttpPost("getReportData")]
		public IActionResult getReportData(int pageSize, int pageIndex, CreateReport report)
		{
			return _returnResult(_reportService.getReportDataByCreateReport(report, pageSize, pageIndex));
		}
        [HttpPost("getReportDataById")]
        public async Task<IActionResult> getReportDataById(int reportId, int pageSize, int pageIndex, List<ContainerOfFilter> filters)
        {
            return _returnResult(await _reportService.getReportDataById(reportId, pageSize, pageIndex, filters));
        }

        [HttpPut("edit")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

        public async Task<IActionResult> Put(int id, CreateReport input)
        {
            var authData = AuthUser();
            return _returnResult(await _reportService.Update(id,input, authData));

        }
    }
}
