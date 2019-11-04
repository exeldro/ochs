using System.Collections.Generic;

namespace Ochs
{
    public class CompetitionFighterView : PersonMatchesView
    {
        private readonly CompetitionFighter _competitionFighter;

        public CompetitionFighterView(CompetitionFighter competitionFighter, IList<Match> matches): base(competitionFighter.Fighter, matches)
        {
            _competitionFighter = competitionFighter;
        }
        public virtual double? Seed => _competitionFighter.Seed;
    }
}