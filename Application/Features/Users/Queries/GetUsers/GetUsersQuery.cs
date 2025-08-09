using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Application.DTOs.Users;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;
public class GetUsersQuery : IRequest<IEnumerable<GetUserDTO>>
{
    public bool IncludeUserCount { get; set; } = true;
}
