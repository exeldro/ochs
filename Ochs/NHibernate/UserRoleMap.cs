using FluentNHibernate.Mapping;

namespace Ochs
{
    public class UserRoleMap :ClassMap<UserRole>
    {
        public UserRoleMap()
        {
            Id(x => x.Id);
            References(x => x.User).Not.Nullable();
            References(x => x.Organization);
            Map(x => x.Role).Not.Nullable();
        }
        
    }
}