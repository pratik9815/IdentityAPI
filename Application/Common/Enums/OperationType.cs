using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Enums;

public enum OperationType
{
    None = 0,
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    Search = 5,
    Login = 6,
    Logout = 7,
    Process = 8
}
