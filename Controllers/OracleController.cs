using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Services.KpisService;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OracleController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly IKpisService _kpiservice;

        public OracleController(DataContext dataContext, IKpisService kpiservice)
        {
            _db = dataContext;
            _kpiservice = kpiservice;
        }

        [HttpGet("GetKpiValue")]
        public async Task<IActionResult> GetKpiValue(int kpiid)
        {
            var response = await _kpiservice.GetKpiQuery(kpiid);

            return Ok(_db.Database.ExecuteSql($"{response.Data.ToString()}"));
        }
    }
}