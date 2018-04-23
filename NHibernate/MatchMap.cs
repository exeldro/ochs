using System.Security.Cryptography.X509Certificates;
using FluentNHibernate.Mapping;

namespace Ochs
{
    public class MatchMap :ClassMap<Match>
    {
        public MatchMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Location);
            References(x => x.FighterRed).Not.LazyLoad();
            References(x => x.FighterBlue).Not.LazyLoad();
            Map(x => x.Round);
            Map(x => x.ScoreBlue);
            Map(x => x.ScoreRed);
            Map(x => x.ExchangeCount);
            Map(x => x.DoubleCount);
            Map(x => x.Result);
            Map(x => x.Time);
            Map(x => x.TimeRunningSince);
            Map(x => x.PlannedDateTime).Index("index_PlannedDateTime");
            Map(x => x.StartedDateTime).Index("index_StartedDateTime");
            Map(x => x.FinishedDateTime).Index("index_FinishedDateTime");
            HasMany(x => x.Events).Cascade.AllDeleteOrphan().Inverse();
            References(x => x.Competition).Not.LazyLoad();
            References(x => x.Phase).Not.LazyLoad();
            References(x => x.Pool).Not.LazyLoad();
        }

    }
}