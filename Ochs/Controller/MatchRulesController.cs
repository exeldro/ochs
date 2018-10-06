using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using NHibernate.Transform;

namespace Ochs
{
    public class MatchRulesController : ApiController
    {
        [HttpGet]
        public MatchRules Get()
        {
            return new MatchRules();
        }

        [HttpGet]
        public IList<MatchRules> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<MatchRules>().List();
            }
        }

        [HttpGet]
        public MatchRules Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var matchRules = session.QueryOver<MatchRules>().Where(x => x.Id == id).SingleOrDefault();
                return matchRules??new MatchRules();
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public MatchRules Save([FromBody]MatchRules matchRules)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(matchRules);
                    transaction.Commit();
                }
                return matchRules;
            }
        }
    }
}