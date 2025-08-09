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

namespace Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }
    public async Task<AuthenticationResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.ToLower());

        if (user == null || !user.IsActive || user.IsDeleted || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        // UpdatedAt and UpdatedBy will be set automatically by DbContext
        await _unitOfWork.Users.UpdateAsync(user);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Revoke old refresh tokens and create new one
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, request.IpAddress);

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress,
            UserId = user.Id
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
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            }
        };
    }
}
