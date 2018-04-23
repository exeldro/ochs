using System;
using System.Collections.Generic;

namespace Ochs
{
    public class User
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string HashedPassword { get; set; }
        public virtual DateTime LastLogin { get; set; }
        public virtual IList<UserRole> Roles { get; set; } = new List<UserRole>();
    }
}