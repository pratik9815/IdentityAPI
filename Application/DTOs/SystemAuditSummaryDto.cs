using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;

public class SystemAuditSummaryDto
{
    public int TotalActions { get; set; }
    public int UserCreations { get; set; }
    public int UserUpdates { get; set; }
    public int UserDeletions { get; set; }
    public int LoginAttempts { get; set; }
    public int FailedLogins { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
    public Dictionary<string, int> ActionsByUser { get; set; } = new();

}
