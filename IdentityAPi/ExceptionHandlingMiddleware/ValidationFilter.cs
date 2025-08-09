using Application.Common.Enums;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace IdentityAPi.ExceptionHandlingMiddleware;
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var firstError = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            var apiResponse = ApiResponse<string>.FailureResponse(
                firstError,
                OperationType.None,
                "Validation failed"
            );

            context.Result = new BadRequestObjectResult(apiResponse);
        }
    }
    public void OnActionExecuted(ActionExecutedContext context){ }
}

