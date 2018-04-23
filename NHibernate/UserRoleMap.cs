using FluentNHibernate.Mapping;

namespace Ochs
{
    public class UserRoleMap :ClassMap<UserRole>
    {
        public UserRoleMap()
        {
            Id(x => x.Id);
            References(x => x.User);
            References(x => x.Organization).Not.LazyLoad();
            Map(x => x.Role);
        }
        
    }
}