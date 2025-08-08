using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AuditTrailRepository : Repository<AuditTrail>, IAuditTrailRepository
{
    public AuditTrailRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditTrail>> GetEntityAuditTrailAsync(string entityName, string entityId)
    {
        return await _dbSet
            .Where(a => a.EntityName == entityName && a.EntityId.Contains(entityId))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditTrail>> GetUserAuditTrailAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.CreatedBy == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditTrail>> GetAuditTrailByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
