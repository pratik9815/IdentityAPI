using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Queries.GetRoles;

public class GetRolesQuery : IRequest<IEnumerable<RoleDto>>
{
    public bool IncludeUserCount { get; set; } = true;
}