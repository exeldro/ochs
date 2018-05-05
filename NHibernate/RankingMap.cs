using FluentNHibernate.Mapping;

namespace Ochs
{
    public class RankingMap : ClassMap<Ranking>
    {
        public RankingMap()
        {
            Id(x => x.Id);
            References(x => x.Person);
            Map(x => x.Rank);
            Map(x => x.MatchPoints);
            Map(x => x.Matches);
            Map(x => x.Draws);
            Map(x => x.Wins);
            Map(x => x.Losses);
            Map(x => x.DoubleHits);
            Map(x => x.Penalties);
            Map(x => x.Warnings);
            Map(x => x.Exchanges);
            Map(x => x.HitsGiven);
            Map(x => x.HitsReceived);
        }
        
    }
}