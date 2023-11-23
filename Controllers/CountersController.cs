using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services;

namespace Tenor.Controllers
{
    [Route("api/Counters")]
    [ApiController]
    public class CountersController : Controller
    {
        private readonly ICountersService _Counterservice;
        public CountersController(ICountersService Counterservice) => _Counterservice = Counterservice;

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] CounterFilterModel CounterFilterModel)
        {
            try
            {
                List<CounterDto> result = await _Counterservice.GetAsync(CounterFilterModel);

                if (result is null)
                    return NotFound(new ResultWithMessage(null, "No Data Found"));

                return Ok(new DataWithSize<CounterDto>(result.Count, result));
            }
            catch (Exception e)
            {
                return BadRequest(new ResultWithMessage(null, e.Message));
            }
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _Counterservice.GetAsync(id);

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CounterDto CounterDto)
        {
            var result = await _Counterservice.Add(CounterDto);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, CounterDto CounterDto)
        {
            var result = await _Counterservice.Update(id, CounterDto);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _Counterservice.Delete(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}