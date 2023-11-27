using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using Tenor.ActionFilters;
using Tenor.Models;
using Tenor.Services.AuthServives;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TokenController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IJwtService _jwtService;
        public readonly IWindowsAuthService _windowsAuthService;

        public TokenController(IHttpContextAccessor contextAccessor, IJwtService jwtService, IWindowsAuthService windowsAuthService)
        {
            _contextAccessor = contextAccessor;
            _jwtService = jwtService;
            _windowsAuthService = windowsAuthService;
        }

        [HttpGet]
        public ActionResult Get()
        {
            string userName = _windowsAuthService.GetLoggedUser();
            var responseToken = _jwtService.GenerateToken(userName);
            if (responseToken == null)
            {
                return Unauthorized();
            }
            return Ok(responseToken);
        }

        [HttpGet("refreshToken")]
        public ActionResult refreshToken()
        {
            string userName = _windowsAuthService.GetLoggedUser();
            string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = Header.Split(' ').Last();
            var refresh = _jwtService.RefreshToken(userName, token);
            if (refresh == null)
            {
                return Unauthorized();
            }
            return Ok(refresh);
        }

        [HttpGet("TestToken")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor" })]
        public ActionResult TestToken()
        {
            string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            if(Header==null)
            {
                return Unauthorized();
            }
            var token = Header.Split(' ').Last();
            return Ok(_jwtService.TokenConverter(token));
        }
        [HttpGet("RevokeToken")]
        public ActionResult RevokeToken(string userName)
        {
            return Ok(_jwtService.RevokeToken(userName));
        }

    }
}
