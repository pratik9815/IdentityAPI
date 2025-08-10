using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Users;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Features.Users.Queries.GetUsers;
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<GetUserDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<IEnumerable<GetUserDTO>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllUserAsync();
        var userDtos = new List<GetUserDTO>();
        foreach (var user in users)
        {
            userDtos.Add(new GetUserDTO
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
            });
        }
        return userDtos;
    }
}
