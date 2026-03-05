using AIResearch.Architecture.Host.Constants;
using AIResearch.Architecture.Host.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AIResearch.Architecture.Host.Controllers;

[ApiController]
[ApiVersion(ApiConstants.Version)]
[Route("v{version:apiVersion}/[controller]")]
public class McpController(IMcpManifestGenerator manifestGenerator) : ControllerBase
{
    [HttpGet("manifest")]
    public IActionResult GetManifest()
    {
        var manifest = manifestGenerator.GenerateManifest(
            [typeof(ArchitectureController)],
            "IaResearch MCP",
            "A tool to generate .NET C# code scaffolds compliant with IaResearch architecture rules",
            "1.0.0"
        );

        return Content(manifest, "application/json");
    }
}
