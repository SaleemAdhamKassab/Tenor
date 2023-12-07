using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
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


        [HttpGet("getById/{id}")]
        public IActionResult getById(int id) => _returnResult(_subsetservice.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getByFilter([FromBody] SubsetFilterModel filter) => _returnResult(_subsetservice.getByFilter(filter));


        [HttpPost("addSubset")]
        public IActionResult addSubset(SubsetBindingModel model) => _returnResult(_subsetservice.addSubset(model));


        [HttpPut("updateSubset")]
        public IActionResult updateSubset(SubsetBindingModel model) => _returnResult(_subsetservice.updateSubset(model));


        [HttpDelete("deleteSubset")]
        public IActionResult deleteSubset(int id) => _returnResult(_subsetservice.deleteSubset(id));
    }
}