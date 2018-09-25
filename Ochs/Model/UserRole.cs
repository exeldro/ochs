using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ochs
{
    public class UserRole
    {
        public virtual Guid Id { get; set; }
        public virtual User User { get; set; }
        public virtual UserRoles Role { get; set; }
        public virtual Organization Organization { get; set; }
    }

    public enum UserRoles
    {
        Admin,
        ScoreValidator,
        Scorekeeper
    }
}