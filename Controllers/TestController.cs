using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenor.Data;
using Tenor.Models;

namespace Tenor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        private readonly TenorDbContext _db;

        public TestController(TenorDbContext tenorDbContext)
        {
            _db = tenorDbContext;
        }

        [HttpPost]

        public ActionResult CreateKpi(KPI model)
        {
            _db.KPIs.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }

    }
}
