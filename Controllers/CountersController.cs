using Microsoft.AspNetCore.Mvc;
using Tenor.Services;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountersController : ControllerBase
    {
        private readonly ICounterService _counterService;

        public CountersController(ICounterService counterService) => _counterService = counterService;


        [HttpGet("GetAllSets")]
        public async Task<IActionResult> GetAllSets()
        {

            var result = await _counterService.GetAllSets();

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("GetAllCounters/{subsetId}")]
        public async Task<IActionResult> GettAllCounters(long subsetId)
        {
            var result = await _counterService.GetAllCounters(subsetId);

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
    }
}