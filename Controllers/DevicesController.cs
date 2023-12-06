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

        private IActionResult _returnResult(ResultWithMessage result) => 
            !string.IsNullOrEmpty(result.Message) ? BadRequest(new ResultWithMessage(null, result.Message)) :
            Ok(new ResultWithMessage(result.Data, string.Empty));


        [HttpGet("getById/{id}")]
        public IActionResult getById(int id) => _returnResult(_deviceService.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getByFilter([FromBody] DeviceFilterModel filter) => _returnResult(_deviceService.getByFilter(filter));


        [HttpGet("getSubsets")]
        public IActionResult getSubsets() => _returnResult(_deviceService.getSubsets());


        [HttpPost("addDevice")]
        public IActionResult addDevice(DeviceBindingModel model) => _returnResult(_deviceService.addDevice(model));


        [HttpPut("updateDevice")]
        public IActionResult updateDevice(DeviceBindingModel model) => _returnResult(_deviceService.updateDevice(model));


        [HttpDelete("deleteDevice")]
        public IActionResult deleteDevice(int id) => _returnResult(_deviceService.deleteDevice(id));
    }
}