using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services;

namespace Tenor.Controllers
{
    [Route("api/subsets")]
    [ApiController]
    public class SubsetsController : Controller
    {
        private readonly ISubsetsService _Subsetservice;
        public SubsetsController(ISubsetsService Subsetservice) => _Subsetservice = Subsetservice;

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] SubsetFilterModel SubsetFilterModel)
        {
            //try
            //{
            //    List<SubsetDto> result = await _Subsetservice.GetAsync(SubsetFilterModel);

            //    if (result is null)
            //        return NotFound(new ResultWithMessage(null, "No Data Found"));

            //    return Ok(new DataWithSize<SubsetDto>(result.Count, result));
            //}
            //catch (Exception e)
            //{
            //    return BadRequest(new ResultWithMessage(null, e.Message));
            //}
            return Ok();
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _Subsetservice.GetAsync(id);

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Post(SubsetDto SubsetDto)
        {
            var result = await _Subsetservice.Add(SubsetDto);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, SubsetDto SubsetDto)
        {
            var result = await _Subsetservice.Update(id, SubsetDto);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _Subsetservice.Delete(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}