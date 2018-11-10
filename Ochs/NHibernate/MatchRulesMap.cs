using FluentNHibernate.Mapping;

namespace Ochs
{
    public class MatchRulesMap :ClassMap<MatchRules>
    {
        public MatchRulesMap()
        {
            Id(x => x.Id);
            Map(x => x.Name).Unique().Not.Nullable();
            Map(x => x.Rounds);
            Map(x => x.PointsMax);
            Map(x => x.ExchangePointsMax);
            Map(x => x.TimeMax);
            Map(x => x.ExchangesMax);
            Map(x => x.DoubleHitMax);
            Map(x => x.BothCanScore);
            Map(x => x.SubtractPoints);
            Map(x => x.RecordSportsmanship);
            Map(x => x.RecordWarnings);
            Map(x => x.RecordProtests);
            Map(x => x.RecordAfterblow);
            Map(x => x.AfterblowScores);
            Map(x => x.RecordDoubleHits);
            Map(x => x.DoubleHitScores);
            Map(x => x.RecordUnclearExchanges);
            Map(x => x.RecordPenalties);
            Map(x => x.PenaltyPoints);
        }
    }
}