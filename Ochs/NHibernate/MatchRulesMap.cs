using FluentNHibernate.Mapping;

namespace Ochs
{
    public class MatchRulesMap :ClassMap<MatchRules>
    {
        public MatchRulesMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Rounds);
            Map(x => x.PointsMax);
            Map(x => x.ExchangePointsMax);
            Map(x => x.TimeMax);
            Map(x => x.ExchangesMax);
        }
    }
}