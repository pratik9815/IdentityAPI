using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Commands.BulkAssignRole;

public class BulkAssignRoleCommand : IRequest<BulkRoleAssignmentResponse>
{
    public List<Guid> UserIds { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
}