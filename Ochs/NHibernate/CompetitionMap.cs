using FluentNHibernate.Mapping;

namespace Ochs
{
    public class CompetitionMap : ClassMap<Competition>
    {
        public CompetitionMap()
        {
            Id(x => x.Id);
            Map(x => x.Name).Unique().Not.Nullable();
            Map(x => x.Start);
            Map(x => x.Stop);
            References(x => x.MatchRules);
            References(x => x.RankingRules);
            HasMany(x => x.Phases).Cascade.DeleteOrphan().Inverse().OrderBy(nameof(Phase.PhaseOrder));
            HasMany(x => x.Matches).Inverse();
            References(x => x.Organization);
            HasMany(x => x.Fighters).Cascade.DeleteOrphan().Inverse();
        }
    }
}