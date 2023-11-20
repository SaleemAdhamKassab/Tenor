using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("GetBySubsetId/{subsetId}")]
        public async Task<IActionResult> GetBySubsetId(long subsetId)
        {
            var result = await _counterService.GetBySubsetId(subsetId);

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
    }
}