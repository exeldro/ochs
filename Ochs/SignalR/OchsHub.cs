using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.SignalR;
using NHibernate;
using NHibernate.Criterion;

namespace Ochs
{
    public class OchsHub : Hub
    {
        public void GetCurrentUser()
        {
            var i = Context.Request.User?.Identity as ClaimsIdentity;
            if (i == null)
            {
                Clients.Caller.authorizationException(false);
                return;
            }
            var idstring = i.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (idstring == null)
            {
                Clients.Caller.authorizationException(false);
                return;
            }
            var id = new Guid(idstring);
            using (var session = NHibernateHelper.OpenSession())
            {
                Clients.Caller.updateUser(session.QueryOver<User>().Where(x => x.Id == id).SingleOrDefault());
            }
        }

        public void AddMatchEvent(Guid matchGuid, int pointsBlue, int pointsRed, MatchEventType eventType, string note = null)
        {
            //Context.Request.User
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match == null)
                    {
                        Clients.Caller.displayMessage("Match not found", "warning");
                        return;
                    }

                    if (match.Finished || match.Validated)
                    {
                        Clients.Caller.displayMessage("Match finished", "warning");
                        return;
                    }

                    if (!HasMatchEditRights(session, match))
                        return;

                    //check for doubleclick
                    if (match.Events.Any(x => DateTime.Now.Subtract(x.CreatedDateTime).TotalSeconds < 0.5))
                        return;

                    if (!match.StartedDateTime.HasValue)
                    {
                        match.StartedDateTime = DateTime.Now;
                        if (Context.Request.Cookies.ContainsKey("location") &&
                            !string.IsNullOrWhiteSpace(Context.Request.Cookies["location"].Value))
                        {
                            match.Location = Context.Request.Cookies["location"].Value;
                        }
                    }

                    match.Events.Add(new MatchEvent
                    {
                        CreatedDateTime = DateTime.Now,
                        PointsBlue = pointsBlue,
                        PointsRed = pointsRed,
                        MatchTime = match.LiveTime,
                        Round = match.Round,
                        Type = eventType,
                        Note = note,
                        Match = match
                    });
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        public void UpdateMatchEventNote(Guid matchGuid, Guid eventGuid, string note)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match == null)
                    {
                        Clients.Caller.displayMessage("Match not found", "warning");
                        return;
                    }
                    if (match.Finished || match.Validated)
                    {
                        Clients.Caller.displayMessage("Match finished", "warning");
                        return;
                    }
                    if (!match.Events.Any())
                    {
                        Clients.Caller.displayMessage("No match events found", "warning");
                        return;
                    }

                    var eventToUpdate = match.Events.SingleOrDefault(x => x.Id == eventGuid);
                    if (eventToUpdate == null)
                    {
                        Clients.Caller.displayMessage("Match event not found", "warning");
                        return;
                    }

                    if (!HasMatchEditRights(session, match))
                        return;

                    eventToUpdate.Note = note;

                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        public void UndoLastMatchEvent(Guid matchGuid)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match == null)
                    {
                        Clients.Caller.displayMessage("Match not found", "warning");
                        return;
                    }
                    if (match.Finished || match.Validated)
                    {
                        Clients.Caller.displayMessage("Match finished", "warning");
                        return;
                    }
                    if (!match.Events.Any())
                    {
                        Clients.Caller.displayMessage("No match events found", "warning");
                        return;
                    }

                    var lastEvent = match.Events.OrderBy(x => x.CreatedDateTime).Last();
                    //if(lastEvent.CreatedDateTime < DateTime.Now.AddSeconds(-300))
                    //    return;

                    if (!HasMatchEditRights(session, match))
                        return;

                    match.Events.Remove(lastEvent);
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        public void DeleteMatchEvent(Guid matchGuid, Guid eventGuid)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match == null)
                    {
                        Clients.Caller.displayMessage("Match not found", "warning");
                        return;
                    }
                    if (match.Finished || match.Validated)
                    {
                        Clients.Caller.displayMessage("Match finished", "warning");
                        return;
                    }
                    if (!match.Events.Any())
                    {
                        Clients.Caller.displayMessage("No match events found", "warning");
                        return;
                    }

                    var deleteEvent = match.Events.SingleOrDefault(x => x.Id == eventGuid);
                    if (deleteEvent == null)
                    {
                        Clients.Caller.displayMessage("Match event not found", "warning");
                        return;
                    }

                    if (!HasMatchEditRights(session, match))
                        return;

                    match.Events.Remove(deleteEvent);
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        private bool HasMatchEditRights(ISession session, Match match)
        {
            if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
            {
                Clients.Caller.displayMessage("Not logged in as score keeper", "warning");
                return false;
            }

            if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
            {
                Clients.Caller.displayMessage("Match is finished and not logged in as score validator", "warning");
                return false;
            }

            if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
            {
                Clients.Caller.displayMessage("Match is validated and not logged in as administrator", "warning");
                return false;
            }

            return true;
        }


        private bool HasMatchRights(ISession session, Match match, UserRoles requiredRole)
        {
            return HasOrganizationRights(session, match.Competition.Organization, requiredRole);
        }

        private bool HasOrganizationRights(ISession session, Organization organization, UserRoles requiredRole)
        {
            var i = Context.Request.User?.Identity as ClaimsIdentity;
            if (i == null)
            {
                Clients.Caller.authorizationException(false);
                return false;
            }
            var idstring = i.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (idstring == null)
            {
                Clients.Caller.authorizationException(false);
                return false;
            }
            var id = new Guid(idstring);
            var roles = session.QueryOver<UserRole>().Where(x => x.User.Id == id).List();
            if (!roles.Any(x => (x.Role == requiredRole || x.Role == UserRoles.Admin) &&
                                (x.Organization == null || x.Organization == organization)))
            {
                Clients.Caller.authorizationException(true);
                return false;
            }
            return true;
        }

        public void StopTime(Guid matchGuid)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (!match.TimeRunningSince.HasValue)
                        return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                    {
                        Clients.Caller.displayMessage("Not logged in as score keeper", "warning");
                        return;
                    }

                    match.Time = match.LiveTime;
                    match.TimeRunningSince = null;
                    match.TimeOutSince = DateTime.Now;
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        public void StartTime(Guid matchGuid)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match.TimeRunningSince.HasValue)
                        return;
                    if (match.Finished)
                    {
                        Clients.Caller.displayMessage("Match finished", "warning");
                        return;
                    }

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                    {
                        Clients.Caller.displayMessage("Not logged in as score keeper", "warning");
                        return;
                    }

                    match.TimeOutSince = null;
                    match.TimeRunningSince = DateTime.Now;
                    if (!match.StartedDateTime.HasValue)
                    {
                        match.StartedDateTime = match.TimeRunningSince;
                        if (Context.Request.Cookies.ContainsKey("location") &&
                            !string.IsNullOrWhiteSpace(Context.Request.Cookies["location"].Value))
                        {
                            match.Location = Context.Request.Cookies["location"].Value;
                        }
                    }
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        public void SetTimeMilliSeconds(Guid matchGuid, long milliSeconds)
        {
            SetTime(matchGuid, new TimeSpan(milliSeconds * 10000));
        }

        public void SetTime(Guid matchGuid, TimeSpan time)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match.TimeRunningSince.HasValue)
                    {
                        Clients.Caller.displayMessage("can not set running time", "warning");
                        return;
                    }

                    if (!HasMatchEditRights(session, match))
                        return;

                    match.Time = time;
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchDetailView(match));
                }
            }
        }

        public void SetMatchResult(Guid matchGuid, MatchResult matchResult)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var match = session.Get<Match>(matchGuid);
                if (!HasMatchEditRights(session, match))
                    return;

                if (matchResult == MatchResult.None)
                {
                    if (!HasMatchRights(session, match, UserRoles.Admin))
                    {
                        Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                        return;
                    }

                    match.Result = matchResult;
                    if (match.FinishedDateTime.HasValue)
                    {
                        match.FinishedDateTime = null;
                    }
                }
                else
                {
                    if (matchResult == MatchResult.Skipped && (match.ScoreBlue != 0 || match.ScoreRed != 0 || match.Events.Count > 0))
                    {
                        Clients.Caller.displayMessage("Can't set match with events to skipped", "warning");
                        return;
                    }
                    if (!match.StartedDateTime.HasValue)
                    {
                        if (matchResult == MatchResult.Skipped || !HasMatchRights(session, match, UserRoles.Admin))
                        {
                            Clients.Caller.displayMessage("Match not started", "warning");
                            return;
                        }
                        match.StartedDateTime = DateTime.Now;
                    }

                    match.Result = matchResult;
                    if (!match.FinishedDateTime.HasValue)
                    {
                        match.FinishedDateTime = DateTime.Now;
                    }

                    if (match.TimeRunningSince.HasValue)
                    {
                        match.Time = match.LiveTime;
                        match.TimeRunningSince = null;
                    }
                    match.TimeOutSince = null;
                }
                using (var transaction = session.BeginTransaction())
                {
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                }
                Clients.All.updateMatch(new MatchDetailView(match));


                var phaseTypeHandler = (match.Phase == null ? null : Service.GetPhaseTypeHandler(match.Phase.PhaseType));
                if (phaseTypeHandler == null)
                    return;

                var updatedMatches =
                    phaseTypeHandler.UpdateMatchesAfterFinishedMatch(match,
                        match.Pool?.Matches ?? match.Phase?.Matches);
                foreach (var updatedMatch in updatedMatches)
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Update(updatedMatch);
                        transaction.Commit();
                    }

                    Clients.All.updateMatch(new MatchView(updatedMatch));
                }

                UpdateRankings(session, match);
            }
        }

        private void UpdateRankings(ISession session, Match match)
        {
            if (match.Pool != null)
            {
                UpdatePoolRankingsInternal(session, match.Pool);
            }

            if (match.Phase != null)
            {
                UpdatePhaseRankingsInternal(session, match.Phase);
            }
        }

        private void UpdatePhaseRankingsInternal(ISession session, Phase phase)
        {
            var rankings = session.QueryOver<PhaseRanking>().Where(x => x.Phase == phase).List().Cast<Ranking>().ToList();
            UpdateRankingsInternal(session, rankings, phase.Matches, () => new PhaseRanking { Phase = phase },
                Service.GetPhaseTypeHandler(phase.PhaseType), phase.Competition.RankingRules);
            Clients.All.updateRankings(phase.Id);
        }

        public void UpdatePhaseRankings(Guid phaseId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                UpdatePhaseRankingsInternal(session, phase);
            }
        }

        private void UpdatePoolRankingsInternal(ISession session, Pool pool)
        {
            var rankings = session.QueryOver<PoolRanking>().Where(x => x.Pool == pool).List().Cast<Ranking>().ToList();
            UpdateRankingsInternal(session, rankings, pool.Matches, () => new PoolRanking { Pool = pool }, Service.GetPhaseTypeHandler(pool.Phase.PhaseType), pool.Phase.Competition.RankingRules);
            Clients.All.updateRankings(pool.Id);
        }

        public void UpdatePoolRankings(Guid poolId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.Get<Pool>(poolId);
                if (pool == null)
                {
                    Clients.Caller.displayMessage("Pool not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                UpdatePoolRankingsInternal(session, pool);
            }
        }


        private void UpdateRankingsInternal(ISession session, IList<Ranking> oldRankings, IList<Match> matches, Func<Ranking> createRanking, IPhaseTypeHandler phaseTypeHandler, RankingRules rankingRules)
        {
            if (rankingRules == null)
                rankingRules = new RankingRules{Sorting = RankingRules.DefaultSorting};
            IList<Ranking> rankings = new List<Ranking>();

            //calc forfeited and disqualified
            foreach (var match in matches)
            {
                if (!match.Finished)
                    continue;
                if (match.Result == MatchResult.DisqualificationBlue)
                {
                    var rankingBlue = rankings.SingleOrDefault(x => x.Person == match.FighterBlue);
                    if (rankingBlue == null)
                    {
                        rankingBlue = createRanking();
                        rankingBlue.Person = match.FighterBlue;
                        rankings.Add(rankingBlue);
                    }
                    rankingBlue.Disqualified = true;
                }
                else if (match.Result == MatchResult.DisqualificationRed)
                {
                    var rankingRed = rankings.SingleOrDefault(x => x.Person == match.FighterRed);
                    if (rankingRed == null)
                    {
                        rankingRed = createRanking();
                        rankingRed.Person = match.FighterRed;
                        rankings.Add(rankingRed);
                    }
                    rankingRed.Disqualified = true;
                }
                else if (match.Result == MatchResult.ForfeitBlue)
                {
                    var rankingBlue = rankings.SingleOrDefault(x => x.Person == match.FighterBlue);
                    if (rankingBlue == null)
                    {
                        rankingBlue = createRanking();
                        rankingBlue.Person = match.FighterBlue;
                        rankings.Add(rankingBlue);
                    }
                    rankingBlue.Forfeited = true;
                }
                else if (match.Result == MatchResult.ForfeitRed)
                {
                    var rankingRed = rankings.SingleOrDefault(x => x.Person == match.FighterRed);
                    if (rankingRed == null)
                    {
                        rankingRed = createRanking();
                        rankingRed.Person = match.FighterRed;
                        rankings.Add(rankingRed);
                    }
                    rankingRed.Forfeited = true;
                }
            }
            var rankingPhaseTypeHandler = phaseTypeHandler as IRankingPhaseTypeHandler;
            //calc ranking stats
            foreach (var match in matches)
            {
                if (!match.Finished)
                    continue;
                var rankingBlue = rankings.SingleOrDefault(x => x.Person == match.FighterBlue);
                var rankingRed = rankings.SingleOrDefault(x => x.Person == match.FighterRed);
                if (rankingPhaseTypeHandler == null && rankingRules.RemoveDisqualifiedFromRanking && ((rankingRed?.Disqualified ?? false) || (rankingBlue?.Disqualified ?? false)))
                    continue;
                if (rankingPhaseTypeHandler == null && rankingRules.RemoveForfeitedFromRanking && ((rankingRed?.Forfeited ?? false) || (rankingBlue?.Forfeited ?? false)))
                    continue;
                if (rankingBlue == null)
                {
                    rankingBlue = createRanking();
                    rankingBlue.Person = match.FighterBlue;
                    rankings.Add(rankingBlue);
                }
                if (rankingRed == null)
                {
                    rankingRed = createRanking();
                    rankingRed.Person = match.FighterRed;
                    rankings.Add(rankingRed);
                }
                UpdateRankingMatch(rankingBlue, rankingRules, match, true);
                UpdateRankingMatch(rankingRed, rankingRules, match, false);
            }

            if (rankingPhaseTypeHandler != null)
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var oldRanking in oldRankings)
                    {
                        session.Delete(oldRanking);
                    }
                    foreach (var ranking in rankings)
                    {
                        ranking.Rank = rankingPhaseTypeHandler.GetRank(ranking.Person, matches);
                        session.Save(ranking);
                    }
                    transaction.Commit();
                }
                return;
            }
            //calc ranking
            var order = rankings.OrderBy(x => x.Disqualified);
            foreach (var rankingStat in rankingRules.Sorting)
            {
                if (rankingStat == RankingStat.MatchPoints)
                {
                    order = order.ThenByDescending(x => x.MatchPointsPerMatch);
                }
                else if (rankingStat == RankingStat.DoubleHits)
                {
                    order = order.ThenBy(x => x.DoubleHitsPerMatch);
                }
                else if (rankingStat == RankingStat.HitRatio)
                {
                    order = order.ThenByDescending(x => x.HitRatio);
                }
                else if (rankingStat == RankingStat.WinRatio)
                {
                    order = order.ThenByDescending(x => x.WinRatio);
                }
                else if (rankingStat == RankingStat.Penalties)
                {
                    order = order.ThenBy(x => x.Penalties);
                }
                else if (rankingStat == RankingStat.Warnings)
                {
                    order = order.ThenBy(x => x.Warnings);
                }
            }

            using (var transaction = session.BeginTransaction())
            {
                foreach (var oldRanking in oldRankings)
                {
                    session.Delete(oldRanking);
                }
                if (rankings.Count == 0)
                {
                    transaction.Commit();
                    return;
                }
                var orderedRankings = order.ToList();
                orderedRankings[0].Rank = 1;
                session.SaveOrUpdate(orderedRankings[0]);
                for (var rankingIndex = 1; rankingIndex < orderedRankings.Count; rankingIndex++)
                {
                    if (orderedRankings[rankingIndex].Disqualified)
                        continue;
                    var same = true;
                    foreach (var rankingStat in rankingRules.Sorting)
                    {
                        if (rankingStat == RankingStat.MatchPoints)
                        {
                            same = same && (orderedRankings[rankingIndex].MatchPointsPerMatch ==
                                            orderedRankings[rankingIndex - 1].MatchPointsPerMatch);
                        }
                        else if (rankingStat == RankingStat.DoubleHits)
                        {
                            same = same && (orderedRankings[rankingIndex].DoubleHitsPerMatch ==
                                            orderedRankings[rankingIndex - 1].DoubleHitsPerMatch);
                        }
                        else if (rankingStat == RankingStat.HitRatio)
                        {
                            same = same && (orderedRankings[rankingIndex].HitRatio ==
                                            orderedRankings[rankingIndex - 1].HitRatio);
                        }
                        else if (rankingStat == RankingStat.WinRatio)
                        {
                            same = same && (orderedRankings[rankingIndex].WinRatio ==
                                            orderedRankings[rankingIndex - 1].WinRatio);
                        }
                        else if (rankingStat == RankingStat.Penalties)
                        {
                            same = same && (orderedRankings[rankingIndex].Penalties ==
                                            orderedRankings[rankingIndex - 1].Penalties);
                        }
                        else if (rankingStat == RankingStat.Warnings)
                        {
                            same = same && (orderedRankings[rankingIndex].Warnings ==
                                            orderedRankings[rankingIndex - 1].Warnings);
                        }
                    }

                    if (same)
                    {
                        orderedRankings[rankingIndex].Rank = orderedRankings[rankingIndex - 1].Rank;
                    }
                    else
                    {
                        orderedRankings[rankingIndex].Rank = rankingIndex + 1;
                    }
                    session.Save(orderedRankings[rankingIndex]);
                }
                transaction.Commit();
            }
        }

        private void UpdateRankingMatch(Ranking ranking, RankingRules rankingRules, Match match, bool blue)
        {
            if (match.Result == MatchResult.Skipped)
                return;
            var win = false;
            ranking.Matches++;
            if (match.Result == MatchResult.Draw)
            {
                ranking.Draws++;
                ranking.MatchPoints += rankingRules.DrawPoints;
            }
            else if (match.Result == MatchResult.WinBlue)
            {
                if (blue)
                {
                    ranking.Wins++;
                    ranking.MatchPoints += rankingRules.WinPoints;
                    win = true;
                }
                else
                {
                    ranking.Losses++;
                    ranking.MatchPoints += rankingRules.LossPoints;
                }
            }
            else if (match.Result == MatchResult.WinRed)
            {
                if (blue)
                {
                    ranking.Losses++;
                    ranking.MatchPoints += rankingRules.LossPoints;
                }
                else
                {
                    ranking.Wins++;
                    ranking.MatchPoints += rankingRules.WinPoints;
                    win = true;
                }
            }
            else if (match.Result == MatchResult.ForfeitBlue)
            {
                if (!blue)
                {
                    ranking.MatchPoints += rankingRules.ForfeitPoints;
                    win = true;
                }
                else
                {
                    ranking.Forfeited = true;
                }
            }
            else if (match.Result == MatchResult.ForfeitRed)
            {
                if (blue)
                {
                    ranking.MatchPoints += rankingRules.ForfeitPoints;
                    win = true;
                }
                else
                {
                    ranking.Forfeited = true;
                }
            }
            else if (match.Result == MatchResult.DisqualificationBlue)
            {
                if (blue)
                {
                    ranking.Disqualified = true;
                }
                else
                {
                    ranking.MatchPoints += rankingRules.DisqualificationPoints;
                    win = true;
                }
            }
            else if (match.Result == MatchResult.DisqualificationRed)
            {
                if (blue)
                {
                    ranking.MatchPoints += rankingRules.DisqualificationPoints;
                    win = true;
                }
                else
                {
                    ranking.Disqualified = true;
                }
            }

            if (rankingRules.DoubleReductionFactor > 0 && match.DoubleCount > rankingRules.DoubleReductionThreshold)
            {
                ranking.MatchPoints -= (match.DoubleCount - rankingRules.DoubleReductionThreshold) / rankingRules.DoubleReductionFactor;
            }

            foreach (var matchEvent in match.Events)
            {
                if (matchEvent.Type == MatchEventType.Score)
                {
                    if (blue)
                    {
                        ranking.HitsGiven += matchEvent.PointsBlue;
                        ranking.HitsReceived += matchEvent.PointsRed;
                    }
                    else
                    {
                        ranking.HitsGiven += matchEvent.PointsRed;
                        ranking.HitsReceived += matchEvent.PointsBlue;
                    }
                    if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                        ranking.Notes++;
                    ranking.Exchanges++;
                }
                else if (matchEvent.Type == MatchEventType.Penalty)
                {
                    if (blue && matchEvent.PointsBlue != 0)
                    {
                        ranking.Penalties++;
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                    else if (!blue && matchEvent.PointsRed != 0)
                    {
                        ranking.Penalties++;
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                    else if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                        ranking.Notes++;
                }
                else if (matchEvent.Type == MatchEventType.MatchPointDeduction)
                {
                    if (blue && matchEvent.PointsBlue != 0)
                    {
                        ranking.Penalties++;
                        ranking.MatchPoints -= Math.Abs(matchEvent.PointsBlue);
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                    else if (!blue && matchEvent.PointsRed != 0)
                    {
                        ranking.Penalties++;
                        ranking.MatchPoints -= Math.Abs(matchEvent.PointsRed);
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                    else if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                        ranking.Notes++;
                }
                else if (matchEvent.Type == MatchEventType.SportsmanshipBlue)
                {
                    if (blue)
                    {
                        ranking.SportsmanshipPoints++;
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.SportsmanshipRed)
                {
                    if (!blue)
                    {
                        ranking.SportsmanshipPoints++;
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.WarningBlue)
                {
                    if (blue)
                    {
                        ranking.Warnings++;
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.WarningRed)
                {
                    if (!blue)
                    {
                        ranking.Warnings++;
                        if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                            ranking.Notes++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.DoubleHit)
                {
                    ranking.DoubleHits++;
                    ranking.Exchanges++;
                    if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                        ranking.Notes++;
                }
                else if (matchEvent.Type == MatchEventType.UnclearExchange)
                {
                    ranking.Exchanges++;
                    if (!string.IsNullOrWhiteSpace(matchEvent.Note))
                        ranking.Notes++;
                }
            }
        }

        public void CreateCompetitionPhase(Guid competiotionId, string phaseName, PhaseType phaseType, string location = null)
        {
            if (string.IsNullOrWhiteSpace(phaseName))
                return;
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var competition = session.Get<Competition>(competiotionId);
                    if (competition == null)
                    {
                        Clients.Caller.displayMessage("Competition not found", "warning");
                        return;
                    }

                    if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                    {
                        Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                        return;
                    }

                    var phase = competition.Phases.SingleOrDefault(x =>
                        string.Equals(x.Name, phaseName, StringComparison.InvariantCultureIgnoreCase));
                    if (phase == null)
                    {
                        phase = new Phase
                        {
                            Name = phaseName,
                            Competition = competition,
                            Location = location,
                            PhaseType = phaseType,
                            PhaseOrder = (competition.Phases.Max(x => x.PhaseOrder as int?) ?? 0) + 1
                        };
                        session.Save(phase);
                        transaction.Commit();
                        Clients.All.addPhase(new PhaseView(phase));
                    }
                    else if (phase.Location != location || phase.PhaseType != phaseType)
                    {
                        phase.Location = location;
                        phase.PhaseType = phaseType;
                        session.Update(phase);
                        transaction.Commit();
                        Clients.All.updatePhase(new PhaseView(phase));
                    }
                }
            }
        }

        public void PhaseAddFighters(Guid phaseId, IList<Guid> fighterIds)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var added = 0;
                foreach (var fighterId in fighterIds)
                {
                    if (phase.Fighters.Any(x => x.Id == fighterId))
                        continue;

                    var fighter = phase.Competition.Fighters.Select(x=>x.Fighter).SingleOrDefault(x => x.Id == fighterId);
                    if (fighter == null)
                        continue;

                    added++;

                    phase.Fighters.Add(fighter);

                }

                if (added <= 0)
                {
                    Clients.Caller.displayMessage("No fighters found to be added to " + phase.Name, "warning");
                    return;
                }

                using (var transaction = session.BeginTransaction())
                {
                    session.Update(phase);
                    transaction.Commit();
                }
                Clients.Caller.displayMessage("Added " + added + " fighter" + (added > 1 ? "s" : "") + " to " + phase.Name, "success");
                Clients.All.updatePhase(new PhaseDetailView(phase));
            }
        }

        public void PhaseSetMatchRules(Guid phaseId, Guid matchRuleId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var matchRules = matchRuleId == Guid.Empty ? null : session.Get<MatchRules>(matchRuleId);
                if (matchRules != null && matchRules == phase.Competition.MatchRules)
                    matchRules = null;
                phase.MatchRules = matchRules;
                session.Update(phase);
                transaction.Commit();
                Clients.All.updatePhase(new PhaseDetailView(phase));
            }
        }

        public void PhaseAddAllFighters(Guid phaseId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                if (phase.Fighters.Count > 0)
                {
                    Clients.Caller.displayMessage("Phase " + phase.Name + " already has fighters", "warning");
                    return;
                }

                phase.Fighters = phase.Competition.Fighters.Select(x=>x.Fighter).ToList();
                session.Update(phase);
                transaction.Commit();
                Clients.All.updatePhase(new PhaseDetailView(phase));
            }
        }

        public void PhaseAddTopFighters(Guid phaseId, int number)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var previousPhase = Service.GetPreviousPhase(phase);
                if (previousPhase == null)
                {
                    Clients.Caller.displayMessage("Previous phase not found", "warning");
                    return;
                }

                var rankings = session.QueryOver<PhaseRanking>().Where(x => x.Phase == previousPhase).List();
                foreach (var phaseRanking in rankings)
                {
                    if (phaseRanking.Rank == null || phaseRanking.Rank > number)
                        continue;
                    if (phase.Fighters.All(x => x.Id != phaseRanking.Person.Id))
                    {
                        phase.Fighters.Add(phaseRanking.Person);
                    }
                }
                session.Update(phase);
                transaction.Commit();
                Clients.All.updatePhase(new PhaseDetailView(phase));
            }
        }

        public void PhaseDistributeFighters(Guid phaseId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                IList<Person> fighters = phase.Fighters.Where(x => phase.Pools.All(y => y.Fighters.All(z => z.Id != x.Id))).ToList();
                fighters = Service.SortFightersByRanking(session, fighters, Service.GetPreviousPhase(phase), phase.Competition.Fighters);
                foreach (var fighter in fighters)
                {
                    var pool = phase.Pools.OrderBy(x => x.Fighters.Sum(y => y.Organizations.Count(z => fighter.Organizations.Any(a => a.Id == z.Id))))
                        .ThenBy(x => x.Fighters.Sum(y => y.Organizations.Count(z => fighter.Organizations.Any(a => !string.IsNullOrWhiteSpace(a.CountryCode) && a.CountryCode == z.CountryCode))))
                        .ThenBy(x => x.Fighters.Count(y => y.CountryCode == fighter.CountryCode))
                        .ThenBy(x => x.Fighters.Count)
                        .FirstOrDefault();
                    if (pool == null)
                        continue;
                    pool.Fighters.Add(fighter);
                    session.Update(pool);
                }

                while (phase.Pools.Max(x => x.Fighters.Count) - phase.Pools.Min(x => x.Fighters.Count) > 1)
                {
                    var toPool = phase.Pools.OrderBy(x => x.Fighters.Count).First();
                    var fromPool = phase.Pools.OrderBy(x => x.Fighters.Count).Last();
                    var fighter = fromPool.Fighters
                        .OrderBy(x => x.Organizations.Sum(y => toPool.Fighters.Sum(z => z.Organizations.Count(a => a.Id == z.Id))))
                        .ThenBy(x => x.Organizations.Sum(y => toPool.Fighters.Sum(z => z.Organizations.Count(a => !string.IsNullOrWhiteSpace(a.CountryCode) && a.CountryCode == z.CountryCode))))
                        .ThenBy(x => toPool.Fighters.Count(y => y.CountryCode == x.CountryCode))
                        .First();

                    fromPool.Fighters.Remove(fighter);
                    session.Update(fromPool);
                    toPool.Fighters.Add(fighter);
                    session.Update(toPool);
                }
                transaction.Commit();
                Clients.All.updatePhase(new PhaseDetailView(phase));
            }
        }

        public void CreatePhasePool(Guid phaseId, string poolName, string location = null, DateTime? plannedDateTime = null)
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return;
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var pool = phase.Pools.SingleOrDefault(x => string.Equals(poolName, x.Name));
                if (pool == null)
                {
                    pool = new Pool
                    {
                        Name = poolName,
                        Phase = phase,
                        Location = location,
                        PlannedDateTime = plannedDateTime
                    };
                    session.Save(pool);
                    transaction.Commit();
                    Clients.All.addPool(new PhasePoolView(pool));
                }
                else if (pool.Location != location || pool.PlannedDateTime != plannedDateTime)
                {
                    pool.Location = location;
                    pool.PlannedDateTime = plannedDateTime;
                    session.Update(pool);
                    transaction.Commit();
                    Clients.All.updatePool(new PoolDetailView(pool));
                }
            }
        }

        public void CompetitionRemoveFighters(Guid competiotionId, IList<Guid> fighterIds)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var competition = session.Get<Competition>(competiotionId);
                if (competition == null)
                {
                    Clients.Caller.displayMessage("Competition not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var deleted = 0;
                var phasesToUpdate = new List<Phase>();
                var poolsToUpdate = new List<Pool>();

                foreach (var fighterId in fighterIds)
                {
                    var fighter = competition.Fighters.SingleOrDefault(x => x.Fighter.Id == fighterId);
                    if (fighter == null)
                        continue;

                    if (competition.Matches.Any(x => x.FighterBlue?.Id == fighterId || x.FighterRed?.Id == fighterId))
                        continue;
                    deleted++;
                    competition.Fighters.Remove(fighter);
                    foreach (var phase in competition.Phases)
                    {
                        if (!phase.Fighters.Contains(fighter.Fighter))
                            continue;
                        phase.Fighters.Remove(fighter.Fighter);
                        if (!phasesToUpdate.Contains(phase))
                        {
                            phasesToUpdate.Add(phase);
                        }

                        foreach (var pool in phase.Pools)
                        {
                            if (!pool.Fighters.Contains(fighter.Fighter))
                                continue;
                            pool.Fighters.Remove(fighter.Fighter);
                            if (!poolsToUpdate.Contains(pool))
                            {
                                poolsToUpdate.Add(pool);
                            }
                        }
                    }
                }

                if (deleted <= 0)
                {
                    Clients.Caller.displayMessage("No fighters found to be removed from " + competition.Name, "warning");
                    return;
                }

                using (var transaction = session.BeginTransaction())
                {
                    session.Update(competition);
                    foreach (var phase in phasesToUpdate)
                    {
                        session.Update(phase);
                    }
                    foreach (var pool in poolsToUpdate)
                    {
                        session.Update(pool);
                    }
                    transaction.Commit();
                }
                Clients.Caller.displayMessage("Removed " + deleted + " fighter" + (deleted > 1 ? "s" : "") + " from " + competition.Name, "success");
                Clients.All.updateCompetition(new CompetitionDetailView(competition));
                foreach (var phase in phasesToUpdate)
                {
                    Clients.All.updatePhase(new PhaseDetailView(phase));
                }
                foreach (var pool in poolsToUpdate)
                {
                    Clients.All.updatePool(new PoolDetailView(pool));
                }
            }
        }

        public void PhaseRemoveFighters(Guid phaseId, IList<Guid> fighterIds)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var deleted = 0;
                var poolsToUpdate = new List<Pool>();

                foreach (var fighterId in fighterIds)
                {
                    var fighter = phase.Fighters.SingleOrDefault(x => x.Id == fighterId);
                    if (fighter == null)
                        continue;

                    if (phase.Matches.Any(x => x.FighterBlue?.Id == fighterId || x.FighterRed?.Id == fighterId))
                        continue;
                    deleted++;
                    phase.Fighters.Remove(fighter);

                    foreach (var pool in phase.Pools)
                    {
                        if (!pool.Fighters.Contains(fighter))
                            continue;
                        pool.Fighters.Remove(fighter);
                        if (!poolsToUpdate.Contains(pool))
                        {
                            poolsToUpdate.Add(pool);
                        }
                    }
                }

                if (deleted <= 0)
                {
                    Clients.Caller.displayMessage("No fighters found to be removed from " + phase.Name, "warning");
                    return;
                }

                using (var transaction = session.BeginTransaction())
                {
                    session.Update(phase);
                    foreach (var pool in poolsToUpdate)
                    {
                        session.Update(pool);
                    }
                    transaction.Commit();
                }
                Clients.Caller.displayMessage("Removed " + deleted + " fighter" + (deleted > 1 ? "s" : "") + " from " + phase.Name, "success");

                Clients.All.updatePhase(new PhaseDetailView(phase));
                foreach (var pool in poolsToUpdate)
                {
                    Clients.All.updatePool(new PoolDetailView(pool));
                }
            }
        }

        public void PoolAddFighters(Guid poolId, IList<Guid> fighterIds)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.Get<Pool>(poolId);
                if (pool == null)
                {
                    Clients.Caller.displayMessage("Pool not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var added = 0;
                foreach (var fighterId in fighterIds)
                {
                    if (pool.Phase.Pools.Any(x => x.Fighters.Any(y => y.Id == fighterId)))
                        continue;

                    var fighter = pool.Phase.Fighters.SingleOrDefault(x => x.Id == fighterId);
                    if (fighter == null)
                        continue;

                    added++;
                    pool.Fighters.Add(fighter);
                }

                if (added <= 0)
                {
                    Clients.Caller.displayMessage("No fighters found to be added to " + pool.Name, "warning");
                    return;
                }

                using (var transaction = session.BeginTransaction())
                {
                    session.Update(pool);
                    transaction.Commit();
                }

                Clients.Caller.displayMessage("Added " + added + " fighter" + (added > 1 ? "s" : "") + " to " + pool.Name, "success");
                Clients.All.updatePool(new PoolDetailView(pool));
                Clients.All.updatePhase(new PhaseDetailView(pool.Phase));

            }

        }
        public void PoolRemoveFighters(Guid poolId, IList<Guid> fighterIds)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.Get<Pool>(poolId);
                if (pool == null)
                {
                    Clients.Caller.displayMessage("Pool not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var deleted = 0;

                foreach (var fighterId in fighterIds)
                {
                    var fighter = pool.Fighters.SingleOrDefault(x => x.Id == fighterId);
                    if (fighter == null)
                        continue;

                    if (pool.Matches.Any(x => x.FighterBlue?.Id == fighterId || x.FighterRed?.Id == fighterId))
                        continue;
                    deleted++;
                    pool.Fighters.Remove(fighter);
                }

                if (deleted <= 0)
                {
                    Clients.Caller.displayMessage("No fighters found to be removed from " + pool.Name, "warning");
                    return;
                }

                using (var transaction = session.BeginTransaction())
                {
                    session.Update(pool);
                    transaction.Commit();
                }
                Clients.Caller.displayMessage("Removed " + deleted + " fighter" + (deleted > 1 ? "s" : "") + " from " + pool.Name, "success");

                Clients.All.updatePool(new PoolDetailView(pool));
                Clients.All.updatePhase(new PhaseDetailView(pool.Phase));
            }
        }
        public void CompetitionAddFighter(Guid competiotionId, string firstName, string lastNamePrefix, string lastName, string orgainzationName, string country, double? seed)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (string.IsNullOrWhiteSpace(firstName) ||
                        string.IsNullOrWhiteSpace(lastName))
                    {
                        Clients.Caller.displayMessage("Name empty", "warning");
                        return;
                    }
                    var competition = session.Get<Competition>(competiotionId);
                    if (competition == null)
                    {
                        Clients.Caller.displayMessage("Competition not found", "warning");
                        return;
                    }

                    if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                    {
                        Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                        return;
                    }

                    var person = session.QueryOver<Person>().Where(x => x.FirstName.IsInsensitiveLike(firstName) && x.LastName.IsInsensitiveLike(lastName)).List().FirstOrDefault();
                    if (person == null)
                    {
                        person = new Person
                        {
                            FirstName = firstName,
                            LastNamePrefix = lastNamePrefix,
                            LastName = lastName
                        };
                        var p = Country.Countries.SingleOrDefault(x => string.Equals(x.Key, country,
                            StringComparison.InvariantCultureIgnoreCase));
                        if (!string.IsNullOrWhiteSpace(p.Key))
                        {
                            person.CountryCode = p.Key;
                        }
                        else
                        {
                            p = Country.Countries.SingleOrDefault(x => string.Equals(x.Value, country,
                                StringComparison.InvariantCultureIgnoreCase));
                            if (!string.IsNullOrWhiteSpace(p.Key))
                            {
                                person.CountryCode = p.Key;
                            }
                        }
                        session.Save(person);
                    }

                    var fighter = competition.Fighters.SingleOrDefault(x => x.Fighter.Id == person.Id);
                    if (fighter == null)
                    {
                        fighter = new CompetitionFighter {Competition = competition, Fighter = person, Seed = seed};
                        session.Save(fighter);
                        competition.Fighters.Add(fighter);
                    }else if (fighter.Seed != seed)
                    {
                        fighter.Seed = seed;
                        session.Update(fighter);
                    }

                    if (!string.IsNullOrWhiteSpace(orgainzationName))
                    {
                        var organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(orgainzationName)).SingleOrDefault();
                        if (organization == null)
                        {
                            organization = new Organization
                            {
                                Name = orgainzationName
                            };
                            session.Save(organization);
                        }
                        if (!person.Organizations.Contains(organization))
                        {
                            person.Organizations.Add(organization);
                            session.Update(person);
                        }
                    }
                    transaction.Commit();
                    Clients.All.updateCompetition(new CompetitionDetailView(competition));
                }
            }
        }

        public void CompetitionSetMatchRules(Guid competiotionId, Guid matchRuleId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var competition = session.Get<Competition>(competiotionId);
                if (competition == null)
                {
                    Clients.Caller.displayMessage("Competition not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var matchRules = session.Get<MatchRules>(matchRuleId);
                if (matchRules == null)
                {
                    Clients.Caller.displayMessage("Match Rules not found", "warning");
                    return;
                }
                competition.MatchRules = matchRules;
                session.Update(competition);
                transaction.Commit();
                Clients.All.updateCompetition(new CompetitionDetailView(competition));
            }
        }

        public void CompetitionSetRankingRules(Guid competiotionId, Guid rankingRuleId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var competition = session.Get<Competition>(competiotionId);
                if (competition == null)
                {
                    Clients.Caller.displayMessage("Competition not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var rankingRules = session.Get<RankingRules>(rankingRuleId);
                if (rankingRules == null)
                {
                    Clients.Caller.displayMessage("Ranking Rules not found", "warning");
                    return;
                }
                competition.RankingRules = rankingRules;
                session.Update(competition);
                transaction.Commit();
                Clients.All.updateCompetition(new CompetitionDetailView(competition));
            }
        }

        public void CompetitionAddFight(Guid competiotionId, string matchName, DateTime? plannedDateTime, Guid blueFighterId, Guid redFighterId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var competition = session.Get<Competition>(competiotionId);
                if (competition == null)
                {
                    Clients.Caller.displayMessage("Competition not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var blueFighter = session.Get<Person>(blueFighterId);
                if (blueFighter == null)
                {
                    Clients.Caller.displayMessage("Blue fighter not found", "warning");
                    return;
                }

                var redFighter = session.Get<Person>(redFighterId);
                if (redFighter == null)
                {
                    Clients.Caller.displayMessage("Red fighter not found", "warning");
                    return;
                }

                if (blueFighter == redFighter)
                {
                    Clients.Caller.displayMessage("Blue and red fighter are the same", "warning");
                    return;
                }

                var uniqueName = matchName;
                if (string.IsNullOrWhiteSpace(matchName))
                {
                    matchName = "Match";
                    uniqueName = matchName + " 1";
                }

                var i = 1;
                while (competition.Matches.Any(x =>
                    string.Equals(x.Name, uniqueName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    uniqueName = matchName + " " + i;
                    i++;
                }

                var match = new Match
                {
                    Name = uniqueName,
                    FighterRed = redFighter,
                    FighterBlue = blueFighter,
                    Competition = competition,
                    PlannedDateTime = plannedDateTime
                };
                session.Save(match);
                competition.Matches.Add(match);
                transaction.Commit();
                Clients.All.addMatch(new MatchView(match));
            }
        }

        public void PhaseGenerateMatches(Guid phaseId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                {
                    Clients.Caller.displayMessage("Phase not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var fighters = Service.SortFightersByRanking(session, phase.Fighters, Service.GetPreviousPhase(phase), phase.Competition.Fighters);

                GenerateMatches(session, phase.PhaseType, phase.Matches, fighters, phase);
            }
        }




        public void PoolGenerateMatches(Guid poolId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.Get<Pool>(poolId);
                if (pool == null)
                {
                    Clients.Caller.displayMessage("Pool not found", "warning");
                    return;
                }

                if (pool.Fighters.Count <= 1)
                {
                    Clients.Caller.displayMessage("Pool " + pool.Name + " has not enough fighter to generate matches", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var fighters = Service.SortFightersByRanking(session, pool.Fighters, Service.GetPreviousPhase(pool.Phase), pool.Phase.Competition.Fighters);
                GenerateMatches(session, pool.Phase.PhaseType, pool.Matches, fighters, pool.Phase, pool);
            }
        }



        private void GenerateMatches(ISession session, PhaseType phaseType, IList<Match> matches, IList<Person> fighters, Phase phase, Pool pool = null)
        {
            if (fighters.Count <= 1)
            {
                Clients.Caller.displayMessage("Not enough fighter to generate matches", "warning");
                return;
            }

            if (!matches.Any(x => x.Started && x.Result != MatchResult.Skipped))
            {
                while (matches.Count > 0)
                {
                    var match = matches.First();
                    Clients.All.removeMatch(match.Id);
                    matches.Remove(match);
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Delete(match);
                        transaction.Commit();
                    }
                }
            }
            else if (matches.Any())
            {
                return;
            }


            var phaseTypeHandler = Service.GetPhaseTypeHandler(phaseType);
            if (phaseTypeHandler == null)
                return;
            matches = phaseTypeHandler.GenerateMatches(fighters.Count, phase, pool);
            var sortedFighters = Service.SortFightersByRanking(session, fighters, Service.GetPreviousPhase(phase), phase.Competition.Fighters);
            phaseTypeHandler.AssignFightersToMatches(matches, sortedFighters);
            var plannedDateTime = pool?.PlannedDateTime;
            foreach (var match in matches)
            {
                match.PlannedDateTime = plannedDateTime;
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(match);
                    transaction.Commit();
                }
                Clients.All.addMatch(new MatchView(match));
                plannedDateTime = plannedDateTime?.AddSeconds(300);
            }
        }

        private void GenerateSingleRoundRobinMatches(ISession session, IList<Match> matches, IList<Person> fighters,
            Phase phase, Pool pool, int secondsPerMatch)
        {

            var matchCounter = 1;
            var plannedDateTime = pool?.PlannedDateTime;

            while (true)
            {
                var fighterBlue = fighters
                    .OrderBy(x => matches.Count(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id))
                    .First();
                var fighterRed = fighters.Where(x =>
                        x.Id != fighterBlue.Id && !matches.Any(y =>
                            (y.FighterBlue.Id == fighterBlue.Id && y.FighterRed.Id == x.Id) ||
                            (y.FighterRed.Id == fighterBlue.Id && y.FighterBlue.Id == x.Id)))
                    .OrderBy(x => matches.Count(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id))
                    .ThenBy(x => matches.OrderBy(y => y.Name).LastOrDefault(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id)?.Name)
                    .FirstOrDefault();
                if (fighterRed == null)
                    break;


                var match = new Match
                {
                    Name = "Match " + matchCounter.ToString().PadLeft(4),
                    FighterBlue = fighterBlue,
                    FighterRed = fighterRed,
                    Competition = phase.Competition,
                    PlannedDateTime = plannedDateTime,
                    Phase = phase,
                    Pool = pool
                };
                matches.Add(match);
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(match);
                    transaction.Commit();
                }

                Clients.All.addMatch(new MatchView(match));
                matchCounter++;
                plannedDateTime = plannedDateTime?.AddSeconds(secondsPerMatch);
            }
        }

        public void MatchSetMatchRules(Guid matchId, Guid matchRuleId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var match = session.Get<Match>(matchId);
                if (match == null)
                {
                    Clients.Caller.displayMessage("Match not found", "warning");
                    return;
                }

                if (!HasOrganizationRights(session, match.Competition.Organization, UserRoles.Admin))
                {
                    Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                    return;
                }

                var matchRules = matchRuleId == Guid.Empty ? null : session.Get<MatchRules>(matchRuleId);
                if (matchRules != null && matchRules == (match.Phase?.MatchRules ?? match.Competition.MatchRules))
                    matchRules = null;

                match.Rules = matchRules;
                match.UpdateMatchData();
                session.Update(match);
                transaction.Commit();
                Clients.All.updateMatch(new MatchDetailView(match));
            }
        }

        public void UpdateMatchRound(Guid matchId, int round)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var match = session.Get<Match>(matchId);
                if (match == null)
                {
                    Clients.Caller.displayMessage("Match not found", "warning");
                    return;
                }
                if (!HasOrganizationRights(session, match.Competition.Organization, UserRoles.Scorekeeper))
                {
                    Clients.Caller.displayMessage("Not logged in as scorekeeper", "warning");
                    return;
                }
                if (match.Round == round)
                    return;
                if (match.Finished)
                {
                    Clients.Caller.displayMessage("Match finished", "warning");
                    return;
                }
                var rules = match.GetRules();
                if (rules == null || round > rules.Rounds)
                {
                    Clients.Caller.displayMessage("Round not allowed", "warning");
                    return;
                }
                match.TimeRunningSince = null;
                match.Time = TimeSpan.Zero;
                match.Round = round;
                match.UpdateMatchData();
                session.Update(match);
                transaction.Commit();
                Clients.All.updateMatch(new MatchDetailView(match));
            }
        }

        public void UpdateMatchLocation(string location, IList<Guid> matchIds)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                foreach (var matchId in matchIds)
                {
                    var match = session.Get<Match>(matchId);
                    if (match == null)
                    {
                        Clients.Caller.displayMessage("Match not found", "warning");
                        return;
                    }

                    if (!HasOrganizationRights(session, match.Competition.Organization, UserRoles.Admin))
                    {
                        Clients.Caller.displayMessage("Not logged in as administrator", "warning");
                        return;
                    }

                    using (var transaction = session.BeginTransaction())
                    {
                        if (string.IsNullOrWhiteSpace(location))
                        {
                            match.Location = null;
                        }
                        else if (!string.IsNullOrWhiteSpace(match.Pool?.Location))
                        {
                            if (location == match.Pool.Location)
                            {
                                match.Location = null;
                            }
                            else
                            {
                                match.Location = location;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(match.Phase?.Location))
                        {
                            if (location == match.Phase.Location)
                            {
                                match.Location = null;
                            }
                            else
                            {
                                match.Location = location;
                            }
                        }
                        else
                        {
                            match.Location = location;
                        }
                        match.UpdateMatchData();
                        session.Update(match);
                        transaction.Commit();
                        Clients.All.updateMatch(new MatchDetailView(match));

                    }

                }
            }
        }

        public void ShowMatchOnLocation(Guid matchId, string location)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var match = session.Get<Match>(matchId);
                if (match == null)
                {
                    Clients.Caller.displayMessage("Match not found", "warning");
                    return;
                }
                if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                {
                    Clients.Caller.displayMessage("Not logged in as score keeper", "warning");
                    return;
                }
            }
            Clients.All.showMatchOnLocation(matchId, location);
        }
    }
}