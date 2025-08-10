using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Authentication.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthenticationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }
    public async Task<AuthenticationResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (!await _unitOfWork.Users.IsEmailUniqueAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower(),
            PasswordHash = _passwordService.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            IsActive = true,
            IsEmailVerified = false
            // CreatedAt, CreatedBy will be set automatically by DbContext
        };

        // Add user to database
        await _unitOfWork.Users.AddAsync(user);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Create refresh token entity
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress,
            UserId = user.Id
            // CreatedAt, CreatedBy will be set automatically by DbContext
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailVerified = user.IsEmailVerified,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
                Roles = new List<string>()
            }
        };
    }
}
