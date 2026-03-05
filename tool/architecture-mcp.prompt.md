# Architecture MCP Tool Instructions

You are working with a .NET solution that uses the IaResearch Architecture MCP (Model Context Protocol) tool.

---

## 🚨 MANDATORY: Load PowerShell Helpers First

**Before making ANY API call, you MUST execute this command:**
```powershell
Invoke-RestMethod -Uri 'http://localhost:5000/v1.0/PowerShell/helpers' | Invoke-Expression
```

This loads helper functions that handle JSON serialization correctly. **Do NOT write your own PowerShell HTTP calls - they will fail due to encoding issues.**

---

## API Reference

### Base URL & Version
- **Base URL**: `http://localhost:5000`
- **API Version**: `v1.0` (NOT `v1` - the `.0` is required)
- **Content-Type**: `application/json`

### Available Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/v1.0/PowerShell/helpers` | GET | **LOAD FIRST** - Get PowerShell helper functions |
| `/v1.0/Mcp/manifest` | GET | Get tool manifest and documentation |
| `/v1.0/Architecture/get-architecture` | GET | Get architecture layers and styles |
| `/v1.0/Architecture/rules` | GET | Get architecture rules and conventions |
| `/v1.0/Architecture/component-roles` | GET | Get all valid component roles — **`name` values are the ONLY valid `ComponentRole` inputs** |
| `/v1.0/Architecture/generate` | POST | Generate component files |
| `/v1.0/Architecture/generate-workspace` | POST | Generate solution and project structure |
| `/v1.0/Architecture/validate` | POST | Validate code against architecture rules |

### ❌ INVALID - These Do NOT Exist
- `/v1.0/Architecture/powershell-helpers` - MOVED to `/v1.0/PowerShell/helpers`
- `/v1.0/Architecture/mcp-manifest` - MOVED to `/v1.0/Mcp/manifest`
- `/v1.0/Architecture/action` - NO generic action endpoint
- `/v1/Architecture/*` - Wrong version format
- Any endpoint with wrapped body `{"action":"...", "request":{...}}`

---

## 🚦 MANDATORY WORKFLOW - FOLLOW THIS EXACT ORDER

**You MUST follow this workflow exactly. Do NOT skip steps. Do NOT reorder steps.**

| Step | Action | Command |
|------|--------|---------|
| 1 | **Load helpers** | `Invoke-RestMethod -Uri 'http://localhost:5000/v1.0/PowerShell/helpers' \| Invoke-Expression` |
| 2 | **Load architecture rules** | `$rules = Invoke-ArchGetRules` |
| 3 | **Load component roles** | `$componentRoles = Invoke-ArchGetComponentRoles` — the `name` values are the **only** valid `ComponentRole` inputs |
| 4 | **Load architecture** | `$architecture = Invoke-ArchGetArchitecture` |
| 5 | **Ask for user input** | This is where the user must provide specifications about the solution that should be generated. |
| 6 | **Analyze & Design** | Based on rules, component roles, and architecture, determine which components are needed for the user's requirements |
| 7 | **Generate components** | `$result = Invoke-ArchGenerate -SolutionName '<name>' -Feature @{Name='<feature>'} -Components @(...)` |
| ⛔ | **STOP — verify step 7** | Check `$result.files`. If it is `$null` or an HTTP error was returned, **do NOT proceed**. Read the error body, fix the component definition (e.g. correct `ComponentRole` to a valid `name` from step 3 or add `Layer`), and **retry step 6**. |
| 8 | **Save component files** | `Save-GeneratedFiles -Files $result.files` |
| 9 | **Generate workspace** | `$workspace = Invoke-ArchGenerateWorkspace -SolutionName '<name>' -Features @(@{Name='<feature>';ApplicationKind='WebApi'})` |
| ⛔ | **STOP — verify step 9** | Check `$workspace.files`. If it is `$null` or an HTTP error was returned, **do NOT proceed**. Read the error body, fix the request, and **retry step 8**. |
| 10 | **Save workspace files** | `Save-GeneratedFiles -Files $workspace.files` |

### ⚠️ WHY THIS ORDER MATTERS
- **Components MUST be generated BEFORE workspace** - The workspace generation uses the component information
- **Rules and architecture MUST be loaded first** - They define what components are valid and how they should be structured
- **Component roles MUST be loaded before designing** - The `name` values from step 3 are the only valid `ComponentRole` inputs; do NOT invent role names
- **Step 5 is YOUR job** - Analyze the user's requirements against the architecture rules and component roles to determine the correct components

### ❌ WRONG ORDER (Will fail)
```
Generate workspace → Generate components  # WRONG! Workspace won't know about components
```

### ✅ CORRECT ORDER
```
Load rules → Load component roles → Load architecture → Design components → Generate components → Generate workspace
```

---

## Component Structure

When calling `Invoke-ArchGenerate`, each component in the `Components` array must have the following structure:

```powershell
@{
    ComponentRole = '<role>'  # REQUIRED - e.g., "DomainModel", "Command", "CommandHandler", "Service", "Repository"
    Name = '<name>'           # REQUIRED - e.g., "Order", "CreateOrderCommand"
    Layer = '<layer>'         # OPTIONAL - e.g., "Domain", "Application" (can be auto-inferred from ComponentRole)
    Commands = @(...)         # OPTIONAL - array of command class names
    Comments = '<comments>'   # OPTIONAL - hints or TODOs
    Dependencies = @(...)     # OPTIONAL - interfaces/services to inject
    ImplementsInterfaces = @(...)  # OPTIONAL - interfaces this component implements or inherits from
}
```

### ⚠️ CRITICAL: Use `ComponentRole`, NOT `Role`
The property name is **`ComponentRole`**, not `Role`. Using `Role` will cause the request to fail.

### 🚨 CRITICAL: `ComponentRole` values MUST come from the component-roles endpoint
The `ComponentRole` value **MUST** be a `name` value returned by `Invoke-ArchGetComponentRoles` (step 3). **Do NOT invent, guess, or reuse role names from examples** — always use the authoritative list from the endpoint.

```powershell
# Inspect available roles before designing components
$componentRoles = Invoke-ArchGetComponentRoles
$componentRoles.componentRoles | Select-Object name, layer, typeKind | Format-Table
```

### Example Component Definitions
```powershell
$components = @(
    @{ComponentRole='DomainModel'; Name='Order'}
    @{ComponentRole='Command'; Name='CreateOrderCommand'}
    @{ComponentRole='CommandHandler'; Name='CreateOrderCommandHandler'; Dependencies=@('IOrderRepository')}
    @{ComponentRole='RepositoryInterface'; Name='IOrderRepository'}
    @{ComponentRole='Repository'; Name='OrderRepository'; ImplementsInterfaces=@('IOrderRepository')}
)
```

### Example with Interface Implementation
When a component needs to implement one or more interfaces, use the `ImplementsInterfaces` property:
```powershell
# A repository that implements its interface
@{ComponentRole='Repository'; Name='OrderRepository'; ImplementsInterfaces=@('IOrderRepository')}

# A service implementing multiple interfaces
@{ComponentRole='Service'; Name='NotificationService'; ImplementsInterfaces=@('INotificationService', 'IDisposable')}

# An interface inheriting from another interface
@{ComponentRole='RepositoryInterface'; Name='IOrderRepository'; ImplementsInterfaces=@('IRepository')}
```

### ⚠️ IMPORTANT: RequiresInterface Rule
When loading the architecture rules via `Invoke-ArchGetRules`, pay attention to the **CODE-001** rule (InterfaceRequirementRule).
This rule indicates which component roles **require an interface implementation**. Cross-check with `$componentRoles.componentRoles` (step 3) — roles where `requiresInterface` is `true` need a corresponding interface generated. The following roles typically require interfaces:
- `Repository` → should implement `I{Name}Repository` using role `RepositoryInterface`
- `Gateway` → should implement `I{Name}Gateway` using role `GatewayInterface`
- `ApplicationService` → should implement `I{Name}Service` using role `ApplicationServiceInterface`
- `DomainService` → should implement `I{Name}Service` using role `DomainServiceInterface`
- `InfrastructureService` → should implement `I{Name}Service` using role `InfrastructureServiceInterface`
- `CommandHandler` → should implement `I{Name}` using role `CommandHandlerInterface`
- `QueryHandler` → should implement `I{Name}` using role `QueryHandlerInterface`
- `ApplicationEventHandler` → should implement `I{Name}` using role `ApplicationEventHandlerInterface`

**When generating components for these roles, you SHOULD also generate the corresponding interface and specify `ImplementsInterfaces`.**

---

| Function | Purpose |
|----------|---------|
| `Invoke-ArchGetArchitecture` | Get architecture definition |
| `Invoke-ArchGetRules` | Get architecture rules |
| `Invoke-ArchGetComponentRoles` | Get all valid component roles — use `name` values as `ComponentRole` inputs |
| `Invoke-ArchGenerateWorkspace` | Generate solution structure (returns files with `code` property) |
| `Invoke-ArchGenerate` | Generate components (returns files with `code` property) |
| `Invoke-ArchValidate` | Validate source files |
| `Read-SourceFile` | Read a single file safely |
| `Read-SourceFiles` | Read multiple files to hashtable |
| `Save-GeneratedFiles` | Save generated files to disk (expects `code` property) |

---

## ⚠️ Critical: Parameter Consistency

When generating a solution with components, **you MUST use identical parameters** across related calls:

| Parameter | Must Be Identical In |
|-----------|---------------------|
| `SolutionName` | `generate-workspace` AND `generate` |
| `Feature.Name` | `generate-workspace` (in Features array) AND `generate` |

### Why This Matters
- `generate-workspace` creates project structure based on `SolutionName` + `Features`
- `generate` creates files that reference those projects
- Mismatched parameters = files that don't match the project structure

### ❌ WRONG
```powershell
# generate-workspace with Feature "Orders"
$result = Invoke-ArchGenerateWorkspace -SolutionName 'IaResearch.OrderService' -Features @(@{Name='Orders'})

# generate with Feature "Order" - MISMATCH!
$components = @(
    @{ComponentRole='DomainModel'; Name='Order'}
    @{ComponentRole='Command'; Name='CreateOrderCommand'}
)
$result = Invoke-ArchGenerate -SolutionName 'IaResearch.OrderService' -Features @(@{Name='Order'}) -Components $components
```

### ✅ CORRECT
```powershell
# BOTH calls use the same SolutionName AND Feature name
$result = Invoke-ArchGenerateWorkspace -SolutionName 'IaResearch.OrderService' -Features @(@{Name='Orders'; ApplicationKind='WebApi'})

$components = @(
    @{ComponentRole='DomainModel'; Name='Order'}
    @{ComponentRole='Command'; Name='CreateOrderCommand'}
    @{ComponentRole='CommandHandler'; Name='CreateOrderCommandHandler'}
)
$result = Invoke-ArchGenerate -SolutionName 'IaResearch.OrderService' -Features @(@{Name='Orders'}) -Components $components
```

---

## Critical Rules

1. **Load helpers first** - Always execute the PowerShell helpers before any other call
2. **MCP is the source of truth** - Never guess layer assignments, file paths, or project structures
3. **Never infer** - If unsure, call the MCP tool; do not assume
4. **Apply output exactly** - Do not modify, enhance, or "improve" MCP output
5. **Validate compliance** - Use the `Invoke-ArchValidate` function to check architecture rules
6. **Parameter consistency** - Always pass the same `SolutionName` and `Feature` to related calls
7. **Component roles are authoritative** - Only use `ComponentRole` values returned by `Invoke-ArchGetComponentRoles`; never invent role names
6. **Parameter consistency** - Always pass the same `SolutionName` and `Feature` to related calls

---

## Remember

- The MCP tool enforces architectural patterns specific to this solution
- Your role is to interface with the MCP tool and apply its output exactly
- Architecture decisions belong to the MCP tool, not to your inference
- When in doubt, consult the manifest: `http://localhost:5000/v1.0/Mcp/manifest`

---

## Initialization Complete

You are now ready to assist with architecture-related tasks. When the user requests code generation or scaffolding:
1. **Load the PowerShell helpers first**
2. Use the helper functions to query and generate
3. Apply the output exactly as returned

**Do NOT ask clarifying questions or prompt the user after initialization. Simply acknowledge that you are ready and wait silently for the user's next request.**