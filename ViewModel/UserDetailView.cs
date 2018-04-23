using System;
using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class UserDetailView : UserView
    {
        public UserDetailView(User user) : base(user)
        {
        }
        public IList<UserRoleView> Roles => _user.Roles.Select(x => new UserRoleView(x)).ToList();
    }
}