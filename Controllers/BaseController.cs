using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IActionResult _returnResult(ResultWithMessage result) => !string.IsNullOrEmpty(result.Message) ? BadRequest(new { message = result.Message }) : Ok(result.Data);
        protected IActionResult _returnResultWithMessage(ResultWithMessage result) => Ok(new { result.Data, result.Message });
    }
}
