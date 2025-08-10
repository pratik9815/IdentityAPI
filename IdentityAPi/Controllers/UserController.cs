using Application.Common.Enums;
using Application.Common.Models;
using Application.DTOs.Users;
using Application.Features.Users.Queries.GetUserById;
using Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] bool includeUserCount = true)
        {
            IEnumerable<GetUserDTO> result = await _mediator.Send(new GetUsersQuery { IncludeUserCount = includeUserCount });
            var apiResponse = ApiResponse<IEnumerable<GetUserDTO>>.SuccessResponse(result, OperationType.Read, "User fetched successfully");
            return Ok(apiResponse);
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var request = new GetUserByIdQuery(userId);
            var result = await _mediator.Send(request); 
            if (result == null)
            {
                return NotFound(ApiResponse<GetUserByIdDTO>.FailureResponse($"User with ID {userId} not found.", OperationType.Read, "User not found"));
            }
            var apiResponse = ApiResponse<GetUserByIdDTO>.SuccessResponse(result, OperationType.Read, "User fetched successfully");
            return Ok(apiResponse);
        }
    }
}
