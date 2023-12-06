using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Services.DevicesService;
using Tenor.Services.DevicesService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/devices")]
    [ApiController]
    public class DevicesController : Controller
    {
        private readonly IDevicesService _deviceService;
        public DevicesController(IDevicesService deviceService) => _deviceService = deviceService;


        [HttpGet("getById/{id}")]
        public async Task<IActionResult> getById(int id)
        {
            ResultWithMessage result = _deviceService.getById(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpPost("getDevicesByFilter")]
        public async Task<IActionResult> getDevicesByFilter([FromBody] DeviceFilterModel filter)
        {
            ResultWithMessage result = _deviceService.getDevicesByFilter(filter);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpGet("getSubsets")]
        public IActionResult getSubsets()
        {
            var result = _deviceService.getSubsets();

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpPost("addDevice")]
        public async Task<IActionResult> addDevice(DeviceBindingModel model)
        {
            var result = _deviceService.addDevice(model);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpPut("updateDevice")]
        public async Task<IActionResult> updateDevice(DeviceBindingModel model)
        {
            var result = _deviceService.updateDevice(model);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(result.Data, string.Empty));
        }

        [HttpDelete("deleteDevice")]
        public IActionResult deleteDevice(int id)
        {
            var result = _deviceService.deleteDevice(id);

            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(new ResultWithMessage(null, result.Message));

            return Ok(new ResultWithMessage(null, string.Empty));
        }
    }
}