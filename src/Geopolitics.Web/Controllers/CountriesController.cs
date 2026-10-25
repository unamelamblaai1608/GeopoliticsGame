using Microsoft.AspNetCore.Mvc;
using Geopolitics.Core.Engines;

namespace Geopolitics.Web.Controllers;

[ApiController]
[Route("api/countries")]
public class CountriesController : ControllerBase
{
    // GET /api/countries
    [HttpGet]
    public IActionResult GetCountries()
    {
        return Ok(CountriesData.AllCountries);
    }
}

