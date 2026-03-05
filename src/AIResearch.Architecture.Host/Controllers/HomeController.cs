using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AIResearch.Architecture.Host.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index() => Ok(new
    {
        message = "IaResearch Architecture Server is running.",
        version = "v1.0",
        documentation = "/swagger"
    });
}