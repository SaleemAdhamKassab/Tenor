using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services.KpisService;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Controllers
{
    [Route("api/kpis")]
    [ApiController]
    public class KpisController : BaseController
    {
        private readonly IKpisService _kpiservice;
        public KpisController(IKpisService kpiservice) => _kpiservice = kpiservice;

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] KpiFilterModel kpitFilterModel)
        {
            var result = await _kpiservice.GetListAsync(kpitFilterModel);
            return Ok(result);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _kpiservice.GetByIdAsync(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateKpi kpi)
        {
            var result = await _kpiservice.Add(kpi);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, CreateKpi kpi)
        {
            if(id!=kpi.Id)
            {
                return BadRequest();
            }
            var result = await _kpiservice.Update(kpi);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(null);
        }

        [HttpGet("GetExtraFields")]
        public async Task<IActionResult> GetExtraFields()
        {
            var result = await _kpiservice.GetExtraFields();
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

    }
}