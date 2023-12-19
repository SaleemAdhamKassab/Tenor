using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Services.CountersService;
using Tenor.Services.CountersService.ViewModels;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Tenor.Controllers
{
    [Route("api/Counters")]
    [ApiController]
    public class CountersController : BaseController
    {
        private readonly ICountersService _countersService;
        public CountersController(ICountersService counterservice) => _countersService = counterservice;

        [HttpGet("getById")]
        public IActionResult getById(int id) => _returnResult(_countersService.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getSubsetsByFilter(object filter) => _returnResult(_countersService.getByFilter(filter));


        [HttpGet("getExtraFields")]
        public async Task<IActionResult> getExtraFields() => _returnResult(_countersService.getExtraFields());


        [HttpPost("add")]
        public IActionResult add(CounterBindingModel model) => _returnResult(_countersService.add(model));


        [HttpPut("edit")]
        public IActionResult edit(CounterBindingModel model) => _returnResult(_countersService.edit(model));


        [HttpDelete("delete")]
        public IActionResult delete(int id) => _returnResult(_countersService.delete(id));

        [HttpPost("exportCounterByFilter")]
        public IActionResult exportDevicesByFilter(object filter)
        {
            var fileResult = _countersService.exportDevicesByFilter(filter);

            if (fileResult.Bytes == null || fileResult.Bytes.Count() == 0)
                return BadRequest(new { message = "No Data To Export." });

            return File(fileResult.Bytes, fileResult.ContentType, fileResult.FileName);
        }
    }
}