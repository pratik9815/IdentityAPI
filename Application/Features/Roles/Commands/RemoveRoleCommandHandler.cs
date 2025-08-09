using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Roles.Commands;

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, RoleAssignmentResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleAssignmentResponse> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                return new RoleAssignmentResponse
                {
                    Success = false,
                    Message = "User not found",
                    Errors = { "User does not exist or has been deleted" }
                };
            }

            // Check if role exists
            var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
            if (role == null)
            {
                return new RoleAssignmentResponse
                {
                    Success = false,
                    Message = "Role not found",
                    Errors = { "Role does not exist" }
                };
            }

            // Find user role assignment
            var userRole = await _unitOfWork.UserRoles.GetFirstOrDefaultAsync(
                ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);

            if (userRole == null)
            {
                return new RoleAssignmentResponse
                {
                    Success = false,
                    Message = "Role not assigned",
                    Errors = { $"User does not have the role '{role.Name}'" }
                };
            }

            // Remove role assignment
            await _unitOfWork.UserRoles.DeleteAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            // Get updated user with roles
            var updatedUser = await GetUserWithRoles(request.UserId);

            return new RoleAssignmentResponse
            {
                Success = true,
                Message = $"Role '{role.Name}' successfully removed from user",
                User = updatedUser
            };
        }
        catch (Exception ex)
        {
            return new RoleAssignmentResponse
            {
                Success = false,
                Message = "An error occurred while removing role",
                Errors = { ex.Message }
            };
        }
    }

    private async Task<UserWithRolesDto> GetUserWithRoles(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId);

        var roles = new List<RoleDto>();
        foreach (var userRole in userRoles)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(userRole.RoleId);
            if (role != null && !role.IsDeleted)
            {
                roles.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    CreatedAt = role.CreatedAt,
                    CreatedBy = role.CreatedBy,
                    UpdatedAt = role.UpdatedAt,
                    UpdatedBy = role.UpdatedBy
                });
            }
        }

        return new UserWithRolesDto
        {
            Id = user!.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = roles
        };
    }
}
