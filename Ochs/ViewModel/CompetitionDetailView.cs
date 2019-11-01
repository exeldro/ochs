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

        public virtual IList<MatchView> Matches => _competition.Matches.OrderBy(x => x.PlannedDateTime).ThenBy(x => x.Name).Select(x => new MatchView(x)).ToList();
        public virtual IList<PhaseView> Phases => _competition.Phases.Select(x => new PhaseView(x)).ToList();
        public virtual IList<PersonMatchesView> Fighters => _competition.Fighters.Select(x => new PersonMatchesView(x,_competition.Matches)).ToList();        
        public virtual int MatchesTotal => _competition.Matches.Count;
        public virtual int FightersTotal => _competition.Fighters.Count;
        public virtual int PhasesTotal => _competition.Phases.Count;
        public virtual string MatchRules => _competition.MatchRules?.Name;

        public virtual IList<OrganizationView> FighterOrganizations => _competition.Fighters.SelectMany(x => x.Organizations)
            .Distinct().Select(x => new OrganizationView(x)).ToList();
    }
}