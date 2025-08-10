using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Roles;
using Application.DTOs.Users;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdDTO>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<GetUserByIdDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetUserById(request.userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.userId} not found.");
        }
        var roles = new List<UserRoleDto>();
        foreach (var ur in user.UserRoles)
        {
            var assignedByUser = await _unitOfWork.Users.GetUserById(Guid.Parse(ur.CreatedBy));
            roles.Add(new UserRoleDto
            {
                UserId = ur.UserId,
                UserName = ur.User.FirstName,
                UserEmail = ur.User.Email,
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                AssignedAt = ur.Role.CreatedAt,
                AssignedBy = assignedByUser?.FirstName ?? "Unknown"
            });
        }
        return new GetUserByIdDTO
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
            Roles = roles
        };
    }
}
