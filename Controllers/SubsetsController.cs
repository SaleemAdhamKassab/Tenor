using Microsoft.AspNetCore.Mvc;
using Tenor.Services.SubsetsService;
using Tenor.Services.SubsetsService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/subsets")]
    [ApiController]
    public class SubsetsController : BaseController
    {
        private readonly ISubsetsService _subsetservice;
        public SubsetsController(ISubsetsService Subsetservice) => _subsetservice = Subsetservice;


        [HttpGet("getById")]
        public IActionResult getById(int id) => _returnResult(_subsetservice.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getByFilter([FromBody] SubsetFilterModel filter) => _returnResult(_subsetservice.getByFilter(filter));


        [HttpGet("getExtraFields")]
        public IActionResult getExtraFields() => _returnResult(_subsetservice.getExtraFields());


        [HttpPost("add")]
        public IActionResult add(SubsetBindingModel model) => _returnResult(_subsetservice.add(model));


        [HttpPut("edit")]
        public IActionResult edit(int id,SubsetBindingModel model) =>_returnResult(_subsetservice.edit(id,model));


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
    }
}