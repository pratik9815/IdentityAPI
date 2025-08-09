using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Queries.GetUserRoles;

public class GetUserRolesQuery : IRequest<UserWithRolesDto>
{
    public Guid UserId { get; set; }
}
