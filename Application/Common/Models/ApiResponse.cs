using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Enums;

namespace Application.Common.Models;
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public string Errors { get; set; }
    public OperationType Operation { get; set; }
    public static ApiResponse<T> SuccessResponse(T data, OperationType operation, string message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "Request processed successfully.",
            Data = data,
            Errors = null,
            Operation = operation
        };
    }
    public static ApiResponse<T> FailureResponse(string errors, OperationType operation, string message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Request failed.",
            Data = default,
            Errors = errors,
            Operation = operation
        };
    }
}
