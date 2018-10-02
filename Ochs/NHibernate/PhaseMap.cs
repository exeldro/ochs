using System.Security.Cryptography.X509Certificates;
using FluentNHibernate.Mapping;

namespace Ochs
{
    public class PhaseMap : ClassMap<Phase>
    {
        public PhaseMap()
        {
            Id(x => x.Id);
            References(x => x.Competition);
            Map(x => x.PhaseOrder);
            Map(x => x.Name);
            Map(x => x.PhaseType);
            HasMany(x => x.Matches).Inverse();
            References(x => x.MatchRules);
            HasMany(x => x.Pools).Cascade.DeleteOrphan().Inverse();
            HasManyToMany(x => x.Fighters);
            Map(x => x.Location);
        }
    }
}