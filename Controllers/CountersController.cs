﻿using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Services.CountersService;
using Tenor.Services.CountersService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/Counters")]
    [ApiController]
    public class CountersController : BaseController
    {
        private readonly ICountersService _counterservice;
        public CountersController(ICountersService counterservice) => _counterservice = counterservice;

        [HttpGet("getById/{id}")]
        public IActionResult getById(int id) => _returnResult(_counterservice.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getSubsetsByFilter(object filter) => _returnResult(_counterservice.getByFilter(filter));


        [HttpPost("add")]
        public IActionResult add(CounterBindingModel model) => _returnResult(_counterservice.add(model));


        [HttpPut("edit")]
        public IActionResult edit(CounterBindingModel model) => _returnResult(_counterservice.edit(model));


        [HttpDelete("delete")]
        public IActionResult delete(int id) => _returnResult(_counterservice.delete(id));

        [HttpGet("GetExtraFields")]
        public async Task<IActionResult> GetExtraFields()
        {
            var result = await _counterservice.GetExtraFields();
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}