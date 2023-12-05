﻿using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Dtos.KpiDto;
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
            return Ok();
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(null);
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
        public async Task<IActionResult> Put(int id, UpdateKpi kpi)
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