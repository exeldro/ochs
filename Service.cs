using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using NHibernate;

namespace Ochs
{
    public static class Service
    {
        private static readonly string[] roundNames = {"Final","Semifinals","Quarterfinals","Eighth-finals","16th-finals","32nd-finals", "64th-finals"};
        private static readonly string thirdPlaceMatchName = "Third place match";
        public static string GetMatchName(int round, int matchNumber)
        {
            if (round == 0)
            {
                return matchNumber == 2 ? " "+thirdPlaceMatchName  : roundNames[0];
            }
            return Strings.Space(round) + roundNames[round] + " match " + matchNumber.ToString().PadLeft(3);
        }

        public static IList<Person> SingleEliminationMatchedFighters(IList<Person> fighters)
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
                var index =  1;
                var back = false;
                var front = false;
                for (var addIndex = 0; addIndex < fightersToAdd; addIndex++)
                {
                    var fighterIndex = fightersToAdd + (addIndex>>1);
                    if (addIndex % 2 == (front?0:1))
                    {
                        fighterIndex = fightersToAdd + fightersToAdd - ((addIndex>>1) + 1);
                    }
                    Person fighter = null;
                    if (fighters.Count > fighterIndex)
                    {
                        fighter = fighters[fighterIndex];
                    }
                    if (back)
                    {
                        matchedFighters.Insert(matchedFighters.Count-index, fighter);
                    }
                    else
                    {
                        matchedFighters.Insert(index, fighter);
                    }

                    if (addIndex % 4 == 1)
                    {
                        back = !back;
                    }
                    if (addIndex % 4 == 3)
                    {
                        index += 4;
                        front = !front;
                    }
                }
            }
            return matchedFighters;
        }

        public static IList<Person> SortFightersByRanking(ISession session, IList<Person> fighters, Phase previousPhase)
        {
            if (previousPhase == null)
                return fighters;
            var rankings = session.QueryOver<PhaseRanking>().Where(x => x.Phase == previousPhase && x.Rank != null).OrderBy(x=>x.Rank).Asc.List();
            var sortedFighters = new List<Person>();
            foreach (var ranking in rankings)
            {
                var fighter = fighters.SingleOrDefault(x => x.Id == ranking.Person.Id);
                if (fighter != null)
                {
                    sortedFighters.Add(fighter);
                }
            }
            foreach (var fighter in fighters)
            {
                if (sortedFighters.All(x => x.Id != fighter.Id))
                {
                    sortedFighters.Add(fighter);
                }
            }
            return sortedFighters;
        }
        public static Phase GetPreviousPhase(Phase phase)
        {
            return phase.Competition.Phases.Where(x => x.PhaseOrder < phase.PhaseOrder)
                .OrderBy(x => x.PhaseOrder).LastOrDefault();
        }

        public static int GetRound(string matchName)
        {
            if (matchName == thirdPlaceMatchName)
                return 0;
            for (var round = 0; round < roundNames.Length; round++)
            {
                if (matchName.StartsWith(roundNames[round]))
                    return round;
            }
            return -1;
        }

        public static int GetMatchNumber(string matchName, int round)
        {
            if (round == 0)
            {
                if (matchName == roundNames[round])
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            return int.Parse(matchName.Replace(roundNames[round] + " match ", ""));

        }
    }
}