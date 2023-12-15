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
        public IActionResult getByFilter([FromBody] object filter) => _returnResult(_subsetservice.getByFilter(filter));


        [HttpPost("add")]
        public IActionResult add(SubsetBindingModel model) => _returnResult(_subsetservice.add(model));


        [HttpPut("edit")]
        public IActionResult edit(SubsetBindingModel model) => _returnResult(_subsetservice.edit(model));


        [HttpDelete("delete")]
        public IActionResult delete(int id) => _returnResult(_subsetservice.delete(id));

        [HttpGet("GetExtraFields")]
        public IActionResult GetExtraFields()
        {
           return  _returnResult(_subsetservice.GetExtraFields());
       
        }
    }
}