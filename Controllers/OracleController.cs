using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenor.Data;
using Tenor.Dtos;
using Tenor.Services.KpisService;

namespace Tenor.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OracleController : ControllerBase
	{
		private readonly DataContext _db;
		private readonly IKpisService _kpiservice;

		public OracleController(DataContext dataContext, IKpisService kpiservice)
		{
			_db = dataContext;
			_kpiservice = kpiservice;
		}

		[HttpGet("GetKpiValue")]
		public async Task<IActionResult> GetKpiValue(int kpiid)
		{
			var response = await _kpiservice.GetKpiQueryByAmro(kpiid);
			try
			{
				var result = _db.Database.SqlQueryRaw<string>($"{response.Data.ToString()}").ToList();
				return Ok(new { data = result.FirstOrDefault() });
			}
			catch (Exception ex)
			{
				return Ok(new { data = ex.Message });
			}

		}
	}
}