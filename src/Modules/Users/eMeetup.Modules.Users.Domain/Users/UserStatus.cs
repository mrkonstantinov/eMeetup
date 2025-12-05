using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eMeetup.Modules.Users.Domain.Users
{
    public enum UserStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Deleted = 4
    }
}
