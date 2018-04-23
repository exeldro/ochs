using System;

namespace Ochs
{
    public class UserView
    {
        protected readonly User _user;

        public UserView(User user)
        {
            _user = user;
        }

        public virtual Guid Id => _user.Id;
        public virtual string Username => _user.Name;
    }
}