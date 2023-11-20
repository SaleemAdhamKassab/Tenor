﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.ActionFilters;
using Tenor.Services.AuthServives;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TokenController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IWindowsAuthService _windowsAuthService;

        public TokenController(IHttpContextAccessor contextAccessor, IJwtService jwtService, IWindowsAuthService windowsAuthService)
        {
            _contextAccessor=contextAccessor;
            _jwtService=jwtService;
            _windowsAuthService=windowsAuthService;
        }

        [HttpGet]
        [Authorize]

        public ActionResult Get()
        {
            string userName=_windowsAuthService.GetLoggedUser();
            var responseToken=_jwtService.GenerateToken(userName);
            if(responseToken==null)
            {
                return Unauthorized();
            }
            return Ok(responseToken);
        }

        [HttpGet("refreshToken")]

        public ActionResult refreshToken()
        {
            string userName = _windowsAuthService.GetLoggedUser();
            var refresh = _jwtService.RefreshToken(userName);
            if(refresh==null)
            {
                return Unauthorized();
            }
            return Ok(refresh);
        }

        [HttpGet("TestToken")]
        [Authorize]
        [ServiceFilter(typeof(AuthTenant))]
        public ActionResult TestToken()
        {
            return Ok();
        }
    }
}
