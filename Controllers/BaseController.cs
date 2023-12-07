using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.Dtos;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public IActionResult _returnResult(ResultWithMessage result) => !string.IsNullOrEmpty(result.Message) ? BadRequest(new { message = result.Message }) : Ok(new ResultWithMessage(result.Data, string.Empty));
    }
}
