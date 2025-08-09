using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId);
    Task<IEnumerable<UserRole>> GetUserRolesByRoleIdAsync(Guid roleId);
    Task<bool> UserHasRoleAsync(Guid userId, Guid roleId);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);
    Task RemoveAllUserRolesAsync(Guid userId);
    Task RemoveRoleFromAllUsersAsync(Guid roleId);
}
