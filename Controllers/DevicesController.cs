﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net;
using Tenor.Dtos;
using Tenor.Models;
using Tenor.Services;

namespace Tenor.Controllers
{
    [Route("api/devices")]
    [ApiController]
    public class DevicesController : Controller
    {
        private readonly IDevicesService _deviceService;
        public DevicesController(IDevicesService deviceService) => _deviceService = deviceService;

        [HttpPost("Get")]
        public async Task<IActionResult> Get([FromBody] DeviceFilterModel deviceFilterModel)
        {
            try
            {
                List<DeviceDto> result = await _deviceService.GetAsync(deviceFilterModel);

                if (result is null)
                    return NotFound(new ResultWithMessage(null, "No Data Found"));

                return Ok(new DataWithSize<DeviceDto>(result.Count, result));
            }
            catch (Exception e)
            {
                return BadRequest(new ResultWithMessage(null, e.Message));
            }
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _deviceService.GetAsync(id);

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("GetSubsets")]
        public async Task<IActionResult> GetSubsets()
        {

            var result = await _deviceService.GetSubsetsAsync();

            if (!string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Post(DeviceDto deviceDto)
        {
            var result = await _deviceService.Add(deviceDto);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut]
        public async Task<IActionResult> Put(int id, DeviceDto deviceDto)
        {
            var result = await _deviceService.Update(id, deviceDto);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _deviceService.Delete(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}