using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AuditTrail>> GetEntityHistoryAsync(string entityName, string entityId)
    {
        return await _unitOfWork.AuditTrails.GetEntityAuditTrailAsync(entityName, entityId);
    }

    public async Task<IEnumerable<AuditTrail>> GetUserActivityAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var userAuditTrail = await _unitOfWork.AuditTrails.GetUserAuditTrailAsync(userId);

        if (from.HasValue || to.HasValue)
        {
            userAuditTrail = userAuditTrail.Where(audit =>
                (!from.HasValue || audit.CreatedAt >= from.Value) &&
                (!to.HasValue || audit.CreatedAt <= to.Value));
        }

        return userAuditTrail.OrderByDescending(a => a.CreatedAt);
    }

    public async Task<IEnumerable<AuditTrail>> GetSystemAuditLogAsync(DateTime from, DateTime to)
    {
        return await _unitOfWork.AuditTrails.GetAuditTrailByDateRangeAsync(from, to);
    }
}
