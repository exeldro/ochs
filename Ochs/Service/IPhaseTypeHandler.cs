using System;
using System.Collections.Generic;
using NHibernate;

namespace Ochs
{
    public interface IPhaseTypeHandler
    {
        PhaseType PhaseType { get; }

        IList<Match> GenerateMatches(int fighterCount, Phase phase, Pool pool = null, IList<Match> oldMatches = null);
        void AssignFightersToMatches(IList<Match> newMatches, IList<Person> sortedByRankFighters, IList<Match> oldMatches = null);
        IList<IList<Match>> GetMatchesPerRound(IList<Match> matches);
        IList<Match> UpdateMatchesAfterFinishedMatch(Match match, IList<Match> matches);
        bool AllowedToGenerateMatches(IList<Match> matches, int fighterCount, Phase phase, Pool pool = null);
    }
}