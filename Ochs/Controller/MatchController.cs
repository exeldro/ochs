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
                return session.QueryOver<Match>().List().Select(x =>
                {
                    NHibernateUtil.Initialize(x.FighterBlue?.Organizations);
                    NHibernateUtil.Initialize(x.FighterRed?.Organizations);
                    NHibernateUtil.Initialize(x.Competition?.Organization);
                    NHibernateUtil.Initialize(x.Phase);
                    NHibernateUtil.Initialize(x.Pool);
                    return new MatchView(x);
                }).ToList();
            }
        }

        [HttpGet]
        public MatchDetailView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var match = session.QueryOver<Match>().Where(x => x.Id == id).Fetch(x => x.Events).Eager.TransformUsing(Transformers.DistinctRootEntity).SingleOrDefault();
                if(match == null)
                    return null;
                InitializeMatch(match);
                return new MatchDetailView(match);
            }
        }

        private static void InitializeMatch(Match match)
        {
            NHibernateUtil.Initialize(match.FighterBlue);
            NHibernateUtil.Initialize(match.FighterBlue?.Organizations);
            NHibernateUtil.Initialize(match.FighterRed);
            NHibernateUtil.Initialize(match.FighterRed?.Organizations);
            NHibernateUtil.Initialize(match.Competition);
            NHibernateUtil.Initialize(match.Competition?.Organization);
            NHibernateUtil.Initialize(match.Phase);
            NHibernateUtil.Initialize(match.Pool);
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

                InitializeMatch(nextMatch);

                return new MatchView(nextMatch);
            }
        }

        [HttpGet]
        public IList<MatchRules> GetRules()
        {
            return new List<MatchRules>();
        }

        [HttpGet]
        public MatchRules GetRules(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var match = session.QueryOver<Match>().Where(x => x.Id == id).SingleOrDefault();
                return match?.GetRules()??new MatchRules();
            }
        }
    }
}