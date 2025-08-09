using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Features.Authentication.Commands.Logout;

public class LogoutCommand : IRequest<AuthenticationResponse>
{
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}
