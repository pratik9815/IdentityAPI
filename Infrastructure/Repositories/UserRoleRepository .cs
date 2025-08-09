using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesByRoleIdAsync(Guid roleId)
    {
        return await _dbSet
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId)
    {
        return await _dbSet.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
    {
        return await _dbSet
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
    }

    public async Task RemoveAllUserRolesAsync(Guid userId)
    {
        var userRoles = await _dbSet.Where(ur => ur.UserId == userId).ToListAsync();
        _dbSet.RemoveRange(userRoles);
        await Task.CompletedTask;
    }

    public async Task RemoveRoleFromAllUsersAsync(Guid roleId)
    {
        var userRoles = await _dbSet.Where(ur => ur.RoleId == roleId).ToListAsync();
        _dbSet.RemoveRange(userRoles);
        await Task.CompletedTask;
    }
}
