using Microsoft.AspNetCore.Mvc;
using Tenor.ActionFilters;
using Tenor.Controllers;
using Tenor.Models;

[Route("api/ExtraFields")]
[ApiController]
public class ExtraFieldController:BaseController
{
    private readonly IExtraPropService _extraPropService;
    public ExtraFieldController(IExtraPropService extraPropService)
    {
        _extraPropService = extraPropService;
    }

    [HttpPost("add")]
    [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "SuperAdmin" })]

    public IActionResult add(CreateExtraFieldViewModel input)
    {
        return _returnResult(_extraPropService.add(input));
    }


    [HttpPut("edit")]
    [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "SuperAdmin" })]

    public IActionResult edit(int id, CreateExtraFieldViewModel input)
    {
        return _returnResult(_extraPropService.edit(id,input));
    }


    [HttpDelete("delete")]
    [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "SuperAdmin" })]

    public IActionResult delete(int id)
    {
        return _returnResult(_extraPropService.delete(id));
    }

    [HttpPost("GetAll")]
    [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "SuperAdmin" })]

    public IActionResult GetAll(ExtraFieldFilter input)
    {
        return _returnResult(_extraPropService.GetAll(input));
    }

    [HttpGet("GetById")]
    [TypeFilter(typeof(AuthTenant), Arguments = new object[] { "SuperAdmin" })]

    public IActionResult GetById(int id)
    {
        return _returnResult(_extraPropService.GetById(id));
    }

}