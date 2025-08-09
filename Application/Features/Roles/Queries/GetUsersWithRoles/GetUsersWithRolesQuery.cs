using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Queries.GetUsersWithRoles;

public class GetUsersWithRolesQuery : IRequest<IEnumerable<UserWithRolesDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public Guid? RoleId { get; set; }
}
