using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Roles;

public class BulkRoleAssignmentRequest
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();

    [Required]
    public List<Guid> RoleIds { get; set; } = new();
}
