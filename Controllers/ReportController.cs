using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.ActionFilters;
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
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IReportService _reportService;
        public ReportController(IHttpContextAccessor contextAccessor, IJwtService jwtService, IReportService reportService)
        {
            _contextAccessor= contextAccessor;
            _jwtService= jwtService;
            _reportService= reportService;
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

        private AuthModels.TenantDto AuthUser()
        {
            string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = Header.Split(' ').Last();
            var result = _jwtService.TokenConverter(token);
            if (result == null)
            {
                return null;
            }
            return result;
        }

    }
}
