using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using NHibernate.Transform;

namespace Ochs
{
    public class RankingRulesController : ApiController
    {
        [HttpGet]
        public RankingRules Get()
        {
            return new RankingRules{Sorting = RankingRules.DefaultSorting};
        }

        [HttpGet]
        public IList<RankingRules> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<RankingRules>().Fetch(x=>x.Sorting).Eager.TransformUsing(Transformers.DistinctRootEntity).List();
            }
        }

        [HttpGet]
        public RankingRules Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var rankingRules = session.QueryOver<RankingRules>().Where(x => x.Id == id).Fetch(x=>x.Sorting).Eager.TransformUsing(Transformers.DistinctRootEntity).SingleOrDefault();
                return rankingRules??new RankingRules{Sorting = RankingRules.DefaultSorting};
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public RankingRules Save([FromBody]RankingRules rankingRules)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(rankingRules);
                    transaction.Commit();
                }
                return rankingRules;
            }
        }
    }
}