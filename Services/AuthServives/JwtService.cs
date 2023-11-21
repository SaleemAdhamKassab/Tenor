using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tenor.Data;
using Tenor.Dtos.AuthDto;
using Tenor.Models;

namespace Tenor.Services.AuthServives
{

    public interface IJwtService
    {
        TokenDto GenerateToken(string userName);
        bool CheckExpiredToken(string token);
        TenantDto TokenConverter(string token);
        bool CheckExpiredRefreshToken();
        TokenDto RefreshToken(string userName);

    }
    public class JwtService: IJwtService
    {
        private readonly IConfiguration _config;
        private readonly IWindowsAuthService _windowsAuthService;
        private readonly TenorDbContext _dbcontext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static UserDto user= new UserDto();
        public JwtService(IConfiguration config, IWindowsAuthService windowsAuthService,
            TenorDbContext dbcontext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _config=config;
            _windowsAuthService=windowsAuthService;
            _dbcontext=dbcontext;
            _mapper=mapper;
            _httpContextAccessor=httpContextAccessor;
        }

        public TokenDto GenerateToken(string userName)
        {
            try
            {
                TenantDto TenantAccessData = CovertToTenantDto(userName);
                if(TenantAccessData==null)
                {
                    return null;
                }
                string jwtToken = BuildToken(TenantAccessData);
                RefreshToken refTokent = GenerateRefreshToken();
                SetRefreshToken(refTokent);
                return new TokenDto {userInfo= TenantAccessData,
                    token = jwtToken,
                    refreshToken = refTokent.Token,
                    ExpiryTime= DateTime.Now.AddMinutes(Convert.ToDouble(_config["JWT:TokenValidityInMinutes"]))
                };

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool CheckExpiredToken(string token)
        {
            try
            {
                var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

        public TenantDto TokenConverter(string token)
        {
            try
            {
                var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                var tenantAuth = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type.ToLower().Contains("authorizationdecision")).Value;
                TenantDto TenantDtoObj = JsonConvert.DeserializeObject<TenantDto>(tenantAuth);
                return TenantDtoObj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool CheckExpiredRefreshToken()
        {
            string cookiesToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if(!user.refreshToken.Equals(cookiesToken) || (user.tokenExpired < DateTime.Now))
            {
                return false;
            }
            return true;

        }
        public TokenDto RefreshToken(string userName)
        {
            string cookiesToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if (!user.refreshToken.Equals(cookiesToken) || (user.tokenExpired < DateTime.Now))
            {
                return GenerateToken(userName);
            }

            return null;

        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expired = DateTime.Now.AddHours(Convert.ToDouble(_config["JWT:RefreshTokenValidityInHour"]))
            };
            return refreshToken;
        }
        private string BuildToken(TenantDto TenantAccessData)
        {
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));

            string TenantAccessDataStr = JsonConvert.SerializeObject(TenantAccessData);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = new SymmetricSecurityKey(hmac.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                 {
                      new Claim(ClaimTypes.AuthorizationDecision,TenantAccessDataStr),

                  }),
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_config["JWT:TokenValidityInMinutes"])),
                SigningCredentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private TenantDto CovertToTenantDto(string userName)
        {
            TenantDto tenantDto = new TenantDto();
            List<TenantAccess> tenantAccList = new List<TenantAccess>();
            // List<string> userGroups = _windowsAuthService.GetUserGroupsFromAD(userName);
            List<string> userGroups=new List<string>() {"MIS-TECH" };
            List<UserTenantDto> userTenant = _mapper.Map<List<UserTenantDto>>(_dbcontext.UserTenantRoles.Include(x => x.Tenant).Include(y => y.Role).Where(x => x.UserName == userName).ToList());
            List<GroupTenantDto> groupTenant = _mapper.Map<List<GroupTenantDto>>(_dbcontext.GroupTenantRoles.Include(x => x.Tenant).Include(y => y.Role).Where(x => userGroups.Contains(x.GroupName)).ToList());
            //-----------------------------------------------

            foreach(var t in userTenant.Select(x=>x.TenantName).Distinct().ToList())
            {
                TenantAccess userAccess = new TenantAccess();
                userAccess.tenantName = t;
                userAccess.RoleList.AddRange(userTenant.Where(x => x.userName == userName && x.TenantName==t).Select(x => x.RoleName).ToList());
                tenantAccList.Add(userAccess);
            }
                  
            foreach (var g in groupTenant.Select(x=>new { x.groupName, x.TenantName }).Distinct().ToList())
            {
                TenantAccess groupAccess = new TenantAccess();
                groupAccess.tenantName = g.TenantName;
                groupAccess.RoleList.AddRange(groupTenant.Where(x => x.groupName == g.groupName).Select(x => x.RoleName).ToList());
                tenantAccList.Add(groupAccess);
            }
            tenantDto.userName = userName;
            tenantDto.tenantAccesses = tenantAccList;
            return tenantDto;

        }
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires=newRefreshToken.Expired
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken",newRefreshToken.Token, cookieOptions);
            user.refreshToken = newRefreshToken.Token;
            user.tokenCreated = newRefreshToken.Created;
            user.tokenExpired = newRefreshToken.Expired;
        }

    }
}

