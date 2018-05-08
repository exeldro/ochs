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

                var fighters = Service.SortFightersByRanking(session, phase.Fighters, Service.GetPreviousPhase(phase));
                var fighterViews = Service.SingleEliminationMatchedFighters(fighters).Select(x=>
                {
                    NHibernateUtil.Initialize(x.Organizations);
                    return new PersonView(x);
                }).ToList();

                //TODO set BracketView.Matches
                
                return new BracketView{Fighters = fighterViews};
            }
        }
    }
}