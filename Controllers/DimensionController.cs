using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.Services.AuthServives;
using Tenor.Services.DimensionService;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DimensionController : BaseController
    {
        private readonly IDimensionsService _dimensionsService;

        public DimensionController(IDimensionsService dimensionsService, IHttpContextAccessor contextAccessor,
            IJwtService jwtService) : base(contextAccessor, jwtService)
        {
            _dimensionsService = dimensionsService;
        }

        [HttpGet("GetDimLevelByDevice")]
        public IActionResult GetDimLevelByDevice(int deviceId, string? searchQuery)
        {
            return _returnResult(_dimensionsService.GetDimLevelByDevice(deviceId, searchQuery));
        }
    }
}
