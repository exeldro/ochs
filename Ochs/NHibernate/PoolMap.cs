using FluentNHibernate.Mapping;

namespace Ochs
{
    public class PoolMap : ClassMap<Pool>
    {
        public PoolMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            HasMany(x => x.Matches).Inverse();
            HasManyToMany(x => x.Fighters);
            References(x => x.Phase);
            Map(x => x.Location);
            Map(x => x.PlannedDateTime);
        }
    }
}