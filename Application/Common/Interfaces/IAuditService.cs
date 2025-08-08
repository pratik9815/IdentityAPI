using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAuditService
{
    Task<IEnumerable<AuditTrail>> GetEntityHistoryAsync(string entityName, string entityId);
    Task<IEnumerable<AuditTrail>> GetUserActivityAsync(string userId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<AuditTrail>> GetSystemAuditLogAsync(DateTime from, DateTime to);
}
