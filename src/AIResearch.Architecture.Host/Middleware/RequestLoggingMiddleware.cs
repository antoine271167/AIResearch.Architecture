using System.Text;
using System.Text.Json;

namespace AIResearch.Architecture.Host.Middleware;

internal sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        logger.LogInformation("Content-Type: {ContentType}", context.Request.ContentType);
        logger.LogInformation("Headers: {Headers}",
            string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")));

        context.Request.EnableBuffering();

        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        logger.LogInformation("Request Body: {Body}", requestBody);

        var originalResponseBodyStream = context.Response.Body;

        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await next(context);

        logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);

        if (IsTextBasedContentType(context.Response.ContentType))
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var formattedBody = FormatResponseBody(responseBody, context.Response.ContentType);
            logger.LogInformation(
                """
                Response Body:
                ==
                {Body}
                ==
                """, formattedBody);
        }

        await responseBodyStream.CopyToAsync(originalResponseBodyStream);
    }

    private static bool IsTextBasedContentType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        var textBasedTypes = new[]
        {
            "application/json",
            "application/xml",
            "application/problem+json",
            "text/plain",
            "text/html",
            "text/xml",
            "application/javascript"
        };

        return textBasedTypes.Any(type => contentType.Contains(type, StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatResponseBody(string responseBody, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return responseBody;
        }

        if (contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true ||
            contentType?.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(responseBody);
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                {
                    Indented = true,
                    IndentCharacter = ' ',
                    IndentSize = 2
                });

                jsonDocument.WriteTo(writer);
                writer.Flush();

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (JsonException)
            {
                // If JSON parsing fails, return the original body
                return responseBody;
            }
        }

        return responseBody;
    }
}