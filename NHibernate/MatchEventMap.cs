using FluentNHibernate.Mapping;

namespace Ochs
{
    public class MatchEventMap : ClassMap<MatchEvent>
    {
        public MatchEventMap()
        {
            Id(x => x.Id);
            Map(x => x.ChangedDateTime).Index("index_ChangedDateTime");
            Map(x => x.CreatedDateTime).Index("index_CreatedDateTime");
            Map(x => x.MatchTime);
            Map(x => x.PointsBlue);
            Map(x => x.PointsRed);
            Map(x => x.Round);
            Map(x => x.Type);
            References(x => x.Match);
        }
    }
}