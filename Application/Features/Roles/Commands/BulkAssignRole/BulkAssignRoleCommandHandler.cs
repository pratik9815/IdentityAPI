using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Roles.Commands.BulkAssignRole;

public class BulkAssignRoleCommandHandler : IRequestHandler<BulkAssignRoleCommand, BulkRoleAssignmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public BulkAssignRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkRoleAssignmentResponse> Handle(BulkAssignRoleCommand request, CancellationToken cancellationToken)
    {
        var response = new BulkRoleAssignmentResponse
        {
            TotalUsers = request.UserIds.Count
        };

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            foreach (var userId in request.UserIds)
            {
                try
                {
                    // Check if user exists
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user == null || user.IsDeleted)
                    {
                        response.FailedAssignments++;
                        response.Errors.Add($"User {userId} not found");
                        continue;
                    }

                    // Get existing user roles
                    var existingUserRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId);
                    var existingRoleIds = existingUserRoles.Select(ur => ur.RoleId).ToHashSet();

                    var assignedRoles = new List<RoleDto>();

                    foreach (var roleId in request.RoleIds)
                    {
                        // Skip if user already has this role
                        if (existingRoleIds.Contains(roleId))
                            continue;

                        // Check if role exists
                        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                        if (role == null || role.IsDeleted)
                        {
                            response.Errors.Add($"Role {roleId} not found");
                            continue;
                        }

                        // Create new user role assignment
                        var userRole = new UserRole
                        {
                            UserId = userId,
                            RoleId = roleId,
                            AssignedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.UserRoles.AddAsync(userRole);

                        assignedRoles.Add(new RoleDto
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description,
                            CreatedAt = role.CreatedAt,
                            CreatedBy = role.CreatedBy
                        });
                    }

                    if (assignedRoles.Any())
                    {
                        response.SuccessfulAssignments++;
                        response.UpdatedUsers.Add(new UserWithRolesDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            IsActive = user.IsActive,
                            Roles = assignedRoles
                        });
                    }
                }
                catch (Exception ex)
                {
                    response.FailedAssignments++;
                    response.Errors.Add($"Error processing user {userId}: {ex.Message}");
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            response.Errors.Add($"Transaction failed: {ex.Message}");
        }

        return response;
    }
}
