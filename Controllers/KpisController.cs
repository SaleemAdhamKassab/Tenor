using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Dynamic;
using System.Text;
using Tenor.Dtos;
using Tenor.Helper;
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
        private readonly IHttpContextAccessor _contextAccessor;

        public KpisController(IKpisService kpiservice, IHttpContextAccessor contextAccessor)
        {
            _kpiservice = kpiservice;
            _contextAccessor = contextAccessor;

        }

        [HttpPost("getByFilter")]
        public IActionResult getSubsetsByFilter(object filter)
        {
          return  _returnResult(_kpiservice.GetListAsync(filter));

        }


        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _kpiservice.GetByIdAsync(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Post(CreateKpi kpi)
        {
            var result = await _kpiservice.Add(kpi);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("edit")]
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

        [HttpDelete("delete")]
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

        [HttpGet("GetOperators")]
        public IActionResult GetOperators()
        {
            var result =  _kpiservice.GetOperators();
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("GetFunctions")]
        public IActionResult GetFunctions()
        {
            var result = _kpiservice.GetFunctions();
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }


    }
}