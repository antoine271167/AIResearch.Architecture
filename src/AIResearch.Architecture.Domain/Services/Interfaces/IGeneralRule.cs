namespace AIResearch.Architecture.Domain.Services.Interfaces;

/// <summary>
///     Marker interface for general architecture rules that don't fit into specific categories
///     like layer dependency, inference, or type kind rules.
/// </summary>
public interface IGeneralRule : IArchitectureRule;