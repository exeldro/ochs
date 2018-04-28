using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class PhaseDetailView : PhaseView
    {
        public PhaseDetailView(Phase phase) : base(phase)
        {

        }

        public virtual int MatchesPlanned => _phase.Matches.Count(x => x.Planned);
        public virtual int MatchesTodo => _phase.Matches.Count(x => !x.Started);
        public virtual int MatchesFinished => _phase.Matches.Count(x => x.Finished);
        public virtual int MatchesStarted => _phase.Matches.Count(x => x.Started);
        public virtual int MatchesBusy => _phase.Matches.Count(x => x.Started && !x.Finished);

        public IList<MatchView> Matches => _phase.Matches.Select(x => new MatchView(x)).ToList();
        public IList<PhasePoolView> Pools => _phase.Pools.Select(x => new PhasePoolView(x)).ToList();
        public IList<PhaseFighterView> Fighters => _phase.Fighters.Select(x => new PhaseFighterView(x,_phase.Pools)).ToList();
    }
}