# Generate Flow Architecture

## 1. Overview

**AIResearch.Architecture** is a .NET 10 Web API that acts as an **architecture-aware code generation engine**. It enforces Clean Architecture rules at code-generation time � callers describe *what* they want (component role, name, feature) and the system decides *where* it goes, *how* it's shaped, and whether the request is even allowed.

The most important endpoint is **`POST /v1/Architecture/generate`**, which is, together with the supporting structures behind it, the focus of this document, the **`'generate' flow`**.

```mermaid
graph LR
    Client([API Consumer / MCP Agent])
    API[Host Layer<br/>ASP.NET Web API]
    APP[Application Layer<br/>Commands, Services]
    DOM[Domain Layer<br/>Rules, Models, Providers]

    Client -->|HTTP POST /generate| API
    API -->|Mediator| APP
    APP -->|Rules & Providers| DOM
```

---

## 2. Solution Structure

The solution is organised into four projects that mirror a classic Clean Architecture layering:

```mermaid
graph TB
    subgraph "AIResearch.Architecture.Host"
        Controllers
        Middleware
        Program[Program.cs � Composition Root]
    end

    subgraph "AIResearch.Architecture.Application"
        Commands[Commands / Queries � CQRS]
        AppServices[Application Services]
        MediatorImpl[Mediator]
    end

    subgraph "AIResearch.Architecture.Contracts"
        Requests[Request / Response DTOs]
        MediatorContracts[IMediator / IRequest / IRequestHandler]
    end

    subgraph "AIResearch.Architecture.Domain"
        DomainModels[Models � Layers, Roles, Types]
        Rules[Architecture Rules]
        Providers[Providers � Layers, Roles]
    end

    Controllers --> MediatorContracts
    Controllers --> Requests
    MediatorImpl --> Commands
    Commands --> AppServices
    AppServices --> Rules
    AppServices --> Providers
    AppServices --> DomainModels
```

| Project | Responsibility |
|---|---|
| **Host** | ASP.NET entry point, controllers, middleware (logging, global exception handling), Swagger, API versioning, MCP manifest. |
| **Application** | CQRS command/query handlers, orchestration services (inference, validation, code generation), the `Mediator` implementation. |
| **Contracts** | Shared DTOs (`GenerateComponentsRequest`, `GenerateComponentsResponse`, etc.) and mediator abstractions (`IMediator`, `IRequest<T>`, `IRequestHandler<TReq,TRes>`). |
| **Domain** | Pure domain logic: architecture layer definitions, component role metadata, and all architecture rule implementations. Zero external dependencies. |

---

## 3. The Generate Endpoint � Request Flow

### 3.1 Entry Point

```
POST /v{version}/Architecture/generate
```

The `ArchitectureController.Generate` method receives a `GenerateComponentsRequest` containing:

| Field | Purpose |
|---|---|
| `SolutionName` | Root namespace prefix and solution file name (e.g. `IaResearch.OrderService`). |
| `Features` | Bounded contexts to scaffold. Each has a `Name` and optional `ApplicationKind`. |
| `Components` | Individual C# types to generate. Each has a required `ComponentRole` and `Name`, plus optional `Layer`, `Dependencies`, `Commands`, `Comments`, and `ImplementsInterfaces`. |

### 3.2 End-to-End Sequence

```mermaid
sequenceDiagram
    autonumber
    participant C as Client
    participant AC as ArchitectureController
    participant M as Mediator
    participant H as GenerateComponentsCommandHandler
    participant TMR as TypeMetadataResolverService
    participant LIS as LayerInferenceService
    participant DVS as DependencyValidationService
    participant CGS as CSharpCodeGeneratorService
    participant FPS as FilePathService

    C->>AC: POST /generate (GenerateComponentsRequest)
    AC->>M: SendAsync(GenerateComponentsCommand)
    M->>H: HandleAsync(command)

    Note over H: Validate SolutionName is present

    loop For each Feature � Component
        H->>TMR: RequiresInterface(role)
        TMR-->>H: bool
        Note over H: If required & missing ? return "invalid"

        H->>TMR: ResolveAccessibility(role)
        TMR-->>H: "internal" | "public"
        Note over H: If interface-required role is not internal ? return "invalid"

        alt Layer NOT provided
            H->>LIS: InferRequest(LayerInferenceRequest)
            LIS-->>H: Ok / Ambiguous / Invalid
        else Layer provided
            H->>H: Look up layer in ArchitectureLayersProvider
            H->>DVS: Validate(dependencies)
            DVS-->>H: violations[]
        end

        Note over H: If any error ? return early

        H->>CGS: GenerateCode(CodeGenerationModel)
        CGS-->>H: C# source string
        H->>FPS: GetSourceFilePath(...)
        FPS-->>H: relative file path
    end

    H-->>M: GenerateComponentsResult
    M-->>AC: result
    AC-->>C: 200 OK / 400 Bad Request
```

---

## 4. Processing Steps in Detail

### Step 1 � Controller & Mediator Dispatch

The controller wraps the incoming DTO into a `GenerateComponentsCommand` and sends it through the custom `Mediator`. The mediator resolves the matching `IRequestHandler<GenerateComponentsCommand, GenerateComponentsResult>` from the DI container via reflection and invokes `HandleAsync`.

### Step 2 � Request Validation

`GenerateComponentsCommandHandler` first checks that `SolutionName` is non-empty. If missing, an `"invalid"` result is returned immediately.

### Step 3 � Per-Component Processing Loop

For every combination of **Feature � Component**, the handler runs the following pipeline:

```mermaid
flowchart TD
    Start([Component received]) --> IV{Interface<br/>required?}
    IV -->|Yes & missing| FAIL_IF[Return invalid<br/><i>CODE-001</i>]
    IV -->|No / present| AV{Accessibility<br/>valid?}
    AV -->|Interface-required<br/>but not internal| FAIL_ACC[Return invalid<br/><i>CODE-002</i>]
    AV -->|OK| LP{Layer<br/>provided?}

    LP -->|No| INFER[LayerInferenceService<br/>.InferRequest]
    LP -->|Yes| LOOKUP[Look up layer in<br/>ArchitectureLayersProvider]

    INFER --> INF_OK{Inference<br/>result?}
    INF_OK -->|Ok| BUILD
    INF_OK -->|Ambiguous| FAIL_AMB[Return ambiguous<br/>with options]
    INF_OK -->|Invalid| FAIL_INV[Return invalid<br/>unknown role]

    LOOKUP --> DEP_V[DependencyValidationService<br/>.Validate]
    DEP_V --> DEP_OK{Violations?}
    DEP_OK -->|Yes| FAIL_DEP[Return invalid<br/>with violations]
    DEP_OK -->|No| BUILD

    BUILD[Build CodeGenerationModel] --> GEN[CSharpCodeGeneratorService<br/>.GenerateCode]
    GEN --> PATH[FilePathService<br/>.GetSourceFilePath]
    PATH --> COLLECT([Add FileContent<br/>to results])
```

#### 3a � Interface Requirement Validation (CODE-001)

The `TypeMetadataResolverService` consults the domain's `InterfaceRequiredRule` (rule **CODE-001**) which delegates to `ComponentRoleProvider`. If the role's metadata has `RequiresInterface = true` and the request has no `ImplementsInterfaces`, the request is rejected with a suggested interface name (`I{ComponentName}`).

#### 3b � Accessibility Validation (CODE-002)

The `InternalRequiredRule` (rule **CODE-002**) enforces that non-interface types requiring an interface implementation must be `internal`. If the role requires an interface but is not an interface itself, and the resolved accessibility is not `internal`, the request is rejected.

#### 3c � Layer Resolution

**Path A � Layer Inference (no explicit layer)**

```mermaid
flowchart LR
    REQ[ComponentRole] --> LIRS[LayerInferenceRulesService]
    LIRS --> DET{Deterministic<br/>match?}
    DET -->|Yes| OK[Return inferred layer<br/>+ allowed dependencies]
    DET -->|No| AMB{Ambiguous<br/>candidates?}
    AMB -->|Yes| AMBR[Return candidates<br/>with suggestions]
    AMB -->|No| INV[Return invalid]
```

The `LayerInferenceService` delegates to `LayerInferenceRulesService` which iterates registered `ILayerInferenceRule` implementations:

| Rule | ID | Layer |
|---|---|---|
| `InferenceApplicationRule` | INFE-001 | Application |
| `InferenceDomainRule` | INFE-002 | Domain |
| `InferenceInfrastructureRule` | INFE-003 | Infrastructure |

Each rule extends `LayerInferenceRuleBase`, which consults `ComponentRoleProvider` to check if the given role has metadata pinning it to that layer. A deterministic match means exactly one rule claims the role. If none claim it deterministically but some could apply, the result is ambiguous with candidate options.

**Path B � Explicit Layer Validation**

When a layer is explicitly provided, the handler verifies it exists in `ArchitectureLayersProvider`, then validates the component's declared dependencies against the architecture's dependency rules using `DependencyValidationService`.

#### 3d � Dependency Validation

```mermaid
flowchart LR
    DEP[LayerDependency] --> DVS[DependencyValidationService]
    DVS --> R1[ARCH-001: Application ? Domain only]
    DVS --> R2[ARCH-002: Domain ? Infrastructure forbidden]
    DVS --> R3[ARCH-003: Infrastructure ? Application + Domain only]
    R1 --> V{Violated?}
    R2 --> V
    R3 --> V
    V -->|Yes| REJECT[Return violations]
    V -->|No| PASS[Continue]
```

The `DependencyValidationService` collects all registered `ILayerDependencyRule` instances and checks each declared dependency pair:

| Rule | ID | Enforces |
|---|---|---|
| `ApplicationDomainDependencyRule` | ARCH-001 | Application may only depend on Domain |
| `DomainInfrastructureDependencyRule` | ARCH-002 | Domain must not depend on Infrastructure |
| `InfrastructureDependencyRule` | ARCH-003 | Infrastructure may depend on Application and Domain only |

#### 3e � Code Generation

Once all validations pass, a `CodeGenerationModel` is built and handed to `CSharpCodeGeneratorService`, which:

1. **Builds the namespace** via `NamespaceService` ? `{SolutionName}.{Feature}.{Layer}`.
2. **Generates using statements** for declared dependencies.
3. **Resolves the C# type kind** via `TypeMetadataResolverService` ? consults `ITypeKindRule` implementations to map component role ? `Class`, `Record`, `Interface`, `Struct`, or `RecordStruct`.
4. **Resolves accessibility** (`public` / `internal`).
5. **Generates the type declaration** with inheritance clauses, method stubs, and comments.
6. **Formats the code** using Roslyn's `Formatter`.

#### 3f � File Path Resolution

`FilePathService` computes the output path: `src/{SolutionName}.{Feature}.{Layer}/{ComponentName}.cs`.

### Step 4 � Response Assembly

All generated `FileContent` items are collected and returned as a dictionary of `path ? code` in the response with `status: "ok"`. Any early exit produces an error response with status `"invalid"` or `"ambiguous"`.

---

## 5. Architecture Rules � Complete Taxonomy

```mermaid
classDiagram
    class IArchitectureRule {
        <<interface>>
        +Id : string
        +Description : string
    }

    class ILayerDependencyRule {
        <<interface>>
        +IsViolated(layerName, dependsOn) : bool
    }

    class ILayerInferenceRule {
        <<interface>>
        +LayerName : string
        +AppliesToComponentRole(role) : bool
        +CouldApplyToComponentRole(role) : bool
        +GetSuggestedComponentRoles(role) : string[]
    }

    class ITypeKindRule {
        <<interface>>
        +AppliesToComponentRole(role) : bool
        +GetTypeKind(role) : CSharpTypeKind
    }

    class ICodeFeatureRule {
        <<interface>>
        +IsRequired(role) : bool
    }

    IArchitectureRule <|-- ILayerDependencyRule
    IArchitectureRule <|-- ILayerInferenceRule
    IArchitectureRule <|-- ITypeKindRule
    IArchitectureRule <|-- ICodeFeatureRule

    ILayerDependencyRule <|.. ApplicationDomainDependencyRule : ARCH-001
    ILayerDependencyRule <|.. DomainInfrastructureDependencyRule : ARCH-002
    ILayerDependencyRule <|.. InfrastructureDependencyRule : ARCH-003

    ILayerInferenceRule <|.. InferenceApplicationRule : INFE-001
    ILayerInferenceRule <|.. InferenceDomainRule : INFE-002
    ILayerInferenceRule <|.. InferenceInfrastructureRule : INFE-003

    ITypeKindRule <|.. InterfaceTypeKindRule
    ITypeKindRule <|.. RecordTypeKindRule
    ITypeKindRule <|.. ClassTypeKindRule

    ICodeFeatureRule <|.. InterfaceRequiredRule : CODE-001
    ICodeFeatureRule <|.. InternalRequiredRule : CODE-002
```

### Rule Summary

| Category | ID | Rule | Description |
|---|---|---|---|
| **Dependency** | ARCH-001 | `ApplicationDomainDependencyRule` | Application layer may only depend on Domain. |
| **Dependency** | ARCH-002 | `DomainInfrastructureDependencyRule` | Domain layer must not depend on Infrastructure. |
| **Dependency** | ARCH-003 | `InfrastructureDependencyRule` | Infrastructure may depend on Application and Domain only. |
| **Inference** | INFE-001 | `InferenceApplicationRule` | Maps roles like `Command`, `CommandHandler`, `Query`, `ApplicationService` ? Application layer. |
| **Inference** | INFE-002 | `InferenceDomainRule` | Maps roles like `Entity`, `ValueObject`, `Aggregate`, `DomainService` ? Domain layer. |
| **Inference** | INFE-003 | `InferenceInfrastructureRule` | Maps roles like `Repository`, `Gateway`, `DbContext` ? Infrastructure layer. |
| **Type Kind** | � | `InterfaceTypeKindRule` | Resolves interface roles ? `CSharpTypeKind.Interface`. |
| **Type Kind** | � | `RecordTypeKindRule` | Resolves record roles (Command, Query, ValueObject, etc.) ? `CSharpTypeKind.Record`. |
| **Type Kind** | � | `ClassTypeKindRule` | Default fallback ? `CSharpTypeKind.Class`. |
| **Code Feature** | CODE-001 | `InterfaceRequiredRule` | Roles like `Repository`, `CommandHandler`, `ApplicationService` must have an interface. |
| **Code Feature** | CODE-002 | `InternalRequiredRule` | Non-interface types that require an interface must be `internal` (interface is `public`). |

---

## 6. The Architecture Layers

The `ArchitectureLayersProvider` defines four layers with strict dependency rules:

```mermaid
graph BT
    Domain["Domain<br/><i>Entities, Value Objects,<br/>Domain Services, Events</i>"]
    Application["Application<br/><i>Commands, Queries, Handlers,<br/>DTOs, Interfaces</i>"]
    Infrastructure["Infrastructure<br/><i>Repositories, DbContext,<br/>External Services, Messaging</i>"]
    WebApi["WebApi<br/><i>Controllers, Program.cs,<br/>DI Configuration, Middleware</i>"]

    Application -->|depends on| Domain
    Infrastructure -->|depends on| Application
    Infrastructure -->|depends on| Domain
    WebApi -->|depends on| Infrastructure
    WebApi -->|depends on| Application
    WebApi -->|depends on| Domain
```

| Layer | Allowed Dependencies |
|---|---|
| **Domain** | *(none)* |
| **Application** | Domain |
| **Infrastructure** | Application, Domain |
| **WebApi** | Infrastructure, Application, Domain |

---

## 7. Component Role System

The `ComponentRoleProvider` is the single source of truth for all supported component roles. Each role defines:

- **Name** � The canonical role identifier (e.g. `CommandHandler`).
- **TypeKind** � The C# type to generate (`Class`, `Record`, `Interface`, `Struct`, `RecordStruct`).
- **Layer** � The owning architecture layer.
- **RequiresInterface** � Whether an interface implementation is mandatory.
- **AlternativeNames** � Aliases that resolve to this role.

The role metadata drives multiple subsystems simultaneously: layer inference, type kind resolution, interface requirement checks, and accessibility decisions.

---

## 8. Key Application Services

| Service | Responsibility |
|---|---|
| `LayerInferenceService` | Orchestrates layer inference: tries deterministic match first, then falls back to ambiguous candidates. |
| `LayerInferenceRulesService` | Iterates `ILayerInferenceRule` instances to find deterministic or ambiguous layer matches for a role. |
| `DependencyValidationService` | Evaluates all `ILayerDependencyRule` instances against declared dependencies. |
| `TypeMetadataResolverService` | Resolves type kind, accessibility, and interface requirements from `ITypeKindRule` and `ICodeFeatureRule` instances. |
| `CSharpCodeGeneratorService` | Generates formatted C# source code using Roslyn, applying namespace, type declaration, and method stubs. |
| `NamespaceService` | Builds namespaces following the pattern `{SolutionName}.{Feature}.{Layer}`. |
| `FilePathService` | Computes output file paths: `src/{Project}/{ComponentName}.cs`. |
| `LayerDependencyService` | Resolves allowed dependencies for a given layer from `ArchitectureLayersProvider`. |

---

## 9. Cross-Cutting Concerns

- **Mediator Pattern** � All controller actions dispatch through `IMediator`, decoupling HTTP concerns from business logic.
- **Global Exception Handling** � `GlobalExceptionHandler` catches unhandled exceptions and returns RFC 7807 Problem Details.
- **Request Logging** � `RequestLoggingMiddleware` logs full request/response bodies for diagnostics.
- **API Versioning** � URL-segment versioning (`/v1/...`) via `Asp.Versioning`.
- **MCP Integration** � Custom `[McpAction]` and `[McpExample]` attributes enable discovery by AI agents through a generated MCP manifest.
