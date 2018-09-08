using System;
using System.Collections.Generic;
using NHibernate;

namespace Ochs
{
    public interface IPhaseTypeHandler
    {
        PhaseType PhaseType { get; }

        IList<Match> GenerateMatches(int fighterCount, Phase phase, Pool pool);
        void AssignFightersToMatches(IList<Match> matches, IList<Person> sortedByRankFighters);
        IList<IList<Match>> GetMatchesPerRound(IList<Match> matches);
        IList<Match> UpdateMatchesAfterFinishedMatch(Match match, IList<Match> matches);
    }
}