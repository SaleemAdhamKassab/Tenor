using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Tenor.Services;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountersController : ControllerBase
    {
        private readonly ICounterService _counterService;

        public CountersController(ICounterService counterService) => _counterService = counterService;


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var result = await _counterService.GetAllSets();

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

    }
}