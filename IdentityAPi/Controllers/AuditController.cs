using System.Net;
using Application.Common.Enums;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public AuditController(IAuditService auditService, ICurrentUserService currentUserService)
        {
            _auditService = auditService;
            _currentUserService = currentUserService;
        }
        [HttpGet("entity/{entityName}/{entityId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AuditTrailDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetEntityHistory(string entityName, string entityId)
        {
            var auditTrail = await _auditService.GetEntityHistoryAsync(entityName, entityId);

            var result = auditTrail.Select(a => new AuditTrailDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                AffectedColumns = a.AffectedColumns,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            });
            var apiResponse = ApiResponse<IEnumerable<AuditTrailDto>>.SuccessResponse(result, OperationType.Read, "Entity history fetched successfully.");
            return Ok(apiResponse);
        }
        [HttpGet("user")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserActivityDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMyActivity([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var auditTrail = await _auditService.GetUserActivityAsync(userId, from, to);

            var result = auditTrail.Select(a => new UserActivityDto
            {
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Timestamp = a.CreatedAt,
                IpAddress = a.IpAddress,
                Description = GetActionDescription(a.Action, a.EntityName)
            });
            var apiResponse = ApiResponse<IEnumerable<UserActivityDto>>.SuccessResponse(result, OperationType.Read, "User activity fetched successfully.");
            return Ok(apiResponse);
        }
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserActivityDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserActivity(string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var auditTrail = await _auditService.GetUserActivityAsync(userId, from, to);

            var result = auditTrail.Select(a => new UserActivityDto
            {
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Timestamp = a.CreatedAt,
                IpAddress = a.IpAddress,
                Description = GetActionDescription(a.Action, a.EntityName)
            });
            var apiResponse = ApiResponse<IEnumerable<UserActivityDto>>.SuccessResponse(result, OperationType.Read, "User activity fetched successfully.");
            return Ok(apiResponse);
        }

        [HttpGet("system")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AuditTrailDto>>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AuditTrailDto>>>> GetSystemAuditLog([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var auditTrail = await _auditService.GetSystemAuditLogAsync(from, to);

            var result = auditTrail.Select(a => new AuditTrailDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                AffectedColumns = a.AffectedColumns,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent
            });
            var apiResponse = ApiResponse<IEnumerable<AuditTrailDto>>.SuccessResponse(result, OperationType.Read, "System audit log fetched successfully.");
            return Ok(apiResponse);
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<SystemAuditSummaryDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResponse<SystemAuditSummaryDto>>> GetAuditSummary([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var auditTrail = await _auditService.GetSystemAuditLogAsync(from, to);

            var summary = new SystemAuditSummaryDto
            {
                TotalActions = auditTrail.Count(),
                UserCreations = auditTrail.Count(a => a.Action == "CREATE" && a.EntityName == "User"),
                UserUpdates = auditTrail.Count(a => a.Action == "UPDATE" && a.EntityName == "User"),
                UserDeletions = auditTrail.Count(a => a.Action == "DELETE" && a.EntityName == "User"),
                LoginAttempts = auditTrail.Count(a => a.EntityName == "RefreshToken" && a.Action == "CREATE"),
                ActionsByType = auditTrail.GroupBy(a => a.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActionsByUser = auditTrail.Where(a => !string.IsNullOrEmpty(a.CreatedBy))
                    .GroupBy(a => a.CreatedBy!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
            var apiResponse = ApiResponse<SystemAuditSummaryDto>.SuccessResponse(summary, OperationType.Read, "Audit summary fetched successfully.");
            return Ok(apiResponse);
        }

        private static string GetActionDescription(string action, string entityName)
        {
            return action switch
            {
                "CREATE" => $"Created {entityName.ToLower()}",
                "UPDATE" => $"Updated {entityName.ToLower()}",
                "DELETE" => $"Deleted {entityName.ToLower()}",
                _ => $"{action} {entityName.ToLower()}"
            };
        }
    }
}
