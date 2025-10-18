using Domain.Entities.Common;

namespace Domain.Entities;

public class MenuFunction : BaseAuditableSoftDeleteEntity
{
    public Guid Id { get; set; }
    public string MenuName { get; set; }
    public string MenuDescription { get; set; }
    public string MenuCode { get; set; }
    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}
