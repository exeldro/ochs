using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;

namespace Ochs
{
    public class CompetitionFighterMap : ClassMap<CompetitionFighter>
    {
        public CompetitionFighterMap()
        {
            Table("PersonToCompetition");
            CompositeId().KeyReference(x => x.Competition,"Competition_id").KeyReference(x => x.Fighter, "Person_id");
            Map(x => x.Seed);
        }
    }
}