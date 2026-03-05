using AIResearch.Architecture.Host.Constants;

namespace AIResearch.Architecture.Host.Services;

public class BaseUrlResolver(IHttpContextAccessor httpContextAccessor)
{
    public string GetBaseUrl()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        return request == null 
            ? $"{ApiConstants.Scheme}://{ApiConstants.Host}:{ApiConstants.Port}" 
            : $"{request.Scheme}://{request.Host}";
    }

    public string GetVersionedUrl(string controller) =>
        $"{GetBaseUrl()}/v{ApiConstants.Version}/{controller}";

    public string GetArchitectureBaseUrl() =>
        GetVersionedUrl("Architecture");

    public string GetPowerShellHelpersUrl() =>
        $"{GetVersionedUrl("PowerShell")}/helpers";

    public string GetMcpManifestUrl() =>
        $"{GetVersionedUrl("Mcp")}/manifest";

    public string GetSwaggerJsonUrl() =>
        $"{GetBaseUrl()}/swagger/v{ApiConstants.Version}/swagger.json";
}
