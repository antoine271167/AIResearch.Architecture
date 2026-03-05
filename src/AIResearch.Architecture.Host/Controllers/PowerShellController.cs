using AIResearch.Architecture.Host.Constants;
using AIResearch.Architecture.Host.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AIResearch.Architecture.Host.Controllers;

[ApiController]
[ApiVersion(ApiConstants.Version)]
[Route("v{version:apiVersion}/[controller]")]
public class PowerShellController(IPowerShellScriptGenerator scriptGenerator) : ControllerBase
{
    /// <summary>
    /// Returns ready-to-use PowerShell helper functions. Copy and paste this entire script block, then call the functions.
    /// </summary>
    [HttpGet("helpers")]
    public IActionResult GetPowerShellHelpers()
    {
        var script = scriptGenerator.GenerateHelpers();
        return Content(script, "text/plain");
    }
}
