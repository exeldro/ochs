using FluentNHibernate.Mapping;

namespace Ochs
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id);
            Map(x => x.Name).Unique().Not.Nullable();
            Map(x => x.HashedPassword).Not.Nullable();
            Map(x => x.LastLogin);
            HasMany(x => x.Roles).Inverse();
        }
    }
}