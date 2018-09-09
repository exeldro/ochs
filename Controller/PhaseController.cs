using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate;

namespace Ochs
{
    public class PhaseController : ApiController
    {

        [HttpGet]
        public PhaseDetailView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.QueryOver<Phase>().Where(x => x.Id == id).SingleOrDefault();
                if (phase == null)
                    return null;
                NHibernateUtil.Initialize(phase.Fighters);
                foreach (var person in phase.Fighters)
                {
                    NHibernateUtil.Initialize(person.Organizations);
                }
                NHibernateUtil.Initialize(phase.Matches);
                NHibernateUtil.Initialize(phase.Pools);
                foreach (var phasePool in phase.Pools)
                {
                    NHibernateUtil.Initialize(phasePool.Fighters);
                    NHibernateUtil.Initialize(phasePool.Matches);
                }
                return new PhaseDetailView(phase);
            }
        }

        public IList<RankingView> GetRanking(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var rankings = session.QueryOver<PhaseRanking>().Where(x => x.Phase.Id == id).List();
                foreach (var ranking in rankings)
                {
                    if(ranking.Person != null)
                        NHibernateUtil.Initialize(ranking.Person.Organizations);
                }
                return rankings.Select(x => new RankingView(x)).OrderBy(x=>x.Disqualified).ThenBy(x=>x.Rank).ToList();
            }
        }
        public BracketView GetElimination(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.QueryOver<Phase>().Where(x => x.Id == id).SingleOrDefault();
                if (phase == null)
                    return null;

                var phaseTypeHandler = Service.GetPhaseTypeHandler(phase.PhaseType);
                var matchesPerRound = phaseTypeHandler.GetMatchesPerRound(phase.Matches);
                var fighterViews = new List<PersonView>();
                foreach (var match in matchesPerRound.First())
                {
                    NHibernateUtil.Initialize(match.FighterBlue?.Organizations);
                    NHibernateUtil.Initialize(match.FighterRed?.Organizations);
                    fighterViews.Add(match.FighterBlue == null ? null : new PersonView(match.FighterBlue));
                    fighterViews.Add(match.FighterRed == null ? null : new PersonView(match.FighterRed));
                }
                var matchViewsPerRound = new List<IList<MatchView>>();
                foreach (var matches in matchesPerRound)
                {
                    matchViewsPerRound.Add(matches.Select(x=> new MatchView(x)).ToList());
                }
                return new BracketView{Fighters = fighterViews, Matches = matchViewsPerRound};
            }
        }
    }
}