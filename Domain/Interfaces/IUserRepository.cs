using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserWithRefreshTokensAsync(Guid userId);
    Task<bool> IsEmailUniqueAsync(string email);
    Task<IEnumerable<User>> GetAllUserAsync();
    Task<User> GetUserById(Guid userId);
}

