using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Dynamic;
using System.Text;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.DevicesService.ViewModels;
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
        public IActionResult getSubsetsByFilter(KpiFilterModel filter)
        {
          return  _returnResult(_kpiservice.getByFilter(filter));

        }


        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int id)
        {
             return _returnResult(await _kpiservice.GetByIdAsync(id));
        
        }

        [HttpPost("add")]
        public async Task<IActionResult> Post(CreateKpi kpi)
        {
            return _returnResult(await _kpiservice.Add(kpi));
         
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Put(int id, CreateKpi kpi)
        {
            
            return _returnResult(await _kpiservice.Update(id,kpi));
            
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(null);
        }

        [HttpGet("GetExtraFields")]
        public async Task<IActionResult> GetExtraFields()
        {
            return _returnResult(await _kpiservice.GetExtraFields());
       
        }

        [HttpGet("GetOperators")]
        public IActionResult GetOperators()
        {
            return _returnResult( _kpiservice.GetOperators());

         
        }

        [HttpGet("GetFunctions")]
        public IActionResult GetFunctions()
        {
            return _returnResult(_kpiservice.GetFunctions());
       
        }

        [HttpPost("exportKpiByFilter")]
        public IActionResult exportKpiByFilter(object filter)
        {
            var fileResult = _kpiservice.exportKpiByFilter(filter);

            if (fileResult.Bytes == null || fileResult.Bytes.Count() == 0)
                return BadRequest(new { message = "No Data To Export." });

            return File(fileResult.Bytes, fileResult.ContentType, fileResult.FileName);
        }
    }
}