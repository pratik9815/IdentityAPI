using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IAuditTrailRepository : IRepository<AuditTrail>
{
    Task<IEnumerable<AuditTrail>> GetEntityAuditTrailAsync(string entityName, string entityId);
    Task<IEnumerable<AuditTrail>> GetUserAuditTrailAsync(string userId);
    Task<IEnumerable<AuditTrail>> GetAuditTrailByDateRangeAsync(DateTime from, DateTime to);
}
