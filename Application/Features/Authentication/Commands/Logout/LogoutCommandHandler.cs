using Application.DTOs.Auth;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Authentication.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, AuthenticationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<AuthenticationResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Revoke all refresh tokens for the user
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(request.UserId,request.IpAddress);
        // Optionally, you can return a response indicating success
        return new AuthenticationResponse
        {
            AccessToken = "",
            RefreshToken = "",
            User = null,
        };
    }
}
