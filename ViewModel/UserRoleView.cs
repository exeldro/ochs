using System;

namespace Ochs
{
    public class UserRoleView
    {
        private readonly UserRole _userRole;

        public UserRoleView(UserRole userRole)
        {
            _userRole = userRole;
        }
        public virtual Guid Id => _userRole.Id;
        public virtual string Organization => _userRole.Organization?.Name;
        public virtual string Role => _userRole.Role.ToString();
    }
}