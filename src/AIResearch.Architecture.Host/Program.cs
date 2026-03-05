using System.Text.Json;
using System.Text.Json.Serialization;
using AIResearch.Architecture.Application;
using AIResearch.Architecture.Domain;
using AIResearch.Architecture.Host.Constants;
using AIResearch.Architecture.Host.Middleware;
using AIResearch.Architecture.Host.Services;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

ConfigureKestrel(builder);
ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();
return;

static void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenAnyIP(ApiConstants.Port); });
}

static void ConfigureServices(IServiceCollection services)
{
    services.AddDomainServices();
    services.AddApplicationServices();

    services.AddHttpContextAccessor();
    services.AddScoped<BaseUrlResolver>();
    services.AddScoped<IMcpManifestGenerator, McpManifestGenerator>();
    services.AddScoped<IPowerShellScriptGenerator, PowerShellScriptGenerator>();

    AddApiVersioning(services);
    AddExceptionHandling(services);
    AddSwagger(services);
    AddCorsPolicy(services);
    AddControllers(services);
}

static void AddApiVersioning(IServiceCollection services)
{
    var versionParts = ApiConstants.Version.Split('.');
    var majorVersion = int.Parse(versionParts[0]);
    var minorVersion = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;

    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
}

static void AddExceptionHandling(IServiceCollection services)
{
    services.AddProblemDetails();
    services.AddExceptionHandler<GlobalExceptionHandler>();
}

static void AddSwagger(IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        // Remove provider usage here; configure Swagger endpoints in middleware instead.
        options.CustomSchemaIds(type => type.FullName);
    });

    var baseUrl = $"{ApiConstants.Scheme}://{ApiConstants.Host}:{ApiConstants.Port}";
    Console.WriteLine($"Swagger UI available at: {baseUrl}/swagger");
    Console.WriteLine($"Swagger JSON endpoint: {baseUrl}/swagger/v{ApiConstants.Version}/swagger.json");
    Console.WriteLine($"PowerShell Helpers endpoint: {baseUrl}/v{ApiConstants.Version}/PowerShell/helpers");
    Console.WriteLine($"MCP Manifest endpoint: {baseUrl}/v{ApiConstants.Version}/Mcp/manifest");
}

static void AddCorsPolicy(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}

static void AddControllers(IServiceCollection services)
{
    services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new
                    {
                        Field = e.Key,
                        Errors = e.Value!.Errors.Select(x => new
                        {
                            Message = x.ErrorMessage +
                                      $"\n\nConsult the swagger JSON endpoint for API details: http://localhost:{ApiConstants.Port}/swagger/v{ApiConstants.Version}/swagger.json",
                            Exception = x.Exception?.Message
                        }).ToArray()
                    })
                    .ToArray();

                logger.LogWarning("Model validation failed: {Errors}", JsonSerializer.Serialize(errors));

                return new BadRequestObjectResult(new
                {
                    status = "validation_error",
                    errors
                });
            };
        });
}

static void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        ConfigureDevelopmentMiddleware(app);
    }
    else
    {
        ConfigureProductionMiddleware(app);
    }

    app.UseCors();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.MapControllers();
}

static void ConfigureDevelopmentMiddleware(WebApplication app)
{
    app.UseDeveloperExceptionPage();
    app.UseStaticFiles(); // Enable static files for dark mode CSS
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Create a Swagger endpoint for each discovered API version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Architecture API {description.GroupName.ToUpperInvariant()}"
            );
        }

        // Enable dark mode
        options.InjectStylesheet("/swagger-ui/dark-mode.css");
    });
}

static void ConfigureProductionMiddleware(WebApplication app)
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseHttpsRedirection();
}