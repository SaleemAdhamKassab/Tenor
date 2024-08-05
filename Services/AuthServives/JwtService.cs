using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Models;
using static Tenor.Helper.Constant;
using static Tenor.Services.AuthServives.ViewModels.AuthModels;

namespace Tenor.Services.AuthServives
{

    public interface IJwtService
    {
        ResultWithMessage GenerateToken(ClaimsPrincipal principal);
        TenantDto TokenConverter(string token);
        bool CheckExpiredToken(string token);
        ResultWithMessage RefreshToken(ClaimsPrincipal principal, string reftoken);
        ResultWithMessage RevokeToken(string token);
        bool IsGrantAccess(string token, List<string> roles);
        string checkUserTenantPermission(TenantDto userData, int deviceid);
    }
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly TenorDbContext _dbcontext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static UserDto user = new UserDto();
        public JwtService(IConfiguration config,
            TenorDbContext dbcontext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _dbcontext = dbcontext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public ResultWithMessage GenerateToken(ClaimsPrincipal principal)
        {
            try
            {
                string userName = null;
                //Check if user has active token
                try
                {
                    userName = principal.Identity.Name;

                }
                catch
                {
                    userName = principal.Claims.ToList()[0].Value.ToString();

                }
                var userToken = _dbcontext.UserTokens.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower() && x.IsActive);
                if (userToken != null)
                {

                    RevokeToken(userToken.Token);
                }

                TenantDto TenantAccessData = CovertToTenantDto(principal);
                if (TenantAccessData == null || TenantAccessData.tenantAccesses.Count() == 0)
                {
                    return new ResultWithMessage(null, "Access denied");
                }
                string jwtToken = BuildToken(TenantAccessData);
                RefreshToken refTokent = GenerateRefreshToken();
                SetRefreshToken(refTokent);
                SetUserTokensCfg(userName, jwtToken, refTokent.Token);
                TokenDto result = new TokenDto
                {
                    userInfo = TenantAccessData,
                    token = jwtToken,
                    refreshToken = refTokent.Token,
                    ExpiryTime = DateTime.Now.AddMinutes(Convert.ToDouble(_config["JWT:TokenValidityInMinutes"]))
                };

                return new ResultWithMessage(result, null);
            }
            catch (Exception ex)
            {
                return new ResultWithMessage(null, ex.Message);

            }
        }
        public bool CheckExpiredToken(string token)
        {
            var username = TokenConverter(token).userName;

            try
            {

                var userToken = _dbcontext.UserTokens.FirstOrDefault(x => x.UserName.ToLower() == username.ToLower() && x.Token == token && x.IsActive);
                if (userToken == null)
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

                var tokenHandler = new JsonWebTokenHandler();
                var tokenPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters);
                var jwtSecurityToken = tokenPrincipal.SecurityToken as JsonWebToken;
                if (jwtSecurityToken == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
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
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JsonWebTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters);
                var jwtSecurityToken = principal.SecurityToken as JsonWebToken;

                if (jwtSecurityToken == null)
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
        public ResultWithMessage RefreshToken(ClaimsPrincipal principal, string reftoken)
        {

            string userName = null;
            try
            {
                userName = principal.Identity.Name;

            }
            catch
            {
                userName = principal.Claims.ToList()[0].Value.ToString();

            }
            if (!CheckExpiredUserRefreshToken(userName, reftoken))
            {
                return new ResultWithMessage(null, "Refresh token is invalid");
            }
            var userToken = _dbcontext.UserTokens.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower() && x.RefreshToken == reftoken && x.RefreshTokenExpired >= DateTime.Now && x.IsActive);
            if (userToken == null)
            {
                return new ResultWithMessage(null, "Refresh token is invalid");
            }
            userToken.IsActive = false;
            _dbcontext.Entry(userToken).State = EntityState.Modified;
            _dbcontext.SaveChanges();
            return GenerateToken(principal);

        }
        public ResultWithMessage RevokeToken(string token)
        {
            string username = TokenConverter(token).userName;
            List<UserToken> userTokens = _dbcontext.UserTokens.Where(x => x.UserName.ToLower() == username.ToLower() && x.Token == token && x.IsActive).ToList();
            if (userTokens != null || userTokens.Count != 0)
            {
                foreach (UserToken userToken in userTokens)
                {
                    userToken.IsActive = false;
                    _dbcontext.Entry(userToken).State = EntityState.Modified;
                    _dbcontext.SaveChanges();
                }

                return new ResultWithMessage(true, null);
            }
            return new ResultWithMessage(false, "token is invalid");
        }
        public bool IsGrantAccess(string token, List<string> roles)
        {
            var userName = TokenConverter(token).userName;
            TenantDto tenantDto = TokenConverter(token);
            if (CheckSuperAdminPermission(userName, roles))
            {
                return true;
            }
            if (tenantDto != null)
            {
                var userData = tenantDto.tenantAccesses.Where(x => tenantDto.tenantAccesses.Select(x => x.tenantName).ToList().Contains(x.tenantName)).ToList();
                if (userData != null)
                {
                    foreach (TenantAccess s in userData)
                    {
                        var userRoles = roles.Intersect(s.RoleList).ToList();
                        if (userRoles.Count() > 0)
                        {
                            return true;
                        }
                    }

                }
            }

            return false;
        }
        public string checkUserTenantPermission(TenantDto userData, int deviceid)
        {
            try
            {
                if (userData.tenantAccesses.Any(x => x.RoleList.Contains("SuperAdmin")))
                {
                    return enAccessType.all.GetDisplayName();
                }
                var tenants = _dbcontext.Tenants.Where(x => x.TenantDevices.Any(x => x.DeviceId == deviceid)).AsQueryable();
                var tenatRoles = userData.tenantAccesses.Where(x => tenants.Any(y => y.Name.Contains(x.tenantName))).ToList();

                if (tenatRoles is null)
                {
                    return enAccessType.denied.GetDisplayName();
                }

                foreach (List<string> s in tenatRoles.Select(x => x.RoleList))
                {
                    if (s.Contains("Admin"))
                    {
                        return enAccessType.all.GetDisplayName();
                    }
                    if (s.Contains("Editor"))
                    {
                        return enAccessType.allOnlyMe.GetDisplayName();

                    }
                    if (s.Contains("User"))
                    {
                        return enAccessType.viewOnlyMe.GetDisplayName();

                    }


                }
                return enAccessType.all.GetDisplayName();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

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
            var tokenHandler = new JsonWebTokenHandler();
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

            return tokenHandler.CreateToken(tokenDescriptor);
        }
        private TenantDto CovertToTenantDto(ClaimsPrincipal principal)
        {
            string userName = null;
            //Check if user has active token
            try
            {
                userName = principal.Identity.Name;

            }
            catch
            {
                userName = principal.Claims.ToList()[0].Value.ToString();

            }
            TenantDto tenantDto = new TenantDto();
            List<TenantAccess> tenantAccList = new List<TenantAccess>();
            List<DeviceAccess> deviceAccList = new List<DeviceAccess>();
            List<DeviceRole> deviceRoles = new List<DeviceRole>();

            var userIdentity = (ClaimsIdentity)principal.Identity;
            var claims = userIdentity.Claims;
            var roleClaimType = userIdentity.RoleClaimType;

            List<string> userGroups = claims.Where(c => c.Type == ClaimTypes.GroupSid).Select(x =>
              new System.Security.Principal.SecurityIdentifier(x.Value).Translate(
              typeof(System.Security.Principal.NTAccount)).ToString().ToLower()
              ).ToList();

            List<UserTenantDto> userTenant = _mapper.Map<List<UserTenantDto>>(_dbcontext.UserTenantRoles.Include(x => x.Tenant).ThenInclude(x => x.TenantDevices).ThenInclude(x => x.Device).Include(y => y.Role).Where(x => x.UserName.ToLower() == userName.ToLower()).ToList());
            List<GroupTenantDto> groupTenant = _mapper.Map<List<GroupTenantDto>>(_dbcontext.GroupTenantRoles.Include(x => x.Tenant).ThenInclude(x => x.TenantDevices).ThenInclude(x => x.Device).Include(y => y.Role).Where(x => userGroups.Contains(x.GroupName.ToLower())).ToList());

            //-----------------------------------------------

            foreach (var t in userTenant.Select(x => new { x.TenantName, x.tenantId }).Distinct().ToList())
            {
                TenantAccess userAccess = new TenantAccess();
                userAccess.tenantName = t.TenantName;
                userAccess.tenantId = t.tenantId;
                userAccess.RoleList.AddRange(userTenant.Where(x => x.userName.ToLower() == userName.ToLower() && x.TenantName == t.TenantName).Select(x => x.RoleName).ToList());
                tenantAccList.Add(userAccess);


            }

            foreach (var g in groupTenant.Select(x => new { x.groupName, x.TenantName ,x.tenantId }).Distinct().ToList())
            {
                TenantAccess groupAccess = new TenantAccess();
                groupAccess.tenantName = g.TenantName;
                groupAccess.tenantId = g.tenantId;
                groupAccess.RoleList.AddRange(groupTenant.Where(x => x.groupName.ToLower() == g.groupName.ToLower() && x.TenantName == g.TenantName).Select(x => x.RoleName).ToList());

                if (tenantAccList.Any(x => x.tenantName == g.TenantName))
                {
                    tenantAccList.Where(x => x.tenantName == g.TenantName).FirstOrDefault().RoleList
                      .AddRange(groupTenant.Where(x => x.groupName.ToLower() == g.groupName.ToLower() && x.TenantName == g.TenantName)
                      .Select(x => x.RoleName).ToList());
                }
                else
                {
                    tenantAccList.Add(groupAccess);

                }

            }

            tenantDto.userName = userName;
            tenantDto.profileUrl = _config["ProfileImg"].Replace("VarXXX", userName.Substring(userName.IndexOf("\\") + 1));
            tenantDto.thumbnailUrl = _config["ThumbImg"].Replace("VarXXX", userName.Substring(userName.IndexOf("\\") + 1));

            tenantDto.tenantAccesses = tenantAccList;
            tenantDto.deviceAccesses = TenantDeviceList(tenantAccList);
            return tenantDto;



        }
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expired
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            user.refreshToken = newRefreshToken.Token;
            user.tokenCreated = newRefreshToken.Created;
            user.tokenExpired = newRefreshToken.Expired;
        }
        private void SetUserTokensCfg(string userName, string token, string refreshToken)
        {
            UserToken userToken = new UserToken(userName, token,
            DateTime.Now.AddMinutes(Convert.ToDouble(_config["JWT:TokenValidityInMinutes"])),
            refreshToken, DateTime.Now.AddHours(Convert.ToDouble(_config["JWT:RefreshTokenValidityInHour"])));

            _dbcontext.UserTokens.Add(userToken);
            _dbcontext.SaveChanges();
        }
        private bool CheckExpiredCookiesRefreshToken()
        {
            string cookiesToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if (!user.refreshToken.Equals(cookiesToken) || (user.tokenExpired < DateTime.Now))
            {
                return false;
            }
            return true;

        }
        private bool CheckExpiredUserRefreshToken(string username, string refreshtoken)
        {
            var activeRefTokent = _dbcontext.UserTokens.FirstOrDefault(x => x.UserName.ToLower() == username.ToLower() && x.IsActive);
            if (activeRefTokent == null || !activeRefTokent.RefreshToken.Equals(refreshtoken))
            {
                return false;
            }
            if (activeRefTokent.RefreshTokenExpired < DateTime.Now)
            {
                RevokeToken(activeRefTokent.Token);
                return false;
            }

            return true;
        }
        private List<DeviceRole> TenantDeviceList(List<TenantAccess> tenantAccList)
        {

            List<DeviceRole> result = new List<DeviceRole>();
            foreach (var t in tenantAccList)
            {
                //var devices = _dbcontext.Devices.Where(x => x.TenantDevices.Any(y => y.TenantId == t.tenantId))
                //    .Select(c => new DeviceRole
                //    { 
                //        DeviceId = c.Id,
                //        Roles = t.RoleList
                //    }).ToList();

                var data = _dbcontext.Tenants.Where(x => x.Name == t.tenantName).Include(x => x.TenantDevices).ThenInclude(x => x.Device).ThenInclude(x => x.Childs).ToList();
                foreach (var d in data)
                {
                    List<DeviceRole> da = new List<DeviceRole>();
                    da = d.TenantDevices.Select(x => new DeviceRole() { DeviceId = x.DeviceId, DeviceName = x.Device.Name, Roles = t.RoleList }).ToList();
                    result.AddRange(da);


                    if (d.TenantDevices.Any(x => x.Device.Childs.Count() > 0))
                    {
                        foreach (var s in d.TenantDevices.Select(x => x.Device.Childs))
                        {
                            List<DeviceRole> daChilds = new List<DeviceRole>();
                            daChilds = s.Select(x => new DeviceRole() { DeviceId = x.Id, DeviceName = x.Name ,Roles = t.RoleList}).ToList();
                            result.AddRange(daChilds);
                        }
                    }
                }
            }
            return result.DistinctBy(x => x.DeviceName).ToList(); ;
        }
        private bool CheckSuperAdminPermission(string userName, List<string> roles)
        {
            if (roles.Contains("SuperAdmin"))
            {
                var permission = _dbcontext.UserTenantRoles.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower() && x.RoleId == 3);
                if (permission != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

