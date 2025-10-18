using Domain.Entities.Common;

namespace Domain.Entities;

public class RoleMenu : BaseAuditableEntity
{
    public Guid RoleId { get; set; }
    public Guid MenuFunctionId { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual MenuFunction MenuFunction { get; set; } = null!;
}
