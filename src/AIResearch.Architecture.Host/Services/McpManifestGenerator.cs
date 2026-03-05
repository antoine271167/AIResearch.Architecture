using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AIResearch.Architecture.Host.Attributes;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AIResearch.Architecture.Host.Services;

public class McpManifestGenerator(BaseUrlResolver baseUrlResolver) : IMcpManifestGenerator
{
    private static readonly HashSet<Type> _genericCollectionTypes =
    [
        typeof(List<>),
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IList<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyList<>)
    ];

    public string GenerateManifest(Type[] controllerTypes, string mcpName, string mcpDescription, string mcpVersion)
    {
        var baseUrl = baseUrlResolver.GetBaseUrl();
        var apiVersion = GetApiVersion(controllerTypes[0]);
        var powerShellHelpersUrl = baseUrlResolver.GetPowerShellHelpersUrl();

        var manifest = new JsonObject
        {
            ["name"] = mcpName,
            ["description"] = mcpDescription,
            ["version"] = mcpVersion,
            ["baseUrl"] = baseUrl,
            ["apiVersion"] = apiVersion,
            ["protocol"] = "http",
            ["swaggerUrl"] = baseUrlResolver.GetSwaggerJsonUrl(),


            // AI Client Instructions - Clear guidance for consuming AI agents
            ["clientInstructions"] = new JsonObject
            {
                ["invocationType"] = "powershell",
                ["required"] = true,
                ["BEFORE_ANY_CALL"] =
                    $"You MUST first execute: Invoke-RestMethod -Uri '{powerShellHelpersUrl}' | Invoke-Expression",
                ["DO_NOT"] = new JsonArray
                {
                    "Write your own PowerShell HTTP calls - use the provided helper functions",
                    "Use Invoke-RestMethod directly for POST endpoints - JSON encoding will fail",
                    "Modify the generated code - apply it exactly as returned",
                    "Skip workflow steps or reorder them",
                    "Generate workspace BEFORE generating components",
                    "Combine multiple workflow steps into a single call",
                    "Proceed to the next step before the current step has succeeded",
                    "Call Save-GeneratedFiles if Invoke-ArchGenerate or Invoke-ArchGenerateWorkspace returned an HTTP error or if $result.files is null - a null $result.files always means the previous generate step FAILED and must be retried with a corrected request"
                },
                ["STEP_EXECUTION_RULES"] = new JsonObject
                {
                    ["RULE_1"] =
                        "Execute each workflow step INDIVIDUALLY as a separate call - never combine multiple steps into one",
                    ["RULE_2"] = "After each step, check whether it succeeded before proceeding",
                    ["RULE_3"] =
                        "If a step fails, read the error response, fix the request, and retry the SAME step - do NOT move on",
                    ["RULE_4"] = "Only proceed to the next step after the current step has completed successfully",
                    ["RULE_5"] =
                        "A step is considered successful when it returns HTTP 200 AND $result.files is not null. If $result.files IS null the step FAILED - do NOT call Save-GeneratedFiles; fix the request and retry the generate step."
                },
                ["CRITICAL_WORKFLOW_ORDER"] =
                    "You MUST follow the WORKFLOW steps in EXACT order, ONE step at a time. Each step must succeed before the next begins. Components MUST be generated BEFORE workspace. Do NOT skip steps. Do NOT reorder. Do NOT batch steps.",
                ["WORKFLOW"] = new JsonArray
                {
                    $"1. Load helpers: Invoke-RestMethod -Uri '{powerShellHelpersUrl}' | Invoke-Expression  ? verify: no error thrown",
                    "2. Load architecture rules: $rules = Invoke-ArchGetRules  ? verify: $rules is not null",
                    "3. Load component roles: $componentRoles = Invoke-ArchGetComponentRoles  ? verify: $componentRoles is not null. Use the 'name' values from $componentRoles.componentRoles as the ONLY valid ComponentRole values in steps 6. Do NOT use role names not present in this list.",
                    "4. Load architecture: $architecture = Invoke-ArchGetArchitecture  ? verify: $architecture is not null",
                    "5. Analyze & Design: Using the architecture rules, component roles, and structure, analyze the user's requirements and determine which components need to be created (e.g., Entities, Services, Controllers, DTOs, etc.)",
                    "6. Generate components: $result = Invoke-ArchGenerate -SolutionName '<name>' -Features @(@{Name='<feature>'}) -Components @(...)  ? MANDATORY verify: $result.files is not null. If $result.files IS null or an HTTP error was returned the step FAILED - read the full error body, fix the component definition (e.g. correct the ComponentRole to a valid 'name' from step 3 or add a Layer), and retry THIS step. Do NOT call Save-GeneratedFiles. Do NOT move to step 7.",
                    "7. Save component files: Save-GeneratedFiles -Files $result.files  ? ONLY call this after step 6 succeeded ($result.files is not null). verify: no error thrown",
                    "8. Generate workspace: $workspace = Invoke-ArchGenerateWorkspace -SolutionName '<name>' -Features @(@{Name='<feature>';ApplicationKind='WebApi'})  ? MANDATORY verify: $workspace.files is not null. If $workspace.files IS null or an HTTP error was returned the step FAILED - read the full error body, fix the request, and retry THIS step. Do NOT call Save-GeneratedFiles. Do NOT move to step 9.",
                    "9. Save workspace files: Save-GeneratedFiles -Files $workspace.files  ? ONLY call this after step 8 succeeded ($workspace.files is not null). verify: no error thrown"
                },
                ["WHY_THIS_ORDER"] =
                    "Components MUST be generated BEFORE workspace because the workspace generation needs to know what components exist to create the correct project structure and references.",
                ["ERROR_HANDLING"] = new JsonObject
                {
                    ["NEVER_DO"] =
                        "Do NOT skip a failed step and move on. Do NOT call Save-GeneratedFiles after a failed generate step. Do NOT stop processing without retrying. Always inspect the full response body before deciding what to do.",
                    ["ON_4XX_ERROR"] = new JsonArray
                    {
                        "1. Read the full response body - it contains structured error details explaining exactly what is wrong and often includes a suggested fix",
                        "2. Identify the root cause from the error message (e.g. missing required field, invalid value, wrong type)",
                        "3. Fix the request based on the error details - apply the suggested fix if one is provided",
                        "4. Retry the SAME step with the corrected request - do NOT proceed to the next step",
                        "5. Only escalate to the user if the error persists after retrying with a fix"
                    },
                    ["ON_5XX_ERROR"] = new JsonArray
                    {
                        "1. Read the full response body for diagnostic details",
                        "2. Report the error details to the user - server-side errors cannot be fixed by retrying"
                    }
                }
            },

            // CRITICAL: Use PowerShell helpers - DO NOT generate your own PowerShell
            ["POWERSHELL_SETUP"] = new JsonObject
            {
                ["MANDATORY_FIRST_STEP"] = "Before calling ANY endpoint, you MUST load the PowerShell helper functions",
                ["howToLoad"] = $"Invoke-RestMethod -Uri '{powerShellHelpersUrl}' | Invoke-Expression",
                ["whyRequired"] =
                    "The helper functions handle all JSON serialization, encoding, and API calls correctly. Do NOT write your own PowerShell - it will fail.",
                ["availableFunctions"] = new JsonArray
                {
                    "Invoke-ArchGetArchitecture - Get architecture definition",
                    "Invoke-ArchGetRules - Get architecture rules",
                    "Invoke-ArchGetComponentRoles - Get all valid component roles (use 'name' values for ComponentRole)",
                    "Invoke-ArchGenerateWorkspace -SolutionName <name> -Features @(@{Name='Feature';ApplicationKind='WebApi'})",
                    "Invoke-ArchGenerate -SolutionName <name> -Feature @{Name='Feature'} -Components @(@{ComponentRole='Entity';Name='Order'})",
                    "Invoke-ArchValidate -Files @{'path/file.cs'='content'}",
                    "Read-SourceFile -Path <filepath> - Read single file safely",
                    "Read-SourceFiles -Directory <dir> -Recurse - Read all .cs files to hashtable",
                    "Save-GeneratedFiles -Files $result.files - Write generated files to src folder"
                },
                ["exampleWorkflow"] = new JsonArray
                {
                    "# Step 1: Load helpers (REQUIRED)",
                    $"Invoke-RestMethod -Uri '{powerShellHelpersUrl}' | Invoke-Expression",
                    "",
                    "# Step 2: Load architecture rules",
                    "$rules = Invoke-ArchGetRules",
                    "",
                    "# Step 3: Load component roles",
                    "$componentRoles = Invoke-ArchGetComponentRoles",
                    "# Use $componentRoles.componentRoles.name values as ComponentRole values in step 6",
                    "",
                    "# Step 4: Load architecture",
                    "$architecture = Invoke-ArchGetArchitecture",
                    "",
                    "# Step 5: Analyze & Design (AI reasoning step)",
                    "# Based on the user's requirements and the loaded architecture rules and component roles:",
                    "# - Identify the feature(s) to implement",
                    "# - Determine required component roles (use ONLY names from $componentRoles.componentRoles)",
                    "# - Plan the component hierarchy and dependencies",
                    "# - Map requirements to appropriate layers based on architecture rules",
                    "",
                    "# Step 6: Generate components",
                    "$result = Invoke-ArchGenerate -SolutionName 'MyApp' -Feature @{Name='Order'} -Components @(@{ComponentRole='Entity';Name='Order'})",
                    "",
                    "# Step 7: Save component files",
                    "Save-GeneratedFiles -Files $result.files",
                    "",
                    "# Step 8: Generate workspace based on components",
                    "$workspace = Invoke-ArchGenerateWorkspace -SolutionName 'MyApp' -Features @(@{Name='Order';ApplicationKind='WebApi'})",
                    "",
                    "# Step 9: Save workspace files",
                    "Save-GeneratedFiles -Files $workspace.files"
                }
            },

            ["validEndpoints"] = new JsonObject
            {
                ["description"] = "These are the ONLY valid endpoints. Use exactly as shown.",
                ["powershell-helpers"] = $"{powerShellHelpersUrl} (LOAD THIS FIRST)",
                ["mcp-manifest"] = baseUrlResolver.GetMcpManifestUrl(),
                ["get-architecture"] = $"{baseUrlResolver.GetArchitectureBaseUrl()}/get-architecture",
                ["rules"] = $"{baseUrlResolver.GetArchitectureBaseUrl()}/rules",
                ["component-roles"] = $"{baseUrlResolver.GetArchitectureBaseUrl()}/component-roles",
                ["validate"] = $"{baseUrlResolver.GetArchitectureBaseUrl()}/validate",
                ["generate"] = $"{baseUrlResolver.GetArchitectureBaseUrl()}/generate",
                ["generate-workspace"] = $"{baseUrlResolver.GetArchitectureBaseUrl()}/generate-workspace"
            },

            ["actions"] = new JsonArray()
        };

        foreach (var controllerType in controllerTypes)
        {
            var controllerRoute = GetControllerRoute(controllerType);
            var methods =
                controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var mcpAction = method.GetCustomAttribute<McpActionAttribute>();
                if (mcpAction == null)
                {
                    continue;
                }

                var action = GenerateAction(method, mcpAction, controllerRoute, apiVersion, baseUrl,
                    powerShellHelpersUrl);
                ((JsonArray)manifest["actions"]!).Add(action);
            }
        }

        return JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            WriteIndented = true,
            IndentSize = 2,
            IndentCharacter = ' '
        });
    }

    private static string GetControllerRoute(Type controllerType)
    {
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        if (routeAttr?.Template == null)
        {
            return controllerType.Name.Replace("Controller", "").ToLower();
        }

        // Remove [controller] placeholder and version parameters
        var route = routeAttr.Template
            .Replace("[controller]", controllerType.Name.Replace("Controller", ""))
            .Replace("{version:apiVersion}", "{version}");

        return route;
    }

    private static string GetApiVersion(Type controllerType)
    {
        var versionAttr = controllerType.GetCustomAttribute<ApiVersionAttribute>();
        return versionAttr is { Versions.Count: > 0 }
            ? versionAttr.Versions[0].ToString()
            : "1.0";
    }

    private static JsonObject GenerateAction(MethodInfo method, McpActionAttribute mcpAction, string controllerRoute,
        string apiVersion, string baseUrl, string powerShellHelpersUrl)
    {
        var actionName = mcpAction.Name ?? DeriveActionNameFromRoute(method);
        var httpMethod = GetHttpMethod(method);
        var methodRoute = GetMethodRoute(method);

        // Build the complete endpoint path
        var fullRoute = controllerRoute.Replace("{version}", apiVersion);
        if (!string.IsNullOrEmpty(methodRoute))
        {
            fullRoute = $"{fullRoute}/{methodRoute}";
        }

        fullRoute = fullRoute.TrimStart('/');

        var fullUrl = $"{baseUrl}/{fullRoute}";

        var action = new JsonObject
        {
            ["name"] = actionName,
            ["description"] = mcpAction.Description,
            ["httpMethod"] = httpMethod,
            ["endpoint"] = fullRoute,
            ["fullUrl"] = fullUrl,
            ["USE_HELPER_FUNCTION"] =
                $"Load helpers first: Invoke-RestMethod -Uri '{powerShellHelpersUrl}' | Invoke-Expression",
            ["usage"] = new JsonObject
            {
                ["method"] = httpMethod,
                ["url"] = fullUrl,
                ["contentType"] = "application/json"
            }
        };

        // Generate input schema from method parameters
        var inputSchema = GenerateInputSchema(method);
        action["input"] = inputSchema;

        // Generate output schema from return type
        var outputSchema = GenerateOutputSchema(method);
        action["output"] = outputSchema;

        // Add examples from attributes
        var examples = GenerateExamples(method, fullUrl, httpMethod);
        if (examples.Count > 0)
        {
            action["examples"] = examples;
        }

        return action;
    }

    private static string GetHttpMethod(MethodInfo method)
    {
        var httpMethodAttribute = method.GetCustomAttributes()
            .FirstOrDefault(attr => attr.GetType().Namespace == "Microsoft.AspNetCore.Mvc");

        return httpMethodAttribute?.GetType().Name switch
        {
            "HttpGetAttribute" => "GET",
            "HttpPostAttribute" => "POST",
            "HttpPutAttribute" => "PUT",
            "HttpDeleteAttribute" => "DELETE",
            "HttpPatchAttribute" => "PATCH",
            _ => "GET"
        };
    }

    private static string GetMethodRoute(MethodInfo method)
    {
        var httpMethodAttribute = method.GetCustomAttributes()
            .FirstOrDefault(attr => attr.GetType().Name.StartsWith("Http") &&
                                    attr.GetType().Namespace == "Microsoft.AspNetCore.Mvc");

        if (httpMethodAttribute == null)
        {
            return string.Empty;
        }

        var templateProperty = httpMethodAttribute.GetType().GetProperty("Template");
        var route = templateProperty?.GetValue(httpMethodAttribute) as string;
        return route ?? string.Empty;
    }

    private static string DeriveActionNameFromRoute(MethodInfo method)
    {
        // Try to get route from HTTP method attributes (HttpGet, HttpPost, etc.)
        var httpMethodAttribute = method.GetCustomAttributes()
            .FirstOrDefault(attr => attr.GetType().Name.StartsWith("Http") &&
                                    attr.GetType().Namespace == "Microsoft.AspNetCore.Mvc");

        if (httpMethodAttribute == null)
        {
            return ConvertMethodNameToActionName(method.Name);
        }

        // Get the Template property which contains the route
        var templateProperty = httpMethodAttribute.GetType().GetProperty("Template");
        var route = templateProperty?.GetValue(httpMethodAttribute) as string;

        if (string.IsNullOrEmpty(route))
        {
            return ConvertMethodNameToActionName(method.Name);
        }

        // Remove route parameters like {id}, {version:apiVersion}, etc.
        route = Regex.Replace(route, @"\{[^}]+\}", "");

        // Remove trailing slashes
        route = route.Trim('/');

        if (!string.IsNullOrEmpty(route))
        {
            return route;
        }

        // Fallback: convert method name to kebab-case
        return ConvertMethodNameToActionName(method.Name);
    }

    private static string ConvertMethodNameToActionName(string methodName)
    {
        // Convert PascalCase to kebab-case
        // e.g., GetArchitecture -> get-architecture
        var result = string.Concat(
            methodName.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()
            )
        );

        return result;
    }

    private static JsonObject GenerateInputSchema(MethodInfo method)
    {
        var parameters = method.GetParameters()
            .Where(p => !p.ParameterType.Name.Contains("CancellationToken"))
            .ToList();

        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject(),
            ["required"] = new JsonArray()
        };

        foreach (var param in parameters)
        {
            var paramType = param.ParameterType;
            var properties = (JsonObject)schema["properties"]!;
            var required = (JsonArray)schema["required"]!;

            // Add parameter to schema
            properties[param.Name!] = GenerateTypeSchema(paramType);

            if (!param.HasDefaultValue)
            {
                required.Add(param.Name!);
            }
        }

        return schema;
    }

    private static JsonObject GenerateOutputSchema(MethodInfo method)
    {
        var returnType = method.ReturnType;

        // Handle Task<IActionResult> or ActionResult<T>
        if (!returnType.IsGenericType)
        {
            return new JsonObject
            {
                ["type"] = "object"
            };
        }

        var genericArgs = returnType.GetGenericArguments();
        if (genericArgs.Length <= 0)
        {
            return new JsonObject
            {
                ["type"] = "object"
            };
        }

        var innerType = genericArgs[0];

        // If it's ActionResult<T>, extract T
        if (!innerType.IsGenericType || !innerType.GetGenericTypeDefinition().Name.Contains("ActionResult"))
        {
            return GenerateTypeSchema(innerType);
        }

        var actualType = innerType.GetGenericArguments().FirstOrDefault();
        return GenerateTypeSchema(actualType ?? innerType);
    }

    private static JsonObject GenerateTypeSchema(Type type)
    {
        // Handle primitive types
        if (type == typeof(string))
        {
            return new JsonObject { ["type"] = "string" };
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return new JsonObject { ["type"] = "integer" };
        }

        if (type == typeof(bool))
        {
            return new JsonObject { ["type"] = "boolean" };
        }

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return new JsonObject { ["type"] = "number" };
        }

        // Handle collections
        if (type.IsArray || (type.IsGenericType && _genericCollectionTypes.Contains(type.GetGenericTypeDefinition())))
        {
            var elementType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
            return new JsonObject
            {
                ["type"] = "array",
                ["items"] = GenerateTypeSchema(elementType)
            };
        }

        // Handle Dictionary
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var valueType = type.GetGenericArguments()[1];
            return new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = GenerateTypeSchema(valueType)
            };
        }

        // Handle complex objects
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
        };

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var propSchema = GenerateTypeSchema(prop.PropertyType);
            var descriptionAttr = prop.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null && !string.IsNullOrEmpty(descriptionAttr.Description))
            {
                propSchema["description"] = descriptionAttr.Description;
            }

            ((JsonObject)schema["properties"]!)[ToCamelCase(prop.Name)] = propSchema;
        }

        return schema;
    }

    private static JsonArray GenerateExamples(MethodInfo method, string fullUrl, string httpMethod)
    {
        var examples = new JsonArray();
        var exampleAttributes = method.GetCustomAttributes<McpExampleAttribute>();

        foreach (var example in exampleAttributes)
        {
            var exampleObj = new JsonObject
            {
                ["description"] = example.Description,
                ["httpMethod"] = httpMethod,
                ["url"] = fullUrl
            };

            if (example.InputJson != null)
            {
                try
                {
                    exampleObj["input"] = JsonSerializer.Deserialize<JsonNode>(example.InputJson);

                    // Add PowerShell examples for POST/PUT/PATCH requests
                    if (httpMethod is "POST" or "PUT" or "PATCH")
                    {
                        // PowerShell example with single-quoted JSON body (easiest approach)
                        exampleObj["powershellExample"] =
                            $"Invoke-RestMethod -Uri '{fullUrl}' -Method {httpMethod} -ContentType 'application/json' -Body '{example.InputJson}'";

                        // PowerShell example using variable for complex JSON
                        exampleObj["powershellExampleWithVariable"] =
                            $"$body = '{example.InputJson}'; Invoke-RestMethod -Uri '{fullUrl}' -Method {httpMethod} -ContentType 'application/json' -Body $body";
                    }
                }
                catch
                {
                    exampleObj["call"] = example.InputJson;
                    if (httpMethod == "GET")
                    {
                        exampleObj["powershellExample"] = $"Invoke-RestMethod -Uri '{fullUrl}' -Method GET";
                    }
                }
            }
            else
            {
                // No body - add simple examples for GET requests
                if (httpMethod == "GET")
                {
                    exampleObj["powershellExample"] = $"Invoke-RestMethod -Uri '{fullUrl}' -Method GET";
                }
            }

            if (example.OutputJson != null)
            {
                exampleObj["output"] = JsonSerializer.Deserialize<JsonNode>(example.OutputJson);
            }

            if (example.ErrorMessage != null)
            {
                exampleObj["error"] = example.ErrorMessage;
            }

            examples.Add(exampleObj);
        }

        return examples;
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
        {
            return str;
        }

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}