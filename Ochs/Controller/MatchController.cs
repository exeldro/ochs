using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        [HttpGet]
        public MatchView GetNext(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var location = Request.GetOwinContext().Request.Cookies["location"];
                var match = session.QueryOver<Match>().Where(x => x.Id == id).SingleOrDefault();
                if (match == null && string.IsNullOrWhiteSpace(location))
                    return null;

                if (string.IsNullOrWhiteSpace(location) && match != null)
                {
                    location = match.GetLocation();
                }

                IList<Match> matchesTodo = new List<Match>();
                var noLocation = string.IsNullOrWhiteSpace(location);
                if (match?.Pool != null)
                {
                    matchesTodo = match?.Pool.Matches
                        .Where(x => !x.Started && x.Id != id && (noLocation || x.GetLocation() == location)).ToList();
                }

                if (match?.Phase != null && !matchesTodo.Any())
                {
                    matchesTodo = match?.Phase.Matches
                        .Where(x => !x.Started && x.Id != id && (noLocation || x.GetLocation() == location)).ToList();
                }

                if (match?.Competition != null && !matchesTodo.Any())
                {
                    matchesTodo = match?.Competition.Matches
                        .Where(x => !x.Started && x.Id != id && (noLocation || x.GetLocation() == location)).ToList();
                }

                var nextMatch = matchesTodo.Where(x => x.Planned).OrderBy(x => x.PlannedDateTime).FirstOrDefault() ??
                                matchesTodo.OrderBy(x => x.Name).FirstOrDefault();

                if (nextMatch == null)
                    return null;

                if (nextMatch.FighterBlue != null)
                    NHibernateUtil.Initialize(nextMatch.FighterBlue.Organizations);
                if (nextMatch.FighterRed != null)
                    NHibernateUtil.Initialize(nextMatch.FighterRed.Organizations);

                return new MatchView(nextMatch);
            }
        }


    }
}