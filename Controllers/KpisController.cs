using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services;

namespace Tenor.Controllers
{
    [Route("api/kpis")]
    [ApiController]
    public class KpisController : Controller
    {
        private readonly IKpisService _kpiservice;
        public KpisController(IKpisService kpiservice) => _kpiservice = kpiservice;

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] KpiFilterModel kpitFilterModel)
        {
            return Ok(new DataWithSize<Kpi>(-1, null));
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(null);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Kpi kpi)
        {
            var result = await _kpiservice.Add(kpi);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, Kpi kpi)
        {
            return Ok(null);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(null);
        }
    }
}