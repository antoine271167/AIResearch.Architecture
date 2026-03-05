using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AIResearch.Architecture.Contracts.Models.Requests;

public sealed record CodeGenerationRequestDto
{
    [JsonPropertyName("componentRole")]
    [Description(
        "The architectural role of the component to generate (e.g. CommandHandler, Repository, Entity). Determines the C# type kind, target layer, and overall code structure. MUST be a 'name' value from the component-roles endpoint — do NOT use role names that are not returned by that endpoint.")]
    public required string ComponentRole { get; init; }

    [JsonPropertyName("name")]
    [Description(
        "The C# type name for the generated component (e.g. CreateOrderHandler, Order). Used verbatim as the class, record, or interface name in the generated file.")]
    public required string Name { get; init; }

    [JsonPropertyName("layer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "The architecture layer to place the component in (e.g. Domain, Application, Infrastructure). Optional — inferred from ComponentRole if omitted. Specify only when the role is ambiguous.")]
    public string? Layer { get; init; }

    [JsonPropertyName("commands")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "Command class names this component handles or declares (e.g. CreateOrderCommand). For handlers, generates HandleAsync method stubs; for interfaces, generates method signatures. Optional — defaults to empty.")]
    public IReadOnlyCollection<string>? Commands { get; init; }

    [JsonPropertyName("comments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "Optional free-text hint or TODO note rendered as an inline comment in the generated source code. Useful for leaving instructions or context for the developer.")]
    public string? Comments { get; init; }

    [JsonPropertyName("dependencies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "Interfaces or services to inject into this component (e.g. IOrderRepository, IEventBus). Generates constructor parameters and infers the required using statements. Optional — defaults to empty.")]
    public IReadOnlyCollection<string>? Dependencies { get; init; }

    [JsonPropertyName("implementsInterfaces")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Description(
        "Interface names this component implements or extends (e.g. IOrderRepository). Added to the type declaration's inheritance clause in the generated code. Optional — defaults to empty.")]
    public IReadOnlyCollection<string>? ImplementsInterfaces { get; init; }
}