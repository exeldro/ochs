using FluentNHibernate.Mapping;

namespace Ochs
{
    public class PoolRankingMap : SubclassMap<PoolRanking>
    {
        public PoolRankingMap()
        {
            References(x => x.Pool);
        }
    }
}