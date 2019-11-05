using FluentNHibernate.Mapping;

namespace Ochs
{
    public class RankingRulesMap : ClassMap<RankingRules>
    {
        public RankingRulesMap()
        {
            Id(x => x.Id);
            Map(x => x.Name).Unique().Not.Nullable();
            Map(x => x.WinPoints);
            Map(x => x.LossPoints);
            Map(x => x.DrawPoints);
            Map(x => x.ForfeitPoints);
            Map(x => x.RemoveForfeitedFromRanking);
            Map(x => x.DisqualificationPoints);
            Map(x => x.RemoveDisqualifiedFromRanking);
            Map(x => x.DoubleReductionThreshold);
            Map(x => x.DoubleReductionFactor);
            HasMany(x => x.Sorting).Table("RankingRulesSorting").Element("RankingStat").AsList(part => part.Column("RankingStatOrder"));
        }
    }
}