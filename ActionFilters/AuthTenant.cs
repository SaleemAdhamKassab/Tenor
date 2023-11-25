using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tenor.Data;
using Tenor.Models;
using Tenor.Services.AuthServives;

namespace Tenor.ActionFilters
{
    public class AuthTenant : IAuthorizationFilter, IActionFilter
    {
        private readonly IWindowsAuthService _windowsAuthService;
        private readonly IJwtService _jwtService;
        private readonly TenorDbContext _dbContext;

        public AuthTenant(IWindowsAuthService windowsAuthService, TenorDbContext dbContext, IJwtService jwtService)
        {
            _windowsAuthService = windowsAuthService;
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        public void OnActionExecuted (ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string Header = context.HttpContext.Request.Headers["Authorization"];
            var winUser = _windowsAuthService.GetLoggedUser();

            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) && Header.ToLower().StartsWith("bearer "))
            {
                var token = authHeader.FirstOrDefault()?.Split(' ').Last();

                // check token and refresh token
                if (_jwtService.CheckExpiredToken(token))
                {
                    return;
                }

                if (!_jwtService.CheckExpiredUserRefreshToken(winUser, token))
                {
                    context.Result = new UnauthorizedObjectResult("Token Expired");
                    return;
                }

                return;
            }

            context.Result = new UnauthorizedObjectResult("Contact to system Administrator");
            return;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var winUser = _windowsAuthService.GetLoggedUser();
            var clientIPAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

            if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                clientIPAddress = forwardedFor.ToString().Split(", ")[0];
            }
            // Get the "Authorization" header from the request
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                // The "Authorization" header is missing, so return a 401 Unauthorized response
                AccessLog log = new AccessLog(winUser, clientIPAddress, "Token is null in the header request.",DateTime.Now);
                _dbContext.AccessLogs.Add(log);
                _dbContext.SaveChanges();
                context.Result = new UnauthorizedObjectResult("Please Contact to administrator. ");
                return;
            }
            var token = authHeader.FirstOrDefault()?.Split(' ').Last();
            if (token == null)
            {

                AccessLog log = new AccessLog(winUser, clientIPAddress, "Token is null in the header request.",DateTime.Now);
                _dbContext.AccessLogs.Add(log);
                _dbContext.SaveChanges();
                context.Result = new UnauthorizedObjectResult("Please Contact to administrator. ");
                return;
            }

        }
    }
}
