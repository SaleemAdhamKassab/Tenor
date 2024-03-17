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
			var response = await _kpiservice.GetKpiQuery(kpiid);
			//var response = new ResultWithMessage("select sum(c1) + sum(c2) from TECH4_123", string.Empty);
			try
			{
				//var result = _db.Database.SqlQuery<string>($"{response.Data.ToString()}");
				var result = _db.KPIResult.FromSqlRaw(response.Data.ToString()).ToList();
				return Ok(new { data = result });
			}
			catch (Exception ex)
			{
				return Ok(new { query = ex.Message });
			}

			//return Ok(_db.Database.ExecuteSql($"{response.Data.ToString()}"));
		}
	}
}