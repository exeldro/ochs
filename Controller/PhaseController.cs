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

                var fighters = phase.Fighters;
                var roundCount = 0;
                while (2<<roundCount < fighters.Count)
                    roundCount++;

                var fighterViews = new List<PersonView>();
                NHibernateUtil.Initialize(fighters[0].Organizations);
                fighterViews.Add(new PersonView(fighters[0]));
                NHibernateUtil.Initialize(fighters[1].Organizations);
                fighterViews.Add(new PersonView(fighters[1]));
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
                        PersonView fighter = null;
                        if (fighters.Count > fighterIndex)
                        {
                            NHibernateUtil.Initialize(fighters[fighterIndex].Organizations);
                            fighter = new PersonView(fighters[fighterIndex]);
                        }
                        if (back)
                        {
                            fighterViews.Insert(fighterViews.Count-index, fighter);
                        }
                        else
                        {
                            fighterViews.Insert(index, fighter);
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
                //phase.Fighters
                return new BracketView{Fighters = fighterViews};
            }
        }
    }
}