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
        private readonly IReportService _reportService;
        public ReportController(IHttpContextAccessor contextAccessor, IJwtService jwtService,
            IReportService reportService) : base(contextAccessor, jwtService)
        {
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


    }
}
