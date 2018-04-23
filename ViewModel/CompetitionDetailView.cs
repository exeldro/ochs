using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class CompetitionDetailView : CompetitionView
    {
        public CompetitionDetailView(Competition competition) : base(competition)
        {

        }

        public virtual int MatchesPlanned => _competition.Matches.Count(x => x.Planned);
        public virtual int MatchesTodo => _competition.Matches.Count(x => !x.Started);
        public virtual int MatchesFinished => _competition.Matches.Count(x => x.Finished);
        public virtual int MatchesStarted => _competition.Matches.Count(x => x.Started);
        public virtual int MatchesBusy => _competition.Matches.Count(x => x.Started && !x.Finished);

        public IList<MatchView> Matches => _competition.Matches.Select(x => new MatchView(x)).ToList();
        public IList<PhaseView> Phases => _competition.Phases.Select(x => new PhaseView(x)).ToList();
        public IList<PersonView> Fighters => _competition.Fighters.Select(x => new PersonView(x)).ToList();
    }
}