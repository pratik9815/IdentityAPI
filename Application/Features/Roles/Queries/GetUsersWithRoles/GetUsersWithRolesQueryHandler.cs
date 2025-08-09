using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Roles.Queries.GetUsersWithRoles;

public class GetUsersWithRolesQueryHandler : IRequestHandler<GetUsersWithRolesQuery, IEnumerable<UserWithRolesDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersWithRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserWithRolesDto>> Handle(GetUsersWithRolesQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var filteredUsers = users.Where(u => !u.IsDeleted).AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredUsers = filteredUsers.Where(u =>
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        // Apply role filter
        if (request.RoleId.HasValue)
        {
            var usersWithRole = await _unitOfWork.UserRoles.FindAsync(ur => ur.RoleId == request.RoleId.Value);
            var userIdsWithRole = usersWithRole.Select(ur => ur.UserId).ToHashSet();
            filteredUsers = filteredUsers.Where(u => userIdsWithRole.Contains(u.Id));
        }

        // Apply pagination
        var pagedUsers = filteredUsers
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var result = new List<UserWithRolesDto>();

        foreach (var user in pagedUsers)
        {
            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == user.Id);
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

            result.Add(new UserWithRolesDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                Roles = roles.OrderBy(r => r.Name).ToList()
            });
        }

        return result;
    }
}
