using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class PhasePoolView : PoolView
    {
        public PhasePoolView(Pool pool) : base(pool)
        {

        }

        public virtual int FightersTotal => _pool.Fighters.Count;

        public virtual int MatchesPlanned => _pool.Matches.Count(x => x.Planned);
        public virtual int MatchesTodo => _pool.Matches.Count(x => !x.Started);
        public virtual int MatchesFinished => _pool.Matches.Count(x => x.Finished);
        public virtual int MatchesStarted => _pool.Matches.Count(x => x.Started);
        public virtual int MatchesBusy => _pool.Matches.Count(x => x.Started && !x.Finished);

        public int MatchesTotal => _pool.Matches.Count;
    }
}