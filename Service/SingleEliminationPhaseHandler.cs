using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;

namespace Ochs
{
    public class SingleEliminationPhaseHandler : IPhaseTypeHandler
    {
        public PhaseType PhaseType => PhaseType.SingleElimination;
        public IList<Match> GenerateMatches(int fighterCount, Phase phase, Pool pool)
        {
            var matches = new List<Match>();
            var roundCount = 0;
            while (2<<roundCount < fighterCount)
                roundCount++;

            for (var round = roundCount; round > 0; round--)
            {
                for (var matchNumber = 1; matchNumber <= (1 << round); matchNumber++)
                {
                    var match = new Match
                    {
                        Name = EliminationRoundNames.GetMatchName(round, matchNumber),
                        Competition = phase?.Competition,
                        Phase = phase,
                        Pool = pool
                    };
                    matches.Add(match);
                }
            }
            var finalMatch = new Match
            {
                Name = EliminationRoundNames.GetMatchName(0, 2),
                Competition = phase?.Competition,
                Phase = phase,
                Pool = pool
            };
            matches.Add(finalMatch);

            finalMatch = new Match
            {
                Name = EliminationRoundNames.GetMatchName(0, 1),
                Competition = phase?.Competition,
                Phase = phase,
                Pool = pool
            };
            matches.Add(finalMatch);
            return matches;
        }

        public void AssignFightersToMatches(IList<Match> matches, IList<Person> sortedByRankFighters)
        {
            var matchedFighters = SingleEliminationMatchedFighters(sortedByRankFighters);
            
            var roundCount = 0;
            while (2<<roundCount < sortedByRankFighters.Count)
                roundCount++;

            for (var i = 0; i < matchedFighters.Count - 1; i += 2)
            {
                var matchNumber = (i >> 1) +1;
                var matchName = EliminationRoundNames.GetMatchName(roundCount, matchNumber).Trim();
                var match = matches.SingleOrDefault(x => x.Name.Trim() == matchName);
                if (match != null)
                {
                    match.FighterBlue = matchedFighters[i];
                    match.FighterRed = matchedFighters[i + 1];
                }
                if (matchedFighters[i] == null || matchedFighters[i + 1] == null)
                {
                    if (match != null)
                    {
                        match.Result = MatchResult.Skipped;
                        match.StartedDateTime = DateTime.Now;
                        match.FinishedDateTime = DateTime.Now;
                    }
                    var fighter = matchedFighters[i] ?? matchedFighters[i + 1];
                    if(fighter == null)
                        continue;
                    matchName = EliminationRoundNames.GetMatchName(roundCount-1, (i>>2)+1).Trim();
                    match = matches.SingleOrDefault(x => x.Name.Trim() == matchName);
                    if(match == null)
                        continue;
                    if (matchNumber % 2 == 1)
                    {
                        match.FighterBlue = fighter;
                    }
                    else
                    {
                        match.FighterRed = fighter;
                    }
                    continue;
                }
            }
        }

        public IList<IList<Match>> GetMatchesPerRound(IList<Match> allMatches)
        {
            var matchesPerRound = new List<IList<Match>>();
            var matches = new List<Match>();
            var round = -1;
            foreach (var match in allMatches)
            {
                var matchRound = EliminationRoundNames.GetRound(match.Name);
                if (matchRound != -1 && matchRound != round)
                {
                    round = matchRound;
                    if (matches.Any())
                    {
                        matchesPerRound.Add(matches);
                        matches = new List<Match>();
                    }
                }
                matches.Add(match);
            }
            if(matches.Any())
                matchesPerRound.Add(matches);
            return matchesPerRound;
        }

        public IList<Match> UpdateMatchesAfterFinishedMatch(Match match, IList<Match> matches)
        {
            var updatedMatches = new List<Match>();
            Person winner = null;
            Person loser = null;
            if (match.Result == MatchResult.WinBlue || match.Result == MatchResult.DisqualificationRed ||
                match.Result == MatchResult.ForfeitRed)
            {
                winner = match.FighterBlue;
                if (match.Result == MatchResult.WinBlue)
                    loser = match.FighterRed;
            }
            else if (match.Result == MatchResult.WinRed || match.Result == MatchResult.DisqualificationBlue ||
                     match.Result == MatchResult.ForfeitBlue)
            {
                winner = match.FighterRed;
                if (match.Result == MatchResult.WinRed)
                    loser = match.FighterBlue;
            }

            var round = EliminationRoundNames.GetRound(match.Name);
            if (round > 0 && winner != null)
            {
                var matchNumber = EliminationRoundNames.GetMatchNumber(match.Name, round);
                var nextRound = round - 1;
                var nextMatchNumber = ((matchNumber - 1) >> 1) + 1;
                var nextMatchName = EliminationRoundNames.GetMatchName(nextRound, nextMatchNumber).Trim();
                var nextMatch = matches.SingleOrDefault(x => x.Name.Trim() == nextMatchName);
                if (nextMatch != null)
                {
                    if (matchNumber % 2 == 1)
                    {
                        nextMatch.FighterBlue = winner;
                    }
                    else
                    {
                        nextMatch.FighterRed = winner;
                    }
                    updatedMatches.Add(nextMatch);
                }

                if (round == 1 && loser != null)
                {
                    nextMatchName = EliminationRoundNames.GetMatchName(0, 2).Trim();
                    nextMatch = matches.SingleOrDefault(x => x.Name.Trim() == nextMatchName);
                    if (nextMatch != null)
                    {
                        if (matchNumber % 2 == 1)
                        {
                            nextMatch.FighterBlue = loser;
                        }
                        else
                        {
                            nextMatch.FighterRed = loser;
                        }
                        updatedMatches.Add(nextMatch);
                    }
                }
            }
            return updatedMatches;
        }

        private static IList<Person> SingleEliminationMatchedFighters(IList<Person> fighters)
        {
            if (fighters.Count < 2)
                return fighters;
            var roundCount = 0;
            while (2<<roundCount < fighters.Count)
                roundCount++;
            var matchedFighters = new List<Person>();
            matchedFighters.Add(fighters[0]);
            matchedFighters.Add(fighters[1]);
            for (var round = 1; round <= roundCount; round++)
            {
                var fightersToAdd = 1 << round;
                for (var addIndex = 0; addIndex < fightersToAdd; addIndex++)
                {
                    var oldFighterIndex = fighters.IndexOf(matchedFighters[addIndex<<1]);
                    var newFighterIndex = (fightersToAdd<<1) - (oldFighterIndex +1 );
                    Person fighter = null;
                    if (newFighterIndex < fighters.Count)
                    {
                        fighter = fighters[newFighterIndex];
                    }
                    if (addIndex % 2 == 1)
                    {
                        //before
                        matchedFighters.Insert(addIndex << 1, fighter);
                    }
                    else
                    {
                        //after
                        matchedFighters.Insert((addIndex << 1) + 1, fighter);
                    }
                }
            }
            return matchedFighters;
        }
    }
}