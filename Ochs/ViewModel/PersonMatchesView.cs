using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class PersonMatchesView : PersonView
    {
        public PersonMatchesView(Person person, IList<Match> matches) : base(person)
        {
            matches = matches.Where(x => x.FighterBlue?.Id == person.Id || x.FighterRed?.Id == person.Id).ToList();
            MatchesTotal = matches.Count;
            MatchesPlanned = matches.Count(x => x.Planned);
            MatchesTodo = matches.Count(x => !x.Started);
            MatchesFinished = matches.Count(x => x.Finished);
            MatchesStarted = matches.Count(x => x.Started);
            MatchesBusy = matches.Count(x => x.Started && !x.Finished);
        }
        public int MatchesBusy { get; }
        public int MatchesStarted { get; }
        public int MatchesFinished { get; }
        public int MatchesTotal { get; }
        public int MatchesTodo { get; }
        public int MatchesPlanned { get; }
    }
}