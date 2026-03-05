# MCP Manifest Endpoint — Architecture Flow

> High-level architecture document describing how the `/v1.0/Mcp/manifest` endpoint processes its request, flows through the application layers, and leverages architecture rules to produce the final JSON manifest.

---

## 1. Solution Structure Overview

The solution follows a **Clean Architecture** pattern with four projects, each mapping to a distinct architectural layer:

```mermaid
graph TB
    subgraph "Solution: AIResearch.Architecture"
        Host["AIResearch.Architecture.<b>Host</b><br/><i>WebApi Layer</i>"]
        App["AIResearch.Architecture.<b>Application</b><br/><i>Application Layer</i>"]
        Domain["AIResearch.Architecture.<b>Domain</b><br/><i>Domain Layer</i>"]
        Contracts["AIResearch.Architecture.<b>Contracts</b><br/><i>Shared Contracts</i>"]
    end

    Host --> App
    Host --> Contracts
    App --> Domain
    App --> Contracts
    Domain --> Contracts

    style Host fill:#4A90D9,color:#fff
    style App fill:#7BC67E,color:#fff
    style Domain fill:#F5A623,color:#fff
    style Contracts fill:#9B9B9B,color:#fff
```

| Project | Layer | Responsibility |
|---------|-------|----------------|
| `AIResearch.Architecture.Host` | WebApi | Controllers, middleware, MCP manifest generation, DI composition root |
| `AIResearch.Architecture.Application` | Application | Use cases, mediator, application services, architecture rules aggregation |
| `AIResearch.Architecture.Domain` | Domain | Architecture rules, layer definitions, component role metadata, type kind rules |
| `AIResearch.Architecture.Contracts` | Shared | DTOs, request/response models, mediator abstractions |

---

## 2. Manifest Endpoint — Request Flow

The manifest endpoint is exposed as `GET /v1.0/Mcp/manifest`. Its purpose is to generate a fully self-describing JSON document that an AI agent can consume to understand the available API, its rules, workflow, and invocation protocol.

### 2.1 Sequence Diagram

```mermaid
sequenceDiagram
    participant Client as AI Agent / Client
    participant MC as McpController
    participant MG as McpManifestGenerator
    participant BUR as BaseUrlResolver
    participant HC as HttpContext
    participant AC as ArchitectureController<br/>(Type Metadata)

    Client->>MC: GET /v1.0/Mcp/manifest
    MC->>MG: GenerateManifest(controllerTypes, name, description, version)

    Note over MG: Phase 1 — Resolve runtime URLs
    MG->>BUR: GetBaseUrl()
    BUR->>HC: Read Request.Scheme + Request.Host
    BUR-->>MG: baseUrl (e.g. "http://localhost:5000")
    MG->>BUR: GetPowerShellHelpersUrl()
    BUR-->>MG: powerShellHelpersUrl
    MG->>BUR: GetSwaggerJsonUrl()
    BUR-->>MG: swaggerUrl
    MG->>BUR: GetMcpManifestUrl()
    BUR-->>MG: mcpManifestUrl
    MG->>BUR: GetArchitectureBaseUrl()
    BUR-->>MG: architectureBaseUrl

    Note over MG: Phase 2 — Build static manifest structure
    MG->>MG: Build JSON: name, description, version,<br/>clientInstructions, workflow, endpoint map

    Note over MG: Phase 3 — Reflect on controller types
    MG->>MG: GetApiVersion(controllerType)
    loop For each controller type
        MG->>MG: GetControllerRoute(controllerType)
        loop For each public method
            MG->>MG: Check for [McpAction] attribute
            alt Has McpAction
                MG->>MG: GenerateAction(method, mcpAction, ...)
                MG->>MG: GenerateInputSchema(method)
                MG->>MG: GenerateOutputSchema(method)
                MG->>MG: GenerateExamples(method, ...)
            end
        end
    end

    MG-->>MC: JSON string (manifest)
    MC-->>Client: 200 OK — Content-Type: application/json
```

### 2.2 Step-by-step Walkthrough

1. **HTTP Entry** — The request arrives at `McpController.GetManifest()` via ASP.NET Core routing (`GET /v1.0/Mcp/manifest`).

2. **Delegation** — The controller delegates entirely to `IMcpManifestGenerator.GenerateManifest()`, passing:
   - The `ArchitectureController` type (as the source of discoverable actions)
   - Static metadata: MCP name, description, and version

3. **URL Resolution** — `McpManifestGenerator` uses `BaseUrlResolver` to compute all runtime-aware URLs (base URL, Swagger, PowerShell helpers, manifest, architecture endpoints) from the current `HttpContext`.

4. **Static Manifest Construction** — The generator builds the core JSON structure containing:
   - Identity fields (`name`, `description`, `version`)
   - Client instructions, workflow steps, and error-handling guidance
   - PowerShell setup and helper function catalog
   - Valid endpoint map

5. **Reflection-based Action Discovery** — The generator iterates over each controller type, reflecting on its public methods to find those decorated with `[McpAction]`. For each discovered action it builds:
   - **Input schema** — by inspecting method parameter types recursively
   - **Output schema** — by unwrapping `ActionResult<T>` / `Task<T>` return types
   - **Examples** — from `[McpExample]` attributes with PowerShell invocation snippets

6. **Response** — The JSON document is serialized and returned with `Content-Type: application/json`.

---

## 3. Component Architecture — Manifest Generation

```mermaid
classDiagram
    class McpController {
        -IMcpManifestGenerator manifestGenerator
        +GetManifest() IActionResult
    }

    class IMcpManifestGenerator {
        <<interface>>
        +GenerateManifest(Type[], string, string, string) string
    }

    class McpManifestGenerator {
        -BaseUrlResolver baseUrlResolver
        +GenerateManifest(Type[], string, string, string) string
        -GetControllerRoute(Type) string
        -GetApiVersion(Type) string
        -GenerateAction(MethodInfo, McpActionAttribute, ...) JsonObject
        -GenerateInputSchema(MethodInfo) JsonObject
        -GenerateOutputSchema(MethodInfo) JsonObject
        -GenerateTypeSchema(Type) JsonObject
        -GenerateExamples(MethodInfo, string, string) JsonArray
    }

    class BaseUrlResolver {
        -IHttpContextAccessor httpContextAccessor
        +GetBaseUrl() string
        +GetVersionedUrl(string) string
        +GetArchitectureBaseUrl() string
        +GetPowerShellHelpersUrl() string
        +GetMcpManifestUrl() string
        +GetSwaggerJsonUrl() string
    }

    class McpActionAttribute {
        +Name : string?
        +Description : string
    }

    class McpExampleAttribute {
        +Description : string
        +InputJson : string?
        +OutputJson : string?
        +ErrorMessage : string?
    }

    McpController --> IMcpManifestGenerator
    IMcpManifestGenerator <|.. McpManifestGenerator
    McpManifestGenerator --> BaseUrlResolver
    McpManifestGenerator ..> McpActionAttribute : reads via reflection
    McpManifestGenerator ..> McpExampleAttribute : reads via reflection
```

---

## 4. How Architecture Rules Shape the Manifest

The manifest does not execute the architecture rules at request time. Instead, the rules are **indirectly embedded** into the manifest through three mechanisms:

### 4.1 Workflow Step References

The `clientInstructions.WORKFLOW` array in the manifest directs consuming AI agents to call architecture-aware endpoints (rules, component roles, architecture definition) **before** generating any code. This ensures the rules are loaded and consulted at the agent's runtime.

```mermaid
flowchart LR
    subgraph "Manifest Workflow (embedded in JSON)"
        W1["1. Load PowerShell Helpers"]
        W2["2. GET /rules"]
        W3["3. GET /component-roles"]
        W4["4. GET /get-architecture"]
        W5["5. Analyze & Design"]
        W6["6. POST /generate"]
        W7["7. Save component files"]
        W8["8. POST /generate-workspace"]
        W9["9. Save workspace files"]
    end

    W1 --> W2 --> W3 --> W4 --> W5 --> W6 --> W7 --> W8 --> W9

    style W2 fill:#F5A623,color:#fff
    style W3 fill:#F5A623,color:#fff
    style W4 fill:#F5A623,color:#fff
```

Steps 2–4 (highlighted) serve architecture rule data to the agent via separate API endpoints backed by the domain layer.

### 4.2 Action Schema Introspection

When the manifest generator reflects on `ArchitectureController`, it discovers all endpoints that expose architecture data:

| Action | HTTP Method | Description | Domain Service Involved |
|--------|-------------|-------------|-------------------------|
| `get-architecture` | GET | Returns layer definitions + style | `IArchitectureLayersProvider` |
| `rules` | GET | Returns all architecture rules | `IArchitectureRulesProvider` ? `IArchitectureRule[]` |
| `component-roles` | GET | Returns all valid component roles | `IComponentRoleProvider` |
| `validate` | POST | Validates code against rules | `ILayerDependencyRule[]`, `ICodeFeatureRule[]` |
| `generate` | POST | Generates code components | `ILayerInferenceRule[]`, `ITypeKindRule[]`, `ICodeFeatureRule[]` |
| `generate-workspace` | POST | Generates solution structure | `IArchitectureLayersProvider` |

### 4.3 Endpoint URL Assembly

The manifest constructs a `validEndpoints` section using `BaseUrlResolver`, providing the AI agent with exact URLs for every architecture-related endpoint:

```json
{
  "validEndpoints": {
    "get-architecture": "http://localhost:5000/v1.0/Architecture/get-architecture",
    "rules": "http://localhost:5000/v1.0/Architecture/rules",
    "component-roles": "http://localhost:5000/v1.0/Architecture/component-roles",
    "validate": "http://localhost:5000/v1.0/Architecture/validate",
    "generate": "http://localhost:5000/v1.0/Architecture/generate",
    "generate-workspace": "http://localhost:5000/v1.0/Architecture/generate-workspace"
  }
}
```

---

## 5. Manifest JSON Structure — Logical Map

The following diagram shows the top-level structure of the generated manifest JSON and how each section contributes to the AI agent's understanding:

```mermaid
graph TD
    Manifest["MCP Manifest JSON"]

    Manifest --> Identity["Identity<br/><code>name, description, version</code>"]
    Manifest --> Protocol["Protocol<br/><code>baseUrl, apiVersion, protocol, swaggerUrl</code>"]
    Manifest --> CI["clientInstructions"]
    Manifest --> PS["POWERSHELL_SETUP"]
    Manifest --> VE["validEndpoints"]
    Manifest --> Actions["actions[]"]

    CI --> InvType["invocationType: powershell"]
    CI --> DoNot["DO_NOT rules"]
    CI --> StepRules["STEP_EXECUTION_RULES"]
    CI --> Workflow["WORKFLOW (9 ordered steps)"]
    CI --> ErrorH["ERROR_HANDLING"]

    PS --> LoadCmd["MANDATORY_FIRST_STEP"]
    PS --> Funcs["availableFunctions"]
    PS --> Example["exampleWorkflow"]

    Actions --> A1["Action: get-architecture"]
    Actions --> A2["Action: rules"]
    Actions --> A3["Action: component-roles"]
    Actions --> A4["Action: validate"]
    Actions --> A5["Action: generate"]
    Actions --> A6["Action: generate-workspace"]

    A1 --> Schema1["input/output schemas"]
    A1 --> Ex1["examples[]"]

    style Manifest fill:#4A90D9,color:#fff
    style CI fill:#7BC67E,color:#fff
    style Actions fill:#F5A623,color:#fff
    style PS fill:#9B59B6,color:#fff
```

---

## 6. Reflection Engine — Schema Generation

The `McpManifestGenerator` uses .NET reflection to automatically produce JSON Schema descriptions for every discovered action's inputs and outputs.

```mermaid
flowchart TD
    Method["Controller Method<br/><i>e.g. Generate(GenerateComponentsRequest)</i>"]

    Method --> Input["GenerateInputSchema(method)"]
    Method --> Output["GenerateOutputSchema(method)"]

    Input --> Params["Iterate method parameters"]
    Params --> TypeSchema["GenerateTypeSchema(paramType)"]

    Output --> Unwrap["Unwrap Task&lt;T&gt; / ActionResult&lt;T&gt;"]
    Unwrap --> TypeSchema

    TypeSchema --> Prim{"Primitive?"}
    Prim -->|string, int, bool, ...| PrimNode["{ type: 'string' }"]
    Prim -->|No| Coll{"Collection?"}
    Coll -->|List, IEnumerable, ...| ArrayNode["{ type: 'array', items: ... }"]
    Coll -->|No| Dict{"Dictionary?"}
    Dict -->|Yes| DictNode["{ type: 'object', additionalProperties: ... }"]
    Dict -->|No| Complex["Complex Object"]
    Complex --> Props["Iterate public properties"]
    Props --> TypeSchema
```

### Supported type mappings:

| .NET Type | JSON Schema Type |
|-----------|-----------------|
| `string` | `string` |
| `int`, `long` | `integer` |
| `bool` | `boolean` |
| `decimal`, `double`, `float` | `number` |
| `List<T>`, `IEnumerable<T>`, etc. | `array` with `items` |
| `Dictionary<K,V>` | `object` with `additionalProperties` |
| Complex object | `object` with `properties` (recursive) |

---

## 7. Dependency Injection Wiring

```mermaid
flowchart TD
    subgraph "Program.cs — Composition Root"
        P["ConfigureServices()"]
    end

    subgraph "Domain Layer Registration"
        P -->|"AddDomainServices()"| DL["Singletons:<br/>IArchitectureLayersProvider<br/>IComponentRoleProvider<br/>IArchitectureRule (11 rules)<br/>ILayerInferenceRule (3)<br/>ITypeKindRule (3)<br/>ICodeFeatureRule (2)"]
    end

    subgraph "Application Layer Registration"
        P -->|"AddApplicationServices()"| AL["Scoped:<br/>IMediator ? Mediator<br/>IArchitectureRulesProvider<br/>+ 6 Request Handlers<br/>+ Application Services"]
    end

    subgraph "Host Layer Registration"
        P --> HL["Scoped:<br/>IMcpManifestGenerator ? McpManifestGenerator<br/>BaseUrlResolver<br/>IHttpContextAccessor"]
    end

    style P fill:#4A90D9,color:#fff
    style DL fill:#F5A623,color:#fff
    style AL fill:#7BC67E,color:#fff
    style HL fill:#4A90D9,color:#fff
```

---

## 8. Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Reflection-based action discovery** | Actions are auto-discovered from `[McpAction]` attributes — no manual manifest maintenance needed. Adding a new endpoint with the attribute automatically includes it in the manifest. |
| **Runtime URL resolution** | `BaseUrlResolver` reads `HttpContext` to produce environment-correct URLs, making the manifest portable across localhost, staging, and production. |
| **Rules as indirect content** | The manifest does not serialize all rules inline. Instead, it directs the AI agent to fetch rules at runtime via dedicated endpoints, keeping the manifest lightweight and rules always up-to-date. |
| **Controller type passed as parameter** | The manifest generator is decoupled from any specific controller. It accepts `Type[]`, making it reusable if additional controllers need to be exposed in the future. |
| **Clean Architecture layering enforced by the system it describes** | The solution itself follows the same Clean Architecture rules that its domain model defines — the architecture is self-documenting. |
