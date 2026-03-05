# AIResearch.Architecture MCP tool

A REST API that generates and validates C# .NET solution scaffolds conforming to Clean Architecture rules, designed for consumption by AI agents via the Model Context Protocol (MCP).

## Why This Project Exists

AI coding agents are incredibly capable. They can generate features, refactor code, and scaffold entire solutions in seconds. But when it comes to architecture, they still tend to guess. They invent folder structures. They improvise namespaces. They quietly bend dependency rules. Not because they are careless � but because architecture usually lives in documents, conventions, and tribal knowledge.

In real-world .NET projects, especially in enterprise environments, architecture is not a suggestion. Layer boundaries matter. Dependency direction matters. Naming conventions matter. A CommandHandler is not just a class � it belongs in a specific layer, in a specific project, with a specific name and type. When those rules are applied inconsistently, the solution slowly drifts.

This project started as an experiment: what if the AI didn�t have to remember or invent any of that?

Instead of asking the AI to decide where something should live or how it should be named, the AI simply declares intent:
�I need a CommandHandler.�
�I need a Repository.�
�I need an Interface.�

The MCP service then determines the rest. It decides the correct layer, project, namespace, file location, and type kind. Architecture becomes executable policy instead of polite documentation.

The core idea is simple but powerful:
The AI generates behavior.
The MCP governs structure.

By moving architectural decisions into a machine-readable, enforceable system, we reduce drift, eliminate guesswork, and make AI far more valuable in structured environments. This project explores what happens when architecture is no longer something the AI must infer � but something it must consult.

It is an investigation into architectural governance for AI-driven development.

## Additional Documentation

- [The Generate Flow](https://github.com/antoine271167/AIResearch.Architecture/blob/main/docs/Generate-Flow_Architecture.md) : Documents the end-to-end POST /v1/Architecture/generate flow, covering request validation, layer inference, dependency rules, code-generation steps, and response assembly.
- [The PowerShell Helpers Flow](https://github.com/antoine271167/AIResearch.Architecture/blob/main/docs/PowerShell-Helpers-Flow_Architecture.md) :  Documents the generation of a runtime-resolved, PowerShell script that exposes wrapper functions for all Architecture API endpoints to simplify and harden AI/human invocations while remaining a Host-layer presentation concern.
- [The MCP Manifest Flow](https://github.com/antoine271167/AIResearch.Architecture/blob/main/docs/MCP-Manifest-Flow_Architecture.md) :  High-level design and runtime flow for the GET /v1.0/Mcp/manifest endpoint, detailing components, URL resolution, reflection-based schema generation, and how architecture rules are exposed to AI agents.

## Features

- **Get architecture descriptor** � returns the layer definitions (Domain, Application, Infrastructure, WebApi) and their allowed dependency directions
- **Get architecture rules** � returns all coded rules with descriptions (dependency constraints, interface requirements, accessibility requirements)
- **Get component roles** � returns the complete catalogue of valid roles (`Entity`, `CommandHandler`, `Repository`, `DomainService`, `Controller`, etc.) with their target layer and C# type kind
- **Generate C# components** � scaffolds files (class, record, interface, struct) for the given component roles, with correct namespace, `using` statements, constructor injection stubs, and interface implementations; layer is inferred from the role when not specified, see also: [Generate-Flow_Architecture.md](https://github.com/antoine271167/AIResearch.Architecture/blob/main/docs/Generate-Flow_Architecture.md)
- **Generate workspace** � scaffolds a full `.sln` and `.csproj` project tree for one or more features across all architecture layers
- **Validate architecture** � accepts a dictionary of file paths ? source content and reports layer dependency violations
- **MCP manifest** � exposes a machine-readable manifest (`/v1.0/Mcp/manifest`) that AI clients use to discover all available actions, see also: [MCP-Manifest-Flow_Architecture.md](https://github.com/antoine271167/AIResearch.Architecture/blob/main/docs/MCP-Manifest-Flow_Architecture.md)
- **PowerShell helpers** � exposes ready-to-use PowerShell functions (`/v1.0/PowerShell/helpers`) that AI agents load before calling the API, see also: [PowerShell-Helpers-Flow_Architecture.md](https://github.com/antoine271167/AIResearch.Architecture/blob/main/docs/PowerShell-Helpers-Flow_Architecture.md)

## Installation

This project is a runnable ASP.NET Core application, not a NuGet package.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build and run

```bash
git clone clone https://github.com/antoine271167/AIResearch.Architecture.git
cd AIResearch.Architecture/src

dotnet restore
dotnet build
dotnet run --project AIResearch.Architecture.Host
```

The API starts on `http://localhost:5000`. Swagger UI is available at:

```
http://localhost:5000/swagger
```

## Quick Start

The Quick Start below walks through the minimal sequence required to generate and validate a Clean Architecture solution using the MCP API. It demonstrates the correct call order, required parameters, and expected responses. Follow the steps exactly as shown � architectural structure is inferred and enforced by the MCP tool, not manually defined.

### 1. Load PowerShell helpers (for AI agents)

```powershell
Invoke-RestMethod -Uri 'http://localhost:5000/v1.0/PowerShell/helpers' | Invoke-Expression

$rules      = Invoke-ArchGetRules
$roles      = Invoke-ArchGetComponentRoles
$result     = Invoke-ArchGenerate -SolutionName 'MyCompany.OrderService' `
                  -Features @(@{Name='Order'}) `
                  -Components @(@{ComponentRole='Entity'; Name='Order'})
Save-GeneratedFiles -Files $result.files
```

### 2. Get available component roles

```http
GET http://localhost:5000/v1.0/Architecture/component-roles
```

Use the `name` values from the response as `ComponentRole` values in generate requests.

### 3. Get the architecture descriptor

```http
GET http://localhost:5000/v1.0/Architecture/get-architecture
```

Returns the full architecture descriptor: layer names (`Domain`, `Application`, `Infrastructure`, `WebApi`), their descriptions, and allowed dependency directions. Use this to understand the structural constraints before generating any components.

### 4. Get architecture rules

```http
GET http://localhost:5000/v1.0/Architecture/rules
```

Returns all coded architecture rules with their IDs and descriptions (dependency constraints, interface requirements, type-kind enforcement, accessibility rules). Review these before generating or validating components to understand what the validator enforces.

### 5. Generate a component

```http
POST http://localhost:5000/v1.0/Architecture/generate
Content-Type: application/json

{
  "SolutionName": "MyCompany.OrderService",
  "Features": [{ "Name": "Order" }],
  "Components": [
    { "ComponentRole": "CommandHandler", "Name": "CreateOrderHandler", "Commands": ["CreateOrderCommand"] }
  ]
}
```

Response includes generated file paths and their source code:

```json
{
  "status": "ok",
  "files": {
    "MyCompany.OrderService.Order.Application/Commands/CreateOrderHandler.cs": "internal sealed class CreateOrderHandler ..."
  }
}
```

### 6. Generate a workspace (solution + projects)

```http
POST http://localhost:5000/v1.0/Architecture/generate-workspace
Content-Type: application/json

{
  "SolutionName": "MyCompany.OrderService",
  "ArchitectureStyle": "CleanArchitecture",
  "Features": [{ "Name": "Order", "ApplicationKind": "WebApi" }],
  "TargetFramework": "net10.0"
}
```

### 7. Validate existing source files

```http
POST http://localhost:5000/v1.0/Architecture/validate
Content-Type: application/json

{
  "Domain/Order.cs": "namespace MyCompany.OrderService.Domain { using MyCompany.OrderService.Infrastructure; public class Order {} }"
}
```

Returns a list of violations, e.g. `["Domain layer cannot depend on Infrastructure layer"]`.

## Using the Architecture MCP Tool with GitHub Copilot

This section explains how to use GitHub Copilot Chat together with the AIResearch.Architecture MCP tool to scaffold a complete .NET solution under strict architectural governance.

Copilot defines intent.  
The MCP tool enforces structure.

Copilot does not invent layers, file paths, or namespaces. It interacts with the MCP tool and applies the returned output exactly as generated.

### 1. Prepare the Workspace

Create a new workspace folder and add:

```
.github/prompts
```

Copy the **Architecture MCP Tool [architecture-mcp.prompt.md](https://github.com/antoine271167/AIResearch.Architecture/blob/main/tool/architecture-mcp.prompt.md) Instructions** file into that folder.

This prompt:

- Defines the correct API base URL and version (`v1.0`)
- Enforces the mandatory execution order
- Prevents invalid endpoints
- Requires authoritative component roles
- Enforces parameter consistency
- Prohibits manual file creation

It converts Copilot from a speculative code generator into a controlled MCP client.

### 2. Initialize Copilot

Open **Copilot Chat** in your IDE and start a session using the slash command that loads the prompt from `.github/prompts`.

During initialization Copilot:

1. Loads the PowerShell helpers  
2. Loads architecture rules  
3. Loads component roles  
4. Loads the architecture descriptor  

Once complete, Copilot confirms readiness and waits for a task.

At this point, the MCP tool is the architectural authority.

### 3. Provide a Task

Provide a structured instruction such as:

#### Task: Scaffold Order Microservice

With requirements:

- **Solution Name**: `IaResearch.OrderService`
- **Feature**: `Order`
- Components:
  - `Order` (Domain model)
  - `CreateOrderCommand`
  - `CreateOrderCommandHandler`
  - `IOrderRepository`
  - `OrderRepository`
- All code must be generated via the MCP tool
- No manual file creation
- No inferred paths
- Strict parameter consistency

Describe what should exist (or instruct the AI to figure out what should exist) � not where it belongs. The MCP tool determines placement.

You can find an example [example.prompt.md](https://github.com/antoine271167/AIResearch.Architecture/blob/main/tool/example.prompt.md) in the Tool folder.

### 4. Execution Flow

Copilot follows the enforced workflow.

#### Load Rules and Metadata

- `Invoke-ArchGetRules`
- `Invoke-ArchGetComponentRoles`
- `Invoke-ArchGetArchitecture`

Component role `name` values returned from the API are the only valid `ComponentRole` inputs.

#### Generate Components (First)

Copilot calls:

```powershell
Invoke-ArchGenerate `
  -SolutionName 'IaResearch.OrderService' `
  -Features @(@{Name='Order'}) `
  -Components @( ...)
```

If an error occurs or no files are returned, Copilot must correct the request and retry before proceeding.

Generated files are saved using:

```powershell
Save-GeneratedFiles -Files $result.files
```

#### Generate Workspace (Second)

After components are generated:

```powershell
Invoke-ArchGenerateWorkspace `
  -SolutionName 'IaResearch.OrderService' `
  -Features @(@{Name='Order'; ApplicationKind='WebApi'})
```

Files are then saved to disk.

### 5. Critical Rules

When using Copilot with the MCP tool:

- Always load PowerShell helpers first
- Generate components before generating the workspace
- Use identical SolutionName and Feature.Name values across calls
- Never invent ComponentRole values
- Never infer layer placement
- Apply MCP output exactly as returned

The MCP tool is the single source of truth for architectural structure.

### 6. Result

After successful execution, the solution contains:

- A .sln file
- Layered projects (Domain, Application, Infrastructure, WebApi)
- Correct namespaces and dependency directions
- Required interfaces for applicable roles
- Architecture rule compliance by construction
- No manual structural decisions are made.
- Architecture is enforced mechanically.

Using Copilot in this way transforms it from a free-form code generator into a governed architectural client. Behavior is generated by AI; structure is enforced by policy.

## Architecture Overview

The solution follows Clean Architecture with CQRS.

```
AIResearch.Architecture.Contracts
  - DTOs, request/response records, IMediator / IRequest / IRequestHandler interfaces

AIResearch.Architecture.Domain
  - IArchitectureLayersProvider    � layer definitions and dependency graph
  - IComponentRoleProvider         � catalogue of component roles and their metadata
  - IArchitectureRule (and impls)  � individual rules (dependency, type-kind, interface requirement, accessibility)

AIResearch.Architecture.Application
  - Commands
        GenerateComponentsCommand  � resolves layer, validates dependencies, generates code
        GenerateWorkspaceCommand   � scaffolds .sln and .csproj files
  - Queries
        GetArchitectureQuery       � returns the architecture descriptor
        GetRulesQuery              � returns all architecture rules
        GetComponentRolesQuery     � returns component role catalogue
        ValidateArchitectureQuery  � analyses source files and reports violations
  - Services
        CSharpCodeGeneratorService � builds C# source text via Roslyn and formats it
        CSharpCodeAnalyzerService  � parses source files with Roslyn to extract dependencies
        LayerInferenceService      � infers the target layer from a component role
        WorkspaceService           � generates solution file and csproj files

AIResearch.Architecture.Host
  - ArchitectureController  � REST endpoints
  - McpController           � serves the MCP manifest JSON
  - PowerShellController    � serves PowerShell helper functions
  - McpManifestGenerator    � reflects over controllers decorated with [McpAction] to build the manifest
```

All cross-layer communication flows through `IMediator`. No direct service-to-service calls cross project boundaries.

Examine the documents referrenced in [Additional Documentation] for detailed architectural design and runtime flow of key features.

## Configuration

The API has no required configuration beyond what ships in `appsettings.json`.

| Setting | Default | Description |
|---|---|---|
| Port | `5000` | Defined in `ApiConstants.Port` (source, not config) |
| `Logging:LogLevel:Default` | `Debug` | Standard ASP.NET Core log level |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Framework log level |

No connection strings, secrets, or external service credentials are required to run the application.

## Running Tests

```bash
cd AIResearch.Architecture/src
dotnet test
```

## Contributing

1. Fork or clone the repository.
2. Create a branch from `main`.
3. Follow the existing project layering � Domain has no dependencies on Application or Infrastructure; Application depends only on Domain and Contracts.
4. Add or extend architecture rules in `AIResearch.Architecture.Domain/Services/Rules`.
5. Add new component roles in `ComponentRoleProvider._allRoles`.
6. Open a pull request targeting `main`.

## License

MIT
