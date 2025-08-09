using System.Net;
using Application.Common.Enums;
using Application.Common.Models;
using Application.DTOs.Auth;
using Application.Features.Authentication.Commands.Login;
using Application.Features.Authentication.Commands.Logout;
using Application.Features.Authentication.Commands.RefreshTokenCommands;
using Application.Features.Authentication.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Register([FromBody] RegisterRequest request)
        {
            var command = new RegisterUserCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                IpAddress = GetIpAddress()
            };
            AuthenticationResponse result = await _mediator.Send(command);
            var apiResponse = ApiResponse<AuthenticationResponse>.SuccessResponse(result, OperationType.Create, "User registered successfully.");
            return Ok(apiResponse);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password,
                IpAddress = GetIpAddress()
            };
            var result = await _mediator.Send(command);
            var apiResponse = ApiResponse<AuthenticationResponse>.SuccessResponse(result, OperationType.Create, "User logged in successfully.");
            return Ok(apiResponse);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken,
                IpAddress = GetIpAddress()
            };
            var result = await _mediator.Send(command);
            var apiResponse = ApiResponse<AuthenticationResponse>.SuccessResponse(result, OperationType.Create, "Token refreshed successfully.");
            return Ok(apiResponse);
        }

        [HttpPost("logout/{userId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Logout([FromQuery] Guid userId)
        {
            var command = new LogoutCommand
            {
                UserId = userId,
                IpAddress = GetIpAddress()
            };
            var result = await _mediator.Send(command);
            var apiResponse = ApiResponse<AuthenticationResponse>.SuccessResponse(result, OperationType.None, "Logged out successfully.");
            return Ok(apiResponse);
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "";

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
        }
    }
}
