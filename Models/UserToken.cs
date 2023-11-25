using System.ComponentModel.DataAnnotations;

namespace Tenor.Models
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Token { get; set; }
        public DateTime TokenExpired { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpired { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public UserToken() { }
        public UserToken(string userName, string token, DateTime tokenExpired, string refreshToken, DateTime refreshTokenExpired)
        {
            UserName=userName;
            Token=token;
            TokenExpired=tokenExpired;
            RefreshToken = refreshToken;
            RefreshTokenExpired=refreshTokenExpired;
            CreatedAt=DateTime.Now;
            IsActive = true;
        }

    }
}
