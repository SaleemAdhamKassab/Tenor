using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services.DevicesService;
using Tenor.Services.DevicesService.ViewModels;
using Tenor.Services.SubsetsService;
using Tenor.Services.SubsetsService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/subsets")]
    [ApiController]
    public class SubsetsController : Controller
    {
        private readonly ISubsetsService _subsetservice;
        public SubsetsController(ISubsetsService Subsetservice) => _subsetservice = Subsetservice;

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> getById(int id)
        {
            ResultWithMessage result = _subsetservice.getById(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpPost("getSubsetsByFilter")]
        public async Task<IActionResult> getSubsetsByFilter([FromBody] SubsetFilterModel filter)
        {
            ResultWithMessage result = _subsetservice.getSubsetsByFilter(filter);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpPost("addSubset")]
        public async Task<IActionResult> addSubset(SubsetBindingModel model)
        {
            var result = _subsetservice.addSubset(model);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpPut("updateSubset")]
        public async Task<IActionResult> updateSubset(SubsetBindingModel model)
        {
            var result = _subsetservice.updateSubset(model);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpDelete("deleteSubset")]
        public IActionResult deleteSubset(int id)
        {
            var result = _subsetservice.deleteSubset(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(null, string.Empty));
        }
    }
}