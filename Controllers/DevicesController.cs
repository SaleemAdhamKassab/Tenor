using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;
using Tenor.Services.DevicesService;
using Tenor.Services.DevicesService.ViewModels;

namespace Tenor.Controllers
{
    [Route("api/devices")]
    [ApiController]
    public class DevicesController : BaseController
    {
        private readonly IDevicesService _deviceService;
        public DevicesController(IDevicesService deviceService) => _deviceService = deviceService;

        [HttpGet("getById/{id}")]
        public IActionResult getById(int id) => _returnResult(_deviceService.getById(id));


        [HttpPost("getByFilter")]
        public IActionResult getByFilter([FromBody] DeviceFilterModel filter) => _returnResult(_deviceService.getByFilter(filter));


        [HttpGet("getSubsets")]
        public IActionResult getSubsets() => _returnResult(_deviceService.getSubsets());


        [HttpPost("add")]
        public IActionResult add(DeviceBindingModel model) => _returnResult(_deviceService.add(model));


        [HttpPut("edit")]
        public IActionResult edit(DeviceBindingModel model) => _returnResult(_deviceService.edit(model));


        [HttpDelete("delete")]
        public IActionResult delete(int id) => _returnResult(_deviceService.delete(id));
    }
}