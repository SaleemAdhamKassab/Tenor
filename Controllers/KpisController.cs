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
using Tenor.Services.AuthServives.ViewModels;
using Tenor.Services.DevicesService.ViewModels;
using Tenor.Services.KpisService;
using Tenor.Services.SharedService;
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
        private readonly ISharedService _sharedService;

        public KpisController(IKpisService kpiservice, IHttpContextAccessor contextAccessor,
            IJwtService jwtService, ISharedService sharedService)
        {
            _kpiservice = kpiservice;
            _contextAccessor = contextAccessor;
            _jwtService = jwtService;
            _sharedService=sharedService;
        }

        [HttpPost("getByFilter")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
        public IActionResult getKpiByFilter(KpiFilterModel filter)
        {
          
            var authData= AuthUser();
            return  _returnResult(_kpiservice.getByFilter(filter, authData));

        }


        [HttpGet("getById")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
        public async Task<IActionResult> GetById(int id)
        {
             return _returnResult(await _kpiservice.GetByIdAsync(id));
        
        }

        [HttpPost("add")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

        public async Task<IActionResult> Post(CreateKpi kpi)
        {
            kpi.CreatedBy = AuthUser().userName;
            var authData = AuthUser();
            return _returnResult(await _kpiservice.Add(kpi, authData));
         
        }

        [HttpPut("edit")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

        public async Task<IActionResult> Put(int id, CreateKpi kpi)
        {
            var authData = AuthUser();
            kpi.ModifyBy = AuthUser().userName;
            kpi.ModifyDate = DateTime.Now;
            return _returnResult(await _kpiservice.Update(id,kpi, authData));
            
        }

        [HttpDelete("delete")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

        public async Task<IActionResult> Delete(int id)
        {
            var authData = AuthUser();
            return _returnResult(await _kpiservice.Delete(id, authData));
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
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

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
            var result = await _kpiservice.GetKpiQuery(kpiid);
            return Ok(new { query= result.Data });

        }

        [HttpGet("GetKpiQueryByAmro")]
        public async Task<IActionResult> GetKpiQueryByAmro(int kpiid)
        {
            var result = await _kpiservice.GetKpiQueryByAmro(kpiid);
            return Ok(new { query = result.Data });

        }

        [HttpGet("ValidateKpi")]
        public  IActionResult ValidateKpi(int? deviceid, string kpiname)
        {
            var result = _kpiservice.ValidateKpi(deviceid, kpiname);
            return Ok(result.Data);

        }

        [HttpPost("CheckFormatValidation")]
        public IActionResult CheckFormatValidation(CreateKpi input)
        {
            return _returnResultWithMessage(_sharedService.CheckValidFormat(input.Operation));

        }

        private AuthModels.TenantDto AuthUser()
        {
            string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = Header.Split(' ').Last();
            var result = _jwtService.TokenConverter(token);
            if (result == null)
            {
                return null;
            }
            return result;
        }

    }
}