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
    }
}