using System.Text.Json;
using Application.Common.Enums;
using Application.Common.Models;

namespace IdentityAPi.ExceptionHandlingMiddleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Map exceptions to status codes
        int statusCode = exception switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        }; 
        context.Response.StatusCode = statusCode;
        var errors = new List<string> { exception.Message };

        // Optional: Only show stack trace in Development
        if (_env.IsDevelopment() && !string.IsNullOrWhiteSpace(exception.StackTrace))
        {
            errors.Add($"StackTrace: {exception.StackTrace}");
        }
        var response = ApiResponse<string>.FailureResponse(
               errors.FirstOrDefault(),
               OperationType.None,
               GetMessageForStatusCode(context.Response.StatusCode)
           );

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
    private static string GetMessageForStatusCode(int statusCode) =>
           statusCode switch
           {
               StatusCodes.Status400BadRequest => "Bad request.",
               StatusCodes.Status401Unauthorized => "Unauthorized.",
               StatusCodes.Status404NotFound => "Resource not found.",
               StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
               _ => "An error occurred."
           };
}
