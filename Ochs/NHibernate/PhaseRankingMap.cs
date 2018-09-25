using FluentNHibernate.Mapping;

namespace Ochs
{
    public class PhaseRankingMap : SubclassMap<PhaseRanking>
    {
        public PhaseRankingMap()
        {
            References(x => x.Phase);
        }
        
    }
}