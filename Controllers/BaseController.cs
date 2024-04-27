using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Services.AuthServives;
using Tenor.Services.AuthServives.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IJwtService _jwtService;

        public BaseController(IHttpContextAccessor contextAccessor, IJwtService jwtService)
        {
            _contextAccessor=contextAccessor;
            _jwtService=jwtService;
        }
        protected AuthModels.TenantDto AuthUser()
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
        protected IActionResult _returnResult(ResultWithMessage result) => !string.IsNullOrEmpty(result.Message) ? BadRequest(new { message = result.Message }) : Ok(result.Data);
        protected IActionResult _returnResultWithMessage(ResultWithMessage result) => Ok(new { result.Data, result.Message });
    }
}
