using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using MediatR;

namespace Application.Features.Authentication.Commands.RegisterUser;
public class RegisterUserCommand : IRequest<AuthenticationResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}
