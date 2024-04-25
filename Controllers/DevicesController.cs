using Microsoft.AspNetCore.Mvc;
using Tenor.ActionFilters;
using Tenor.Dtos;
using Tenor.Services.AuthServives;
using Tenor.Services.AuthServives.ViewModels;
using Tenor.Services.DevicesService;
using Tenor.Services.DevicesService.ViewModels;

namespace Tenor.Controllers
{
	[Route("api/devices")]
	[ApiController]
	public class DevicesController : BaseController
	{
		private readonly IDevicesService _deviceService;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly IJwtService _jwtService;
		public DevicesController(IDevicesService deviceService, IHttpContextAccessor contextAccessor, IJwtService jwtService)

		{
			_deviceService = deviceService;
			_contextAccessor = contextAccessor;
			_jwtService = jwtService;
		}

		private AuthModels.TenantDto AuthUser()
		{
			string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
			var token = Header.Split(' ').Last();
			var result = _jwtService.TokenConverter(token);
			if (result == null)
			{
				return null;
			}
			return result;
		}

		[HttpGet("getById")]
		public IActionResult getById(int id) => _returnResult(_deviceService.getById(id));


		[HttpPost("getByFilter")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]
		public IActionResult getByFilter([FromBody] DeviceFilterModel filter)
		{
			var authData = AuthUser();
			return _returnResult(_deviceService.getByFilter(filter, authData));

		}



		[HttpGet("getSubsets")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

		public IActionResult getSubsets() => _returnResult(_deviceService.getSubsets());


		[HttpPost("add")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

		public IActionResult add(DeviceBindingModel model) => _returnResult(_deviceService.add(model));


		[HttpPut("edit")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

		public IActionResult edit(int id, DeviceBindingModel model) => _returnResult(_deviceService.edit(id, model));


		[HttpDelete("delete")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,Editor,SuperAdmin" })]

		public IActionResult delete(int id) => _returnResult(_deviceService.delete(id));


		[HttpPost("exportDevicesByFilter")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

		public IActionResult exportDevicesByFilter(DeviceFilterModel filter)
		{
			var authData = AuthUser();

			var fileResult = _deviceService.exportDevicesByFilter(filter, authData);

			if (fileResult.Bytes == null || fileResult.Bytes.Count() == 0)
				return BadRequest(new { message = "No Data To Export." });

			return File(fileResult.Bytes, fileResult.ContentType, fileResult.FileName);
		}

		[HttpGet("validateDevice")]
		public IActionResult validateDevice(int deviceId, string name) => _returnResult(_deviceService.validateDevice(deviceId, name));

		[HttpGet("GetDeviceByParent")]
		[TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

		public IActionResult GetDeviceByParent(int parentid, string? searchQuery)
		{
			var authData = AuthUser();

			return _returnResult(_deviceService.GetDeviceByParentId(parentid, searchQuery, authData));
		}

		[HttpGet("getAllDevices")]
		public IActionResult getAllDevices() => _returnResult(_deviceService.getAllDevices(AuthUser()));


		[HttpGet("GetDevicesTree")]
        [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "Admin,User,Editor,SuperAdmin" })]

        public IActionResult GetTreeDevices(int? parentid, string ? searchQuery)
		{
			var authData= AuthUser();

			return _returnResult(_deviceService.getDevicesWithRelations(parentid, searchQuery, authData));

        }

    }
}