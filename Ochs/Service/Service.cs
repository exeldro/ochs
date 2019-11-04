using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using NHibernate;

namespace Ochs
{
    public static class Service
    {
        private static readonly Dictionary<PhaseType,IPhaseTypeHandler> phaseTypeHandlers = new Dictionary<PhaseType, IPhaseTypeHandler>{{PhaseType.SingleRoundRobin, new SingleRoundRobinPhaseHandler()},{PhaseType.SingleElimination,new SingleEliminationPhaseHandler()}};
        public static IPhaseTypeHandler GetPhaseTypeHandler(PhaseType phaseType)
        {
            return phaseTypeHandlers.ContainsKey(phaseType) ? phaseTypeHandlers[phaseType] : null;
        }

        public static IList<Person> SortFightersByRanking(ISession session, IList<Person> fighters, Phase previousPhase, IList<CompetitionFighter> competitionFighters)
        {
            var sortedFighters = new List<Person>();
            if (previousPhase != null)
            {
                var rankings = session.QueryOver<PhaseRanking>().Where(x => x.Phase == previousPhase && x.Rank != null).OrderBy(x => x.Rank).Asc.List();
                var groupedRankings = rankings.GroupBy(x => x.Rank).OrderBy(x=>x.Key).Select(grp => grp.ToList()).ToList();
                foreach (var groupedRanking  in groupedRankings)
                {
                    IList<Person> groupedFighters = fighters.Where(x => groupedRanking.Any(y => y.Person.Id == x.Id)).ToList();
                    if (groupedFighters.Count > 1) // multiple fighters with same rank, sort them by previous phase
                    {
                        groupedFighters = SortFightersByRanking(session, groupedFighters, GetPreviousPhase(previousPhase), competitionFighters);
                    }
                    foreach (var fighter in groupedFighters)
                    {
                        sortedFighters.Add(fighter);
                    }
                }
            }
            foreach (var competitionFighter in competitionFighters.Where(x=>x.Seed != null).OrderBy(x=>x.Seed))
            {
                var fighter = fighters.SingleOrDefault(x => x.Id == competitionFighter.Fighter.Id);
                if (fighter != null && sortedFighters.All(x => x.Id != fighter.Id))
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
    }
}