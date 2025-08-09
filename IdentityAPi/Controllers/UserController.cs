using Application.Common.Enums;
using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            throw new ArgumentNullException("Argument cannot be null");
            var apiResponse = new ApiResponse<IEnumerable<GetUserDTO>>
            {
                Success = true,
                Message = "User fetched successfully.",
                Data = result,
                Errors = null,
                Operation = OperationType.Read
            };
            return Ok(apiResponse);
        }
    }
}
