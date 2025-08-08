using System.Net;
using FluentValidation;

namespace IdentityAPi.ExceptionHandlingMiddleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex) 
        {
            await WriteValidationFailedResponse(context, ex.Errors.Select(e => e.ErrorMessage));
        }
    }
    private static async Task WriteValidationFailedResponse(HttpContext context, IEnumerable<string> errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            Message = "Validation failed",
            TraceId = context.TraceIdentifier
            // Optionally: Errors = errors
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
