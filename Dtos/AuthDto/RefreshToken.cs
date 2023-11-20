using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Tenor.Dtos.AuthDto
{
    public class RefreshToken
    {
        [Required]
        public string Token { get;set; }
        public DateTime Created { get; set; }=DateTime.Now;
        public DateTime Expired { get; set; }
    }
}
