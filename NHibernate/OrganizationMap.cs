using FluentNHibernate.Mapping;
using NHibernate.Mapping;

namespace Ochs
{
    public class OrganizationMap : ClassMap<Organization>
    {
        public OrganizationMap()
        {
            Id(x => x.Id);
            Map(x => x.Name).Unique();
            HasMany(x => x.Competitions).Cascade.DeleteOrphan().Inverse();
            HasMany(x => x.Aliases).Element("Alias").Not.KeyNullable();
        }
    }
}