using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Users;
using MediatR;

namespace Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid userId) : IRequest<GetUserByIdDTO>;
