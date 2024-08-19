using Microsoft.AspNetCore.Mvc;
using Tenor.ActionFilters;
using Tenor.Services.AuthServives;
using Tenor.Services.AuthServives.ViewModels;
using Tenor.Services.SubsetsService;
using Tenor.Services.SubsetsService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/subsets")]
    [ApiController]
    public class SubsetsController : BaseController
    {
        private readonly ISubsetsService _subsetservice;
        public SubsetsController(ISubsetsService Subsetservice , IHttpContextAccessor contextAccessor,
            IJwtService jwtService) : base(contextAccessor, jwtService)
        {
            _subsetservice = Subsetservice;
        }


        [HttpGet("getById")]
        public IActionResult getById(int id) => _returnResult(_subsetservice.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getByFilter([FromBody] SubsetFilterModel filter) => _returnResult(_subsetservice.getByFilter(filter));


        [HttpGet("getExtraFields")]
        public IActionResult getExtraFields() => _returnResult(_subsetservice.getExtraFields());


        [HttpPost("add")]
        public IActionResult add(SubsetBindingModel model) => _returnResult(_subsetservice.add(model));


        [HttpPut("edit")]
        public IActionResult edit(int id, SubsetBindingModel model) => _returnResult(_subsetservice.edit(id, model));


        [HttpDelete("delete")]
        public IActionResult delete(int id) => _returnResult(_subsetservice.delete(id));

        [HttpPost("exportSubsetByFilter")]
        public IActionResult exportSubsetByFilter(object filter)
        {
            var fileResult = _subsetservice.exportSubsetByFilter(filter);

            if (fileResult.Bytes == null || fileResult.Bytes.Count() == 0)
                return BadRequest(new { message = "No Data To Export." });

            return File(fileResult.Bytes, fileResult.ContentType, fileResult.FileName);
        }


        [HttpGet("validateSubset")]
        public IActionResult validateSubset(int deviceId, string name) => _returnResult(_subsetservice.validateSubset(deviceId, name));

        [HttpGet("GetSubsetByDevice")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

        public IActionResult GetSubsetByDevice(int deviceid, string? searchQuery)
        {
            var authData = AuthUser();

            return _returnResult(_subsetservice.GetSubsetByDeviceId(deviceid, searchQuery, authData));
        }

        [HttpGet("GetSubsetBySet")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

        public IActionResult GetSubsetBySet(int setId, string? searchQuery)
        {
            var authData = AuthUser();
            return _returnResult(_subsetservice.GetSubsetBySetId(setId, searchQuery, authData));
        }

    }
}