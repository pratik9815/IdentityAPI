using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Commands;

public class RemoveRoleCommand : IRequest<RoleAssignmentResponse>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
