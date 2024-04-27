using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using Tenor.ActionFilters;
using Tenor.Models;
using Tenor.Services.AuthServives;
using static System.Collections.Specialized.BitVector32;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : BaseController
    {
        private static ClaimsPrincipal authPrincipal = null;
        private readonly IJwtService _jwtService;


        public TokenController(IHttpContextAccessor contextAccessor, IJwtService jwtService):base(contextAccessor, jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
           
             authPrincipal = User;
             return _returnResult(_jwtService.GenerateToken(User));

        }

        [HttpPost("refreshToken")]
        public IActionResult refreshToken(RefreshTokenDto input)
        {          
            
            return _returnResult(_jwtService.RefreshToken(authPrincipal, input.RefreshToken));
        }
      

        [HttpPost("RevokeToken")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin" })]
        public IActionResult RevokeToken(RevokeTokenDto input)
        {
           
            return _returnResult(_jwtService.RevokeToken(input.token));
        }

       
      
    }
}
