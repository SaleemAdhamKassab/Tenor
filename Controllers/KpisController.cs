using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Dynamic;
using System.Text;
using Tenor.ActionFilters;
using Tenor.Dtos;
using Tenor.Helper;
using Tenor.Models;
using Tenor.Services.AuthServives;
using Tenor.Services.DevicesService.ViewModels;
using Tenor.Services.KpisService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Controllers
{
    [Route("api/kpis")]
    [ApiController]
    public class KpisController : BaseController
    {
        private readonly IKpisService _kpiservice;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IJwtService _jwtService;

        public KpisController(IKpisService kpiservice, IHttpContextAccessor contextAccessor, IJwtService jwtService)
        {
            _kpiservice = kpiservice;
            _contextAccessor = contextAccessor;
            _jwtService = jwtService;

        }

        [HttpPost("getByFilter")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User" })]

        public IActionResult getSubsetsByFilter(KpiFilterModel filter)
        {
          
            filter.UserName = GetUserName();
            return  _returnResult(_kpiservice.getByFilter(filter));

        }


        [HttpGet("getById")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User" })]

        public async Task<IActionResult> GetById(int id)
        {
             return _returnResult(await _kpiservice.GetByIdAsync(id));
        
        }

        [HttpPost("add")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User" })]

        public async Task<IActionResult> Post(CreateKpi kpi)
        {
            kpi.CreatedBy = GetUserName();
            return _returnResult(await _kpiservice.Add(kpi));
         
        }

        [HttpPut("edit")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User" })]

        public async Task<IActionResult> Put(int id, CreateKpi kpi)
        {
            kpi.ModifyBy = GetUserName();
            kpi.ModifyDate = DateTime.Now;
            return _returnResult(await _kpiservice.Update(id,kpi));
            
        }

        [HttpDelete("delete")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User" })]

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
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User" })]

        public IActionResult exportKpiByFilter(KpiFilterModel filter)
        {
            var fileResult = _kpiservice.exportKpiByFilter2(filter);

            if (fileResult.Bytes == null || fileResult.Bytes.Count() == 0)
                return BadRequest(new { message = "No Data To Export." });

            return File(fileResult.Bytes, fileResult.ContentType, fileResult.FileName);
        }

        [HttpGet("GetKpiQuery")]
        public async Task<IActionResult> GetKpiQuery(int kpiid)
        {
            return _returnResult(await _kpiservice.GetKpiQuery(kpiid));

        }

        [HttpGet("ValidateKpi")]
        public  IActionResult ValidateKpi(int? deviceid, string kpiname)
        {
            return _returnResult( _kpiservice.ValidateKpi(deviceid, kpiname));

        }

        [HttpPost("CheckFormatValidation")]
        public IActionResult CheckFormatValidation(CreateKpi input)
        {
            return _returnResult(_kpiservice.CheckValidFormat(input));

        }

        private string GetUserName()
        {
            string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = Header.Split(' ').Last();
            var result = _jwtService.TokenConverter(token);
            if (result == null)
            {
                return null;
            }
            return result.userName;
        }
    }
}