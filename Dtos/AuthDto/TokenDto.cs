namespace Tenor.Dtos.AuthDto
{
    public class TokenDto
    {
        public TenantDto userInfo { get;set; }
        public string token { get;set; }
        public string refreshToken { get; set; }
        public DateTime ? ExpiryTime { get; set; }
    }
}
