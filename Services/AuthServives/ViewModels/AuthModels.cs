using System.ComponentModel.DataAnnotations;

namespace Tenor.Services.AuthServives.ViewModels
{
    public class AuthModels
    {
        public class ADUser
        {
            public string Name { get; set; }
            public string Alias { get; set; }

        }

        public class GroupTenantDto
        {
            public string groupName { get; set; }
            public string TenantName { get; set; }
            public string RoleName { get; set; }
        }

        public class RefreshToken
        {
            [Required]
            public string Token { get; set; }
            public DateTime Created { get; set; } = DateTime.Now;
            public DateTime Expired { get; set; }
        }

        public class TenantDto
        {
            public string userName { get; set; }
            public List<TenantAccess> tenantAccesses { get; set; }
        }

        public class TenantAccess
        {
            public string tenantName { get; set; }
            public List<string> RoleList { get; set; } = new List<string>();
        }

        public class TokenDto
        {
            public TenantDto userInfo { get; set; }
            public string token { get; set; }
            public string refreshToken { get; set; }
            public DateTime? ExpiryTime { get; set; }
        }

        public class UserDto
        {
            public string userName { get; set; }
            public string refreshToken { get; set; }
            public DateTime tokenCreated { get; set; }
            public DateTime tokenExpired { get; set; }
        }

        public class UserTenantDto
        {
            public string userName { get; set; }
            public string TenantName { get; set; }
            public string RoleName { get; set; }
        }

        public class RevokeTokenDto
        {
            public string ? token { get; set; }
        }

        public class RefreshTokenDto
        {
            public string? RefreshToken { get; set; }
        }
    }
}
