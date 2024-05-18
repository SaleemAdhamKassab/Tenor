using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.ActionFilters;
using Tenor.Models;
using Tenor.Services.AuthServives;
using Tenor.Services.AuthServives.ViewModels;
using Tenor.Services.ReportService;
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
		public async Task<IActionResult> getDimensionLevels(List<ReportMeasure> reportMeasures)
		{
			return _returnResult(await _reportService.getDimensionLevels(reportMeasures));
		}
	}
}
