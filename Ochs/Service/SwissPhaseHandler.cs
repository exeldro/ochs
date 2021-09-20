using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Utils;

namespace Ochs
{
    public class SwissPhaseHandler : IPhaseTypeHandler
    {
        public PhaseType PhaseType => PhaseType.Swiss;
        public IList<Match> GenerateMatches(int fighterCount, Phase phase, Pool pool, IList<Match> oldMatches)
        {
            var matches = new List<Match>();
            var matchCount = fighterCount >> 1;
            var matchCounter = 1;
            var round = 1;
            if (oldMatches != null && oldMatches.Any())
            {
                round = oldMatches.Count / (fighterCount / 2)+ 1;
            }

            for (var i = 1; i <= matchCount; i++)
            {
                var match = new Match
                {
                    Name = "Round "+ round.ToString().PadLeft(2) + " Match " + matchCounter.ToString().PadLeft(4),
                    Competition = phase?.Competition,
                    Phase = phase,
                    Pool = pool
                };
                matches.Add(match);
                matchCounter++;
            }
            return matches;
        }

        public void AssignFightersToMatches(IList<Match> newMatches, IList<Person> sortedByRankFighters, IList<Match> oldMatches)
        {
            if (oldMatches!= null && oldMatches.Any())
            {
                bool reverse = false;
                int start = 1;
                int step = 2;
                IList<Person> sortedByRankAndMatchesFighters = sortedByRankFighters.ToList();
                if (sortedByRankAndMatchesFighters.Count % 2 == 1)
                {
                    //make sure fighters get max amount of matches
                    //remove random fighter with max amount of matches
                    var groupedByMatchCount = sortedByRankAndMatchesFighters
                        .GroupBy(x => oldMatches.Count(y => y.FighterBlue == x || y.FighterRed == x))
                        .OrderBy(x => x.Key);
                    var fightersMostMatches = groupedByMatchCount.Last().ToList();
                    var random = new Random();
                    var index = random.Next(fightersMostMatches.Count);
                    sortedByRankAndMatchesFighters.Remove(fightersMostMatches[index]);
                }
                var fightersTodo = sortedByRankAndMatchesFighters.ToList();
                while (!TryAssignFighters(fightersTodo, newMatches, oldMatches))
                {
                    fightersTodo = reverse ? sortedByRankAndMatchesFighters.Reverse().ToList() : sortedByRankAndMatchesFighters.ToList();
                    for (var i = start; i < fightersTodo.Count; i += step)
                    {
                        (fightersTodo[i - (step-1)], fightersTodo[i]) = (fightersTodo[i], fightersTodo[i - (step-1)]);
                    }

                    if (reverse)
                    {
                        if (start < step)
                        {
                            start++;
                        }
                        else if(step < fightersTodo.Count)
                        {
                            start = step;
                            step++;
                        }
                        else
                        {
                            break;
                        }
                        reverse = false;
                    }
                    else
                    {
                        reverse = true;
                    }
                }
            }
            else
            {
                var fighterCount = sortedByRankFighters.Count;
                var halfFighters = fighterCount >> 1; 
                for (var i = 0; i < newMatches.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        newMatches[i].FighterRed = sortedByRankFighters[fighterCount - (i + 1)];
                        newMatches[i].FighterBlue = sortedByRankFighters[halfFighters - (i + 1)];
                    }
                    else
                    {
                        newMatches[i].FighterBlue = sortedByRankFighters[fighterCount - (i + 1)];
                        newMatches[i].FighterRed = sortedByRankFighters[halfFighters - (i + 1)];
                    }
                }
            }
        }

        private bool TryAssignFighters(List<Person> fightersTodo, IList<Match> newMatches, IList<Match> oldMatches)
        {
            bool succes = true;
            foreach (var newMatch in newMatches)
            {
                newMatch.FighterRed = fightersTodo.First();
                fightersTodo.Remove(newMatch.FighterRed);

                newMatch.FighterBlue = fightersTodo.FirstOrDefault(x => !oldMatches.Any(y =>
                    (y.FighterRed == x && y.FighterBlue == newMatch.FighterRed) ||
                    (y.FighterBlue == x && y.FighterRed == newMatch.FighterRed)));
                if (newMatch.FighterBlue == null)
                {
                    succes = false;
                    break;
                }
                fightersTodo.Remove(newMatch.FighterBlue);
            }
            return succes;
        }

        public IList<IList<Match>> GetMatchesPerRound(IList<Match> matches)
        {
            return new List<IList<Match>>{matches};
        }

        public IList<Match> UpdateMatchesAfterFinishedMatch(Match match, IList<Match> matches)
        {
            return new List<Match>();
        }

        public bool AllowedToGenerateMatches(IList<Match> matches, int fighterCount, Phase phase, Pool pool) =>
            matches.All(x => x.Finished) && matches.Count / (fighterCount / 2) * 2 < fighterCount;
    }
}