using FluentNHibernate.Mapping;

namespace Ochs
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id);
            Map(x => x.Name).Unique();
            Map(x => x.HashedPassword);
            Map(x => x.LastLogin);
            HasMany(x => x.Roles).Inverse();
        }
    }
}