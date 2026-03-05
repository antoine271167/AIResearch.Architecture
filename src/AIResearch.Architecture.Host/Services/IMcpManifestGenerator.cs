namespace AIResearch.Architecture.Host.Services;

public interface IMcpManifestGenerator
{
    string GenerateManifest(Type[] controllerTypes, string mcpName, string mcpDescription, string mcpVersion);
}
