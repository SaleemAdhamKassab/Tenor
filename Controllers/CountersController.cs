using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Services.CountersService;
using Tenor.Services.CountersService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/Counters")]
    [ApiController]
    public class CountersController : Controller
    {
        private readonly ICountersService _counterservice;
        public CountersController(ICountersService counterservice) => _counterservice = counterservice;

        private IActionResult _returnResult(ResultWithMessage result) => !string.IsNullOrEmpty(result.Message) ? BadRequest(new ResultWithMessage(null, result.Message)) : Ok(new ResultWithMessage(result.Data, string.Empty));


        [HttpGet("getById/{id}")]
        public IActionResult getById(int id) => _returnResult(_counterservice.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getSubsetsByFilter([FromBody] CounterFilterModel filter) => _returnResult(_counterservice.getByFilter(filter));


        [HttpPost("addCounter")]
        public IActionResult addCounter(CounterBindingModel model) => _returnResult(_counterservice.addCounter(model));


        [HttpPut("updateCounter")]
        public IActionResult updateCounter(CounterBindingModel model) => _returnResult(_counterservice.updateCounter(model));


        [HttpDelete("deleteCounter")]
        public IActionResult deleteSubset(int id) => _returnResult(_counterservice.deleteCounter(id));

    }
}