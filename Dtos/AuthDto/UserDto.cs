namespace Tenor.Dtos.AuthDto
{
    public class UserDto
    {
        public string userName { get; set; }
        public string refreshToken { get; set; }
        public DateTime tokenCreated { get; set; }
        public DateTime tokenExpired { get; set; }
    }
}
