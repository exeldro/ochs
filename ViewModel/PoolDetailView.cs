﻿using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class PoolDetailView : PoolView
    {
        public PoolDetailView(Pool pool) : base(pool)
        {

        }

        public virtual int MatchesPlanned => _pool.Matches.Count(x => x.Planned);
        public virtual int MatchesTodo => _pool.Matches.Count(x => !x.Started);
        public virtual int MatchesFinished => _pool.Matches.Count(x => x.Finished);
        public virtual int MatchesStarted => _pool.Matches.Count(x => x.Started);
        public virtual int MatchesBusy => _pool.Matches.Count(x => x.Started && !x.Finished);

        public IList<MatchView> Matches => _pool.Matches.Select(x => new MatchView(x)).ToList();
        public IList<PersonView> Fighters => _pool.Fighters.Select(x => new PersonView(x)).ToList();
    }
}