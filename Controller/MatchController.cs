using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate.Transform;

namespace Ochs
{
    public class MatchController : ApiController
    {
        [HttpGet]
        public IList<MatchView> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Match>().List().Select(x => new MatchView(x)).ToList();
            }
        }

        [HttpGet]
        public MatchWithEventsView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Match>().Where(x => x.Id == id).Fetch(x => x.Events).Eager.TransformUsing(Transformers.DistinctRootEntity).List().Select(x => new MatchWithEventsView(x)).SingleOrDefault();
            }
        }
    }
}