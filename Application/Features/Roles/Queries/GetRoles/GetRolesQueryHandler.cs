using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Roles.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var result = new List<RoleDto>();

        foreach (var role in roles.Where(r => !r.IsDeleted))
        {
            var userCount = 0;
            if (request.IncludeUserCount)
            {
                var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.RoleId == role.Id);
                userCount = userRoles.Count();
            }

            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedAt = role.UpdatedAt,
                UpdatedBy = role.UpdatedBy,
                UserCount = userCount
            });
        }

        return result.OrderBy(r => r.Name);
    }
}
