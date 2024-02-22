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
using Tenor.Models;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;

namespace Tenor.Services.AuthServives
{

    public interface IJwtService
    {
        TokenDto GenerateToken(ClaimsPrincipal principal);


        bool CheckExpiredToken(string username, string token);
        TenantDto TokenConverter(string token);
        bool CheckExpiredCookiesRefreshToken();
        bool CheckExpiredUserRefreshToken(string username, string refreshtoken);
        TokenDto RefreshToken(ClaimsPrincipal principal, string reftoken);
        bool RevokeToken(string username);
        bool IsGrantAccess(ClaimsPrincipal principal,string tenant,List<string> roles);
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

        public TokenDto GenerateToken(ClaimsPrincipal principal)
        {
            try
            {
                //Check if user has active token
                var userName = principal.Identity.Name;
                var userToken = _dbcontext.UserTokens.FirstOrDefault(x => x.UserName == userName && x.IsActive);
                if (userToken != null)
                {

                    RevokeToken(userName);
                }

                TenantDto TenantAccessData = CovertToTenantDto(principal);
                if(TenantAccessData==null)
                {
                    return null;
                }
                string jwtToken = BuildToken(TenantAccessData);
                RefreshToken refTokent = GenerateRefreshToken();
                SetRefreshToken(refTokent);
                SetUserTokensCfg(userName, jwtToken, refTokent.Token);
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
        public bool CheckExpiredToken(string username, string token)
        {
            try
            {
                var userToken = _dbcontext.UserTokens.FirstOrDefault(x=>x.UserName==username && x.Token==token && x.IsActive);
                if(userToken==null)
                {
                    return false;
                }

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
        public bool CheckExpiredCookiesRefreshToken()
        {
            string cookiesToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if(!user.refreshToken.Equals(cookiesToken) || (user.tokenExpired < DateTime.Now))
            {
                return false;
            }
            return true;

        }
        public bool CheckExpiredUserRefreshToken(string username,string refreshtoken)
        {
            var activeRefTokent = _dbcontext.UserTokens.FirstOrDefault(x=>x.UserName==username && x.IsActive);
            if (activeRefTokent == null || !activeRefTokent.RefreshToken.Equals(refreshtoken))
            {
                return false;
            }
            if (activeRefTokent.RefreshTokenExpired < DateTime.Now)
            {
                RevokeToken(username);
                return false;
            }
           
            return true;
        }
        public TokenDto RefreshToken(ClaimsPrincipal principal,string reftoken)
        {
            var userName = principal.Identity.Name;
            var userToken = _dbcontext.UserTokens.FirstOrDefault(x=>x.UserName==userName && x.RefreshToken==reftoken);
            if (userToken == null)
            {
                return null;
            }
            userToken.IsActive = false;
            _dbcontext.Entry(userToken).State = EntityState.Modified;
            _dbcontext.SaveChanges();
             return GenerateToken(principal);
           
        }
        public bool RevokeToken(string username)
        {
           List<UserToken> userTokens = _dbcontext.UserTokens.Where(x => x.UserName == username && x.IsActive).ToList();
            if (userTokens != null || userTokens.Count!=0)
            {
                 foreach(UserToken userToken in userTokens)
                {
                    userToken.IsActive = false;
                    _dbcontext.Entry(userToken).State = EntityState.Modified;
                    _dbcontext.SaveChanges();
                }
               
                return true;
            }
            return false;
        }

        public bool IsGrantAccess(ClaimsPrincipal principal, string tenant, List<string> roles)
        {
            string userName = principal.Identity.Name;
            TenantDto tenantDto = CovertToTenantDto(principal);
            if(tenantDto!=null)
            {
                var userData = tenantDto.tenantAccesses.FirstOrDefault(x => x.tenantName == tenant);
                if (userData != null)
                {
                    var userRoles = roles.Intersect(userData.RoleList).ToList();
                    if (userRoles.Count()>0)
                    {
                        return true;
                    }
                }
            }
                     
            return false;
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
        private TenantDto CovertToTenantDto(ClaimsPrincipal principal)
        {
            string userName = principal.Identity.Name;
                TenantDto tenantDto = new TenantDto();
                List<TenantAccess> tenantAccList = new List<TenantAccess>();
                var userIdentity = (ClaimsIdentity)principal.Identity;
                var claims = userIdentity.Claims;
                var roleClaimType = userIdentity.RoleClaimType;
            List<string> userGroups = claims.Where(c => c.Type == ClaimTypes.GroupSid).Select(x =>
                new System.Security.Principal.SecurityIdentifier(x.Value).Translate(
                typeof(System.Security.Principal.NTAccount)).ToString().ToLower()
                ).ToList();
            // List<string> userGroups=new List<string>() {"MIS-TECH" };
            List<UserTenantDto> userTenant = _mapper.Map<List<UserTenantDto>>(_dbcontext.UserTenantRoles.Include(x => x.Tenant).Include(y => y.Role).Where(x => x.UserName == userName).ToList());
                List<GroupTenantDto> groupTenant = _mapper.Map<List<GroupTenantDto>>(_dbcontext.GroupTenantRoles.Include(x => x.Tenant).Include(y => y.Role).Where(x => userGroups.Contains(x.GroupName)).ToList());
                //-----------------------------------------------

                foreach (var t in userTenant.Select(x => x.TenantName).Distinct().ToList())
                {
                    TenantAccess userAccess = new TenantAccess();
                    userAccess.tenantName = t;
                    userAccess.RoleList.AddRange(userTenant.Where(x => x.userName == userName && x.TenantName == t).Select(x => x.RoleName).ToList());
                    tenantAccList.Add(userAccess);
                }

                foreach (var g in groupTenant.Select(x => new { x.groupName, x.TenantName }).Distinct().ToList())
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
        private void SetUserTokensCfg(string userName,string token,string refreshToken)
        {
            UserToken userToken = new UserToken(userName,token,
            DateTime.Now.AddMinutes(Convert.ToDouble(_config["JWT:TokenValidityInMinutes"])),
            refreshToken,DateTime.Now.AddHours(Convert.ToDouble(_config["JWT:RefreshTokenValidityInHour"])));

            _dbcontext.UserTokens.Add(userToken);
            _dbcontext.SaveChanges();
        }
    }
}

