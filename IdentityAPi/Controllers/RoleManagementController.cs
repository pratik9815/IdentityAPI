using System.Net;
using Application.DTOs.Roles;
using Application.Features.Authentication.Commands.AssignRole;
using Application.Features.Roles.Commands;
using Application.Features.Roles.Commands.BulkAssignRole;
using Application.Features.Roles.Commands.CreateRole;
using Application.Features.Roles.Queries.GetRoles;
using Application.Features.Roles.Queries.GetUserRoles;
using Application.Features.Roles.Queries.GetUsersWithRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoleManagementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoleManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get all available roles
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles([FromQuery] bool includeUserCount = true)
        {
            var query = new GetRolesQuery { IncludeUserCount = includeUserCount };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        [HttpPost("roles")]
        [ProducesResponseType(typeof(RoleDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
        {
            var command = new CreateRoleCommand
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUserRoles), new { userId = Guid.Empty }, result);
        }

        /// <summary>
        /// Get all users with their assigned roles
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<UserWithRolesDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<UserWithRolesDto>>> GetUsersWithRoles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? roleId = null)
        {
            var query = new GetUsersWithRolesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                RoleId = roleId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get roles for a specific user
        /// </summary>
        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(typeof(UserWithRolesDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<UserWithRolesDto>> GetUserRoles(Guid userId)
        {
            var query = new GetUserRolesQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        [HttpPost("users/{userId}/roles")]
        [ProducesResponseType(typeof(RoleAssignmentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RoleAssignmentResponse>> AssignRole(
            Guid userId,
            [FromBody] AssignRoleRequest request)
        {
            var command = new AssignRoleCommand
            {
                UserId = userId,
                RoleId = request.RoleId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        [HttpDelete("users/{userId}/roles/{roleId}")]
        [ProducesResponseType(typeof(RoleAssignmentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RoleAssignmentResponse>> RemoveRole(Guid userId, Guid roleId)
        {
            var command = new RemoveRoleCommand
            {
                UserId = userId,
                RoleId = roleId
            };

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Bulk assign roles to multiple users
        /// </summary>
        [HttpPost("bulk-assign")]
        [ProducesResponseType(typeof(BulkRoleAssignmentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<BulkRoleAssignmentResponse>> BulkAssignRoles([FromBody] BulkRoleAssignmentRequest request)
        {
            var command = new BulkAssignRoleCommand
            {
                UserIds = request.UserIds,
                RoleIds = request.RoleIds
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Assign multiple roles to a single user
        /// </summary>
        [HttpPost("users/{userId}/roles/bulk")]
        [ProducesResponseType(typeof(BulkRoleAssignmentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<BulkRoleAssignmentResponse>> AssignMultipleRolesToUser(
            Guid userId,
            [FromBody] List<Guid> roleIds)
        {
            var command = new BulkAssignRoleCommand
            {
                UserIds = new List<Guid> { userId },
                RoleIds = roleIds
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Remove all roles from a user
        /// </summary>
        [HttpDelete("users/{userId}/roles")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveAllUserRoles(Guid userId)
        {
            // Get user roles first
            var userRolesQuery = new GetUserRolesQuery { UserId = userId };
            var userWithRoles = await _mediator.Send(userRolesQuery);

            // Remove each role
            var tasks = userWithRoles.Roles.Select(role =>
                _mediator.Send(new RemoveRoleCommand { UserId = userId, RoleId = role.Id }));

            await Task.WhenAll(tasks);

            return Ok(new { message = "All roles removed successfully" });
        }

        /// <summary>
        /// Get role assignment statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetRoleStatistics()
        {
            var rolesQuery = new GetRolesQuery { IncludeUserCount = true };
            var roles = await _mediator.Send(rolesQuery);

            var usersQuery = new GetUsersWithRolesQuery { PageSize = int.MaxValue };
            var users = await _mediator.Send(usersQuery);

            var statistics = new
            {
                TotalRoles = roles.Count(),
                TotalUsers = users.Count(),
                UsersWithRoles = users.Count(u => u.Roles.Any()),
                UsersWithoutRoles = users.Count(u => !u.Roles.Any()),
                RoleDistribution = roles.Select(r => new
                {
                    RoleName = r.Name,
                    UserCount = r.UserCount
                }).OrderByDescending(r => r.UserCount),
                TopRoles = roles
                    .OrderByDescending(r => r.UserCount)
                    .Take(5)
                    .Select(r => new { r.Name, r.UserCount })
            };

            return Ok(statistics);
        }
    }
}
