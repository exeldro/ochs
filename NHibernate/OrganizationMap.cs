using FluentNHibernate.Mapping;

namespace Ochs
{
    public class OrganizationMap : ClassMap<Organization>
    {
        public OrganizationMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            HasMany(x => x.Competitions).Cascade.DeleteOrphan().Inverse();
        }
    }
}