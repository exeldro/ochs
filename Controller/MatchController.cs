using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate;
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
                var match = session.QueryOver<Match>().Where(x => x.Id == id).Fetch(x => x.Events).Eager.TransformUsing(Transformers.DistinctRootEntity).SingleOrDefault();
                if(match == null)
                    return null;

                if(match.FighterBlue != null)
                    NHibernateUtil.Initialize(match.FighterBlue.Organizations);
                if(match.FighterRed != null)
                    NHibernateUtil.Initialize(match.FighterRed.Organizations);

                return new MatchWithEventsView(match);
            }
        }
    }
}