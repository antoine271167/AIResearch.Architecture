namespace AIResearch.Architecture.Contracts.Models;

public sealed record FileContent
{
    public required string Path { get; init; }
    public required string Code { get; init; }
}
