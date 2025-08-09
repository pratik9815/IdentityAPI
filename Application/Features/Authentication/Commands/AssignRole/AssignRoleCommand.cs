using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using MediatR;

namespace Application.Features.Authentication.Commands.AssignRole;

public class AssignRoleCommand : IRequest<RoleAssignmentResponse>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
