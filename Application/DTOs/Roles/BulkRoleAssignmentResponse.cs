using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Roles;

public class BulkRoleAssignmentResponse
{
    public int TotalUsers { get; set; }
    public int SuccessfulAssignments { get; set; }
    public int FailedAssignments { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<UserWithRolesDto> UpdatedUsers { get; set; } = new();

}
