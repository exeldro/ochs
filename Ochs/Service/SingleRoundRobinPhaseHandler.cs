using System.Collections.Generic;
using System.Linq;
using NHibernate;

namespace Ochs
{
    public class SingleRoundRobinPhaseHandler : IPhaseTypeHandler
    {
        public PhaseType PhaseType => PhaseType.SingleRoundRobin;

        public IList<Match> GenerateMatches(int fighterCount, Phase phase, Pool pool = null, IList<Match> oldMatches = null)
        {
            var matches = new List<Match>();
            var matchCounter = 1;

            for (int i = 0; i < fighterCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    var match = new Match
                    {
                        Name = "Match " + matchCounter.ToString().PadLeft(4),
                        Competition = phase?.Competition,
                        Phase = phase,
                        Pool = pool
                    };
                    matches.Add(match);
                    matchCounter++;
                }
            }
            return matches;
        }

        public void AssignFightersToMatches(IList<Match> matches, IList<Person> fighters, IList<Match> oldMatches = null)
        {
            matches = matches.OrderByDescending(y => y.Name).ToList();

            for (var matchCounter = 0; matchCounter < matches.Count; matchCounter++)
            {
                Person fighterBlue = null;

                if (fighters.Count == 8)
                {
                    fighterBlue = fighters
                        .OrderBy(x => MatchCount(x, matches))
                        .ThenBy(x => BlueCount(x, matches))
                        .ThenBy(x => matches.LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id) != null)
                        .ThenByDescending(x => matches.LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id)?.Name)
                        .First();
                }
                else
                {

                    fighterBlue = fighters
                        .OrderBy(x => matches.LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id) != null)
                        .ThenByDescending(x => matches.LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id)?.Name)
                        .ThenBy(x => MatchCount(x, matches))
                        .ThenBy(x => BlueCount(x, matches))
                        .First();
                }

                var fighterRed = fighters.Where(x =>
                        x.Id != fighterBlue.Id && !matches.Any(y =>
                            (y.FighterBlue?.Id == fighterBlue.Id && y.FighterRed?.Id == x.Id) ||
                            (y.FighterRed?.Id == fighterBlue.Id && y.FighterBlue?.Id == x.Id)))
                    .OrderBy(x => matches.LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id) != null)
                    .ThenByDescending(x => matches.LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id)?.Name)
                    .ThenBy(x => MatchCount(x, matches))
                    .ThenBy(x => RedCount(x, matches))
                    .First();
                var match = matches[matchCounter];
                match.FighterBlue = fighterBlue;
                match.FighterRed = fighterRed;
            }
        }


        private int MatchCount(Person person, IList<Match> matches) => matches.Count(y => y.FighterBlue?.Id == person.Id || y.FighterRed?.Id == person.Id);
        private int BlueCount(Person person, IList<Match> matches) => matches.Count(y => y.FighterBlue?.Id == person.Id);
        private int RedCount(Person person, IList<Match> matches) => matches.Count(y => y.FighterRed?.Id == person.Id);

        public IList<IList<Match>> GetMatchesPerRound(IList<Match> matches)
        {
            return new List<IList<Match>> { matches };
        }

        public IList<Match> UpdateMatchesAfterFinishedMatch(Match match, IList<Match> matches)
        {
            return new List<Match>();
        }

        public bool AllowedToGenerateMatches(IList<Match> matches, int fighterCount, Phase phase, Pool pool) => !matches.Any();
    }
}