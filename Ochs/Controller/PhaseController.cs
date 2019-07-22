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
                foreach (var person in phase.Fighters)
                {
                    NHibernateUtil.Initialize(person.Organizations);
                }
                NHibernateUtil.Initialize(phase.Competition);
                foreach (var match in phase.Matches)
                {
                    NHibernateUtil.Initialize(match.FighterBlue?.Organizations);
                    NHibernateUtil.Initialize(match.FighterRed?.Organizations);
                    NHibernateUtil.Initialize(match.Competition.Organization);
                    NHibernateUtil.Initialize(match.Pool);
                }
                foreach (var phasePool in phase.Pools)
                {
                    NHibernateUtil.Initialize(phasePool.Fighters);
                    NHibernateUtil.Initialize(phasePool.Matches);
                }

                var view = new PhaseDetailView(phase);
                NHibernateUtil.Initialize(view.MatchRules);
                return view;
            }
        }

        public IList<RankingView> GetRanking(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var rankings = session.QueryOver<PhaseRanking>().Where(x => x.Phase.Id == id).List();
                foreach (var ranking in rankings)
                {
                    if (ranking.Person != null)
                        NHibernateUtil.Initialize(ranking.Person.Organizations);
                }
                return rankings.Select(x => new RankingView(x)).OrderBy(x => x.Disqualified).ThenBy(x => x.Rank).ToList();
            }
        }

        [HttpGet]
        public MatchRules GetRules(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.QueryOver<Phase>().Where(x => x.Id == id).SingleOrDefault();
                var rules = phase.MatchRules ?? phase.Competition?.MatchRules;
                NHibernateUtil.Initialize(rules);
                rules = (MatchRules)session.GetSessionImplementation().PersistenceContext.Unproxy(rules);
                return rules ?? new MatchRules();
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
                    NHibernateUtil.Initialize(match.Competition.Organization);
                    NHibernateUtil.Initialize(match.Pool);
                    fighterViews.Add(match.FighterBlue == null ? null : new PersonView(match.FighterBlue));
                    fighterViews.Add(match.FighterRed == null ? null : new PersonView(match.FighterRed));
                }
                var matchViewsPerRound = new List<IList<MatchView>>();
                foreach (var matches in matchesPerRound)
                {
                    matchViewsPerRound.Add(matches.Select(x => new MatchView(x)).ToList());
                }
                return new BracketView { Fighters = fighterViews, Matches = matchViewsPerRound };
            }
        }
    }
}