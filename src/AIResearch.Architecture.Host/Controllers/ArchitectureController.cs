using AIResearch.Architecture.Application.Commands.GenerateComponents;
using AIResearch.Architecture.Application.Commands.GenerateWorkspace;
using AIResearch.Architecture.Application.Queries.GetArchitecture;
using AIResearch.Architecture.Application.Queries.GetComponentRoles;
using AIResearch.Architecture.Application.Queries.GetRulesQuery;
using AIResearch.Architecture.Application.Queries.ValidateArchitectureQuery;
using AIResearch.Architecture.Contracts.Mediator;
using AIResearch.Architecture.Contracts.Models.Requests;
using AIResearch.Architecture.Contracts.Models.Responses;
using AIResearch.Architecture.Host.Attributes;
using AIResearch.Architecture.Host.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AIResearch.Architecture.Host.Controllers;

[ApiController]
[ApiVersion(ApiConstants.Version)]
[Route("v{version:apiVersion}/[controller]")]
public class ArchitectureController(
    IMediator mediator,
    ILogger<ArchitectureController> logger) : ControllerBase
{
    [HttpGet("get-architecture")]
    [McpAction(
        "Get the complete architecture descriptor including layers and styles. This action requires NO input parameters.")]
    [McpExample("Get architecture - no parameters needed", "get-architecture")]
    public async Task<ActionResult<GetArchitectureResponse>> GetArchitecture()
    {
        var result = await mediator.SendAsync(new GetArchitectureQuery());

        return Ok(new GetArchitectureResponse
        {
            Name = result.ArchitectureDescriptor.Name,
            Version = result.ArchitectureDescriptor.Version,
            Description = result.ArchitectureDescriptor.Description,
            Style = result.ArchitectureDescriptor.Style.ToString(),
            Layers = result.ArchitectureDescriptor.Layers.Select(layer => new ArchitectureLayerDto
            {
                Name = layer.Name,
                Description = layer.Description,
                AllowedDependencies = layer.AllowedDependencies
            }).ToList()
        });
    }

    [HttpGet("rules")]
    [McpAction("Get all architecture rules and conventions. This action requires NO input parameters.")]
    [McpExample("Get rules - no parameters needed", "rules")]
    public async Task<ActionResult<GetRulesResponse>> GetRules()
    {
        var result = await mediator.SendAsync(new GetRulesQuery());

        return Ok(new GetRulesResponse
        {
            Rules = result.Rules.Select(rule => new ArchitectureRuleDto
            {
                Id = rule.Id,
                Description = rule.Description
            }).ToList()
        });
    }

    [HttpGet("component-roles")]
    [McpAction("Get all available component roles with their metadata. This action requires NO input parameters. The returned roles are the ONLY valid values for the 'ComponentRole' property when requesting component generation. Do NOT invent or use role names that are not present in this list.")]
    [McpExample("Get component roles - no parameters needed", "component-roles")]
    public async Task<ActionResult<GetComponentRolesResponse>> GetComponentRoles()
    {
        var result = await mediator.SendAsync(new GetComponentRolesQuery());

        return Ok(new GetComponentRolesResponse
        {
            ComponentRoles = result.ComponentRoles.Select(role => new ComponentRoleDto
            {
                Name = role.Name,
                Layer = role.Layer.ToString(),
                TypeKind = role.TypeKind.ToString(),
                Description = role.Description,
                RequiresInterface = role.RequiresInterface,
                AlternativeNames = role.AlternativeNames
            }).ToList()
        });
    }

    [HttpPost("validate")]
    [McpAction(
        "Validate source code files against IaResearch architecture rules. Send a JSON object where each key is the file path and each value is the file content (do NOT wrap in a 'files' property).")]
    [McpExample(
        "Validate two files with a violation",
        """{"Domain/Order.cs":"namespace IaResearch.Architecture.Server.Domain { using IaResearch.Architecture.Server.Infrastructure; public class Order {} }","Application/OrderHandler.cs":"namespace IaResearch.Architecture.Server.Application { using IaResearch.Architecture.Server.Domain { public class OrderHandler {} }"}""",
        """{"violations":["Domain layer cannot depend on Infrastructure layer"]}"""
    )]
    public async Task<ActionResult<ValidateArchitectureResponse>> Validate([FromBody] Dictionary<string, string>? files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Status = "validation_error",
                Message =
                    "Request body must be a JSON object with file paths as keys and file contents as values. Example: {\"Domain/Order.cs\": \"namespace MyApp.Domain { public class Order {} }\"}"
            });
        }

        var result = await mediator.SendAsync(new ValidateArchitectureQuery(files));

        return Ok(new ValidateArchitectureResponse
        {
            Violations = result.Rules.Select(rule => rule.Description).ToList()
        });
    }

    [HttpPost("generate")]
    [EndpointDescription("Generate one or more components with explicit or inferred layers")]
    [McpAction(
        "Generate one or more C# components with explicit or inferred layers. REQUIRED: SolutionName and Features array. Each feature MUST have Name, and Components array. Each component MUST have ComponentRole and Name. Layer, Dependencies, Commands, and ImplementsInterfaces are OPTIONAL (will use defaults if omitted).")]
    [McpExample(
        "SIMPLEST: Generate component with minimal required fields",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order"}],"Components":[{"ComponentRole":"Entity","Name":"Order"}]}"""
    )]
    [McpExample(
        "Generate component with Feature and ApplicationKind specified",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order","ApplicationKind":"WebApi"}],"Components":[{"ComponentRole":"Entity","Name":"Order"}]}"""
    )]
    [McpExample(
        "Generate component with commands (Dependencies omitted)",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order"}],"Components":[{"ComponentRole":"CommandHandler","Name":"CreateOrderHandler","Commands":["CreateOrderCommand"]}]}"""
    )]
    [McpExample(
        "Generate component with explicit layer and dependencies",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order"}],"Components":[{"ComponentRole":"Repository","Name":"OrderRepository","Layer":"Infrastructure","Dependencies":["IOrderRepository"]}]}"""
    )]
    [McpExample(
        "Generate component implementing an interface",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order"}],"Components":[{"ComponentRole":"Repository","Name":"OrderRepository","Layer":"Infrastructure","ImplementsInterfaces":["IOrderRepository"]}]}"""
    )]
    [McpExample(
        "Generate multiple components (mix of minimal and detailed)",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order"}],"Components":[{"ComponentRole":"Entity","Name":"Order"},{"ComponentRole":"Command","Name":"CreateOrderCommand"},{"ComponentRole":"CommandHandler","Name":"CreateOrderCommandHandler","Commands":["CreateOrderCommand"]}]}"""
    )]
    public async Task<IActionResult> Generate([FromBody] GenerateComponentsRequest request)
    {
        var result = await mediator.SendAsync(new GenerateComponentsCommand(request));

        var response = new GenerateComponentsResponse
        {
            Status = result.Status,
            Files = result.Files?.ToDictionary(f => f.Path, f => f.Code),
            Component = result.Component,
            Reason = result.Reason,
            Message = result.Message,
            Options = result.Options,
            Violations = result.Violations
        };

        return response.Status == "ok" ? Ok(response) : BadRequest(response);
    }

    [HttpPost("generate-workspace")]
    [EndpointDescription("Generate solution and project structure according to architecture rules")]
    [McpAction(
        "Generate solution and project structure according to architecture rules. Creates or updates the complete solution structure including projects for each layer and feature. REQUIRED: SolutionName and Features array. Each feature MUST have Name. ArchitectureStyle and TargetFramework are OPTIONAL.")]
    [McpExample(
        "Create a simple WebApi solution with a single feature",
        """{"SolutionName":"IaResearch.OrderService","Features":[{"Name":"Order","ApplicationKind":"WebApi"}]}"""
    )]
    [McpExample(
        "Create a solution with multiple features and different application kinds",
        """{"SolutionName":"IaResearch.ECommerce","ArchitectureStyle":"CleanArchitecture","Features":[{"Name":"Order","ApplicationKind":"WebApi"},{"Name":"Customer","ApplicationKind":"WebApi"},{"Name":"Payment","ApplicationKind":"Worker"}],"TargetFramework":"net8.0"}"""
    )]
    public async Task<ActionResult<GenerateWorkspaceResponse>> GenerateWorkspace(
        [FromBody] GenerateWorkspaceRequest request)
    {
        logger.LogInformation("=== GenerateWorkspace START === SolutionName: {SolutionName}", request.SolutionName);

        try
        {
            var result = await mediator.SendAsync(new GenerateWorkspaceCommand(request));

            if (result.Status == "invalid")
            {
                logger.LogWarning("=== GenerateWorkspace VALIDATION ERROR === {Reason}", result.Reason);

                return BadRequest(new GenerateWorkspaceResponse
                {
                    Status = result.Status,
                    Reason = result.Reason
                });
            }

            // Log each file path for debugging
            foreach (var file in result.Files!)
            {
                logger.LogInformation("Generated file: {Path} ({Length} chars)", file.Path, file.Code.Length);
            }

            logger.LogInformation("=== GenerateWorkspace END === Returning {FileCount} files", result.Files.Count);

            return Ok(new GenerateWorkspaceResponse
            {
                Status = "ok",
                Files = result.Files.ToDictionary(f => f.Path, f => f.Code)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "=== GenerateWorkspace ERROR === {Message}", ex.Message);

            return StatusCode(500, new ErrorResponse
            {
                Error = ex.Message,
                StackTrace = ex.StackTrace
            });
        }
    }
}