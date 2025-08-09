using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Roles.Queries.GetUserRoles;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, UserWithRolesDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserWithRolesDto> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        if (user == null || user.IsDeleted)
        {
            throw new ArgumentException("User not found");
        }

        var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == request.UserId);
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
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = roles.OrderBy(r => r.Name).ToList()
        };
    }
}
