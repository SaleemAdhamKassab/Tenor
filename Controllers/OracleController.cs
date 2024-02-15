using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenor.Data;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OracleController : ControllerBase
    {
        private readonly DataContext _db;

        public OracleController(DataContext dataContext)
        {
            _db = dataContext;
        }

        [HttpGet]
        public IActionResult getTest()
        {
            return Ok(_db.Database.ExecuteSql($"select sysdate from dual"));
        }
    }
}