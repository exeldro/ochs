using System;
using System.Collections.Generic;
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
            if(idstring == null)
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
        
        public void AddMatchEvent(Guid matchGuid, int pointsBlue, int pointsRed, MatchEventType eventType)
        {
            //Context.Request.User
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (match == null || match.Finished || match.Validated)
                        return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                        return;

                    //check for doubleclick
                    if(match.Events.Any(x=>DateTime.Now.Subtract(x.CreatedDateTime).TotalSeconds < 0.5))
                        return;

                    match.Events.Add(new MatchEvent
                    {
                        CreatedDateTime = DateTime.Now,
                        PointsBlue = pointsBlue,
                        PointsRed = pointsRed,
                        MatchTime = match.LiveTime,
                        Round = match.Round,
                        Type = eventType,
                        Match = match
                    });
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
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
                    if (match == null || match.Finished || match.Validated || !match.Events.Any())
                        return;

                    var lastEvent = match.Events.OrderBy(x => x.CreatedDateTime).Last();
                    //if(lastEvent.CreatedDateTime < DateTime.Now.AddSeconds(-300))
                    //    return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                        return;

                    match.Events.Remove(lastEvent);
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
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
                    if (match == null || match.Finished || match.Validated || !match.Events.Any())
                        return;

                    var deleteEvent = match.Events.SingleOrDefault(x => x.Id == eventGuid);
                    if(deleteEvent == null)
                        return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                        return;

                    match.Events.Remove(deleteEvent);
                    match.UpdateMatchData();
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
                }
            }
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
            if(idstring == null)
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
                        return;

                    match.Time = match.LiveTime;
                    match.TimeRunningSince = null;
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
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
                    if (match.TimeRunningSince.HasValue || match.Finished)
                        return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

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
                    Clients.All.updateMatch(new MatchWithEventsView(match));
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
                        return;
                    
                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                        return;

                    match.Time = time;
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
                }
            }
        }

        public void SetMatchResult(Guid matchGuid, MatchResult matchResult)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var match = session.Get<Match>(matchGuid);
                if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                    return;

                if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                    return;

                if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                    return;

                if (matchResult == MatchResult.None)
                {
                    if (!HasMatchRights(session, match, UserRoles.Admin))
                        return;
                    match.Result = matchResult;
                    if (match.FinishedDateTime.HasValue)
                    {
                        match.FinishedDateTime = null;
                    }
                }
                else
                {
                    if (!match.StartedDateTime.HasValue)
                    {
                        if (!HasMatchRights(session, match, UserRoles.Admin))
                            return;
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
                }
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(match);
                    transaction.Commit();
                }
                Clients.All.updateMatch(new MatchWithEventsView(match));
                if (match.Phase?.Elimination ?? false)
                {
                    Person winner = null;
                    Person loser = null;
                    if (match.Result == MatchResult.WinBlue || match.Result == MatchResult.DisqualificationRed || match.Result == MatchResult.ForfeitRed)
                    {
                        winner = match.FighterBlue;
                        if(match.Result == MatchResult.WinBlue)
                            loser = match.FighterRed;
                    }
                    else if(match.Result == MatchResult.WinRed || match.Result == MatchResult.DisqualificationBlue || match.Result == MatchResult.ForfeitBlue)
                    {
                        winner = match.FighterRed;
                        if(match.Result == MatchResult.WinRed)
                            loser = match.FighterBlue;
                    }

                    var round = Service.GetRound(match.Name);
                    if (round > 0 && winner != null)
                    {
                        var matchNumber = Service.GetMatchNumber(match.Name, round);
                        var nextRound = round-1;
                        var nextMatchNumber = ((matchNumber - 1) >> 1) + 1;
                        var nextMatchName = Service.GetMatchName(nextRound, nextMatchNumber).Trim();
                        Match nextMatch = null;
                        if (match.Pool != null)
                        {
                            nextMatch = match.Pool.Matches.SingleOrDefault(x => x.Name.Trim() == nextMatchName);
                        }
                        else if (match.Phase != null)
                        {
                            nextMatch = match.Phase.Matches.SingleOrDefault(x => x.Name.Trim() == nextMatchName);
                        }

                        if (nextMatch != null)
                        {
                            if (matchNumber % 2 == 1)
                            {
                                nextMatch.FighterBlue = winner;
                            }
                            else
                            {
                                nextMatch.FighterRed = winner;
                            }

                            using (var transaction = session.BeginTransaction())
                            {
                                session.Update(nextMatch);
                                transaction.Commit();
                            }

                            Clients.All.updateMatch(new MatchView(nextMatch));
                        }

                        if (round == 1 && loser != null)
                        {
                            nextMatchName = Service.GetMatchName(0, 2).Trim();
                            nextMatch = null;
                            if (match.Pool != null)
                            {
                                nextMatch = match.Pool.Matches.SingleOrDefault(x => x.Name.Trim() == nextMatchName);
                            }
                            else if (match.Phase != null)
                            {
                                nextMatch = match.Phase.Matches.SingleOrDefault(x => x.Name.Trim() == nextMatchName);
                            }

                            if (nextMatch != null)
                            {
                                if (matchNumber % 2 == 1)
                                {
                                    nextMatch.FighterBlue = loser;
                                }
                                else
                                {
                                    nextMatch.FighterRed = loser;
                                }

                                using (var transaction = session.BeginTransaction())
                                {
                                    session.Update(nextMatch);
                                    transaction.Commit();
                                }

                                Clients.All.updateMatch(new MatchView(nextMatch));
                            }
                        }
                    }
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
            UpdateRankingsInternal(session, rankings, phase.Matches, () => new PhaseRanking {Phase = phase},
                phase.Elimination);
            Clients.All.updateRankings(phase.Id);
        }

        public void UpdatePhaseRankings(Guid phaseId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;
                UpdatePhaseRankingsInternal(session, phase);
            }
        }

        private void UpdatePoolRankingsInternal(ISession session, Pool pool)
        {
            var rankings = session.QueryOver<PoolRanking>().Where(x => x.Pool == pool).List().Cast<Ranking>().ToList();
            UpdateRankingsInternal(session, rankings, pool.Matches, () => new PoolRanking {Pool = pool}, pool.Phase.Elimination);
            Clients.All.updateRankings(pool.Id);
        }

        public void UpdatePoolRankings(Guid poolId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.Get<Pool>(poolId);
                if (pool == null)
                    return;

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                    return;
                UpdatePoolRankingsInternal(session, pool);
            }
        }


        private void UpdateRankingsInternal(ISession session, IList<Ranking> rankings, IList<Match> matches, Func<Ranking> createRanking, bool elimination)
        {
            var rankingRules = new RankingRules();
            // clear ranking stats
            foreach (var ranking in rankings)
            {
                ranking.Rank = null;
                ranking.Matches = 0;
                ranking.DoubleHits = 0;
                ranking.Exchanges = 0;
                ranking.HitsGiven = 0;
                ranking.HitsReceived = 0;
                ranking.Losses = 0;
                ranking.Draws = 0;
                ranking.Wins = 0;
                ranking.Warnings = 0;
                ranking.Penalties = 0;
                ranking.MatchPoints = 0;
                ranking.SportsmanshipPoints = 0;
            }
            //calc ranking stats
            foreach (var match in matches)
            {
                if(!match.Finished)
                    continue;
                var rankingBlue = rankings.SingleOrDefault(x => x.Person == match.FighterBlue);
                if (rankingBlue == null)
                {
                    rankingBlue = createRanking();
                    rankingBlue.Person = match.FighterBlue;
                    rankings.Add(rankingBlue);
                }

                UpdateRankingMatch(rankingBlue, rankingRules, match, true, elimination);
                var rankingRed = rankings.SingleOrDefault(x => x.Person == match.FighterRed);
                if (rankingRed == null)
                {
                    rankingRed = createRanking();
                    rankingRed.Person = match.FighterRed;
                    rankings.Add(rankingRed);
                }
                UpdateRankingMatch(rankingRed, rankingRules, match, false, elimination);
            }

            if (elimination)
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var ranking in rankings)
                    {
                        session.SaveOrUpdate(ranking);
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
                    order = order.ThenByDescending(x=>x.MatchPointsPerMatch);
                }else if (rankingStat == RankingStat.DoubleHits)
                {
                    order = order.ThenBy(x => x.DoubleHitsPerMatch);
                }else if (rankingStat == RankingStat.HitRatio)
                {
                    order = order.ThenByDescending(x => x.HitRatio);
                }else if (rankingStat == RankingStat.WinRatio)
                {
                    order = order.ThenByDescending(x => x.WinRatio);
                }else if (rankingStat == RankingStat.Penalties)
                {
                    order = order.ThenBy(x => x.Penalties);
                }else if (rankingStat == RankingStat.Warnings)
                {
                    order = order.ThenBy(x => x.Warnings);
                }
            }

            using (var transaction = session.BeginTransaction())
            {
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
                    session.SaveOrUpdate(orderedRankings[rankingIndex]);
                }
                transaction.Commit();
            }
        }

        private void UpdateRankingMatch(Ranking ranking, RankingRules rankingRules, Match match, bool blue, bool elimination)
        {
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
            }
            else if (match.Result == MatchResult.ForfeitRed)
            {
                if (blue)
                {
                    ranking.MatchPoints += rankingRules.ForfeitPoints;
                    win = true;
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
            if (elimination && match.Finished)
            {
                var round = Service.GetRound(match.Name);
                if (round == 0)
                {
                    if (match.Name.Trim() == Service.GetMatchName(round, 2).Trim())
                    {
                        ranking.Rank = win ? 3 : 4;
                    }
                    else
                    {
                        ranking.Rank = win ? 1 : 2;
                    }
                }
                else if(win)
                {
                    if (ranking.Rank == null || ranking.Rank > 1 << round)
                    {
                        ranking.Rank = 1 << round;
                    }
                }
                else
                {
                    if (ranking.Rank == null || ranking.Rank > (1 << round) + 1)
                    {
                        ranking.Rank = (1 << round) + 1;
                    }
                }
            }

            if (rankingRules.DoubleReduction > 0 && match.DoubleCount> rankingRules.DoubleReduction)
            {
                ranking.MatchPoints -= match.DoubleCount - rankingRules.DoubleReduction;
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
                    ranking.Exchanges++;
                }
                else if (matchEvent.Type == MatchEventType.Penalty)
                {
                    if (blue)
                    {
                        ranking.Penalties += Math.Abs(matchEvent.PointsBlue);
                    }
                    else
                    {
                        ranking.Penalties += Math.Abs(matchEvent.PointsRed);
                    }
                }
                else if (matchEvent.Type == MatchEventType.SportsmanshipBlue)
                {
                    if (blue)
                    {
                        ranking.SportsmanshipPoints++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.SportsmanshipRed)
                {
                    if (!blue)
                    {
                        ranking.SportsmanshipPoints++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.WarningBlue)
                {
                    if (blue)
                    {
                        ranking.Warnings++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.WarningRed)
                {
                    if (!blue)
                    {
                        ranking.Warnings++;
                    }
                }
                else if (matchEvent.Type == MatchEventType.DoubleHit)
                {
                    ranking.DoubleHits++;
                    ranking.Exchanges++;
                }
                else if (matchEvent.Type == MatchEventType.UnclearExchange)
                {
                    ranking.Exchanges++;
                }
            }
        }

        public void CreateCompetitionPhase(Guid competiotionId, string phaseName, PhaseType phaseType, string location = null)
        {
            if(string.IsNullOrWhiteSpace(phaseName))
                return;
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var competition = session.Get<Competition>(competiotionId);
                    if (competition == null)
                        return;

                    if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                        return;

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
                    }else if (phase.Location != location || phase.PhaseType != phaseType)
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
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

                var added = 0;
                foreach (var fighterId in fighterIds)
                {
                    if (phase.Fighters.Any(x => x.Id == fighterId))
                        continue;

                    var fighter = phase.Competition.Fighters.SingleOrDefault(x => x.Id == fighterId);
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
        public void PhaseAddAllFighters(Guid phaseId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var phase = session.Get<Phase>(phaseId);
                if (phase == null)
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

                if(phase.Fighters.Count > 0)
                    return;

                phase.Fighters = phase.Competition.Fighters.ToList();
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
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

                var previousPhase = Service.GetPreviousPhase(phase);
                if(previousPhase == null)
                    return;

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
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

                var fighters = phase.Fighters.Where(x => phase.Pools.All(y => y.Fighters.All(z => z.Id != x.Id))).ToList();
                foreach (var fighter in fighters)
                {
                    var pool = phase.Pools.OrderBy(x => x.Fighters.Sum(y => y.Organizations.Count(z => fighter.Organizations.Any(a => a.Id == z.Id))))
                        .ThenBy(x => x.Fighters.Count(y => y.CountryCode == fighter.CountryCode))
                        .ThenBy(x => x.Fighters.Count)
                        .FirstOrDefault();
                    if(pool == null)
                        continue;
                    pool.Fighters.Add(fighter);
                    session.Update(pool);
                }

                while (phase.Pools.Max(x => x.Fighters.Count) - phase.Pools.Min(x => x.Fighters.Count) > 1)
                {
                    var toPool = phase.Pools.OrderBy(x => x.Fighters.Count).First();
                    var fromPool = phase.Pools.OrderBy(x => x.Fighters.Count).Last();
                    var fighter = fromPool.Fighters
                        .OrderBy(x =>x.Organizations.Sum(y => toPool.Fighters.Sum(z => z.Organizations.Count(a => a.Id == z.Id))))
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
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

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
                if(competition == null)
                    return;

                if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                    return;

                var deleted = 0;
                var phasesToUpdate = new List<Phase>();
                var poolsToUpdate = new List<Pool>();

                foreach (var fighterId in fighterIds)
                {
                    var fighter = competition.Fighters.SingleOrDefault(x => x.Id == fighterId);
                    if(fighter == null)
                        continue;

                    if(competition.Matches.Any(x=>x.FighterBlue?.Id == fighterId || x.FighterRed?.Id == fighterId))
                        continue;
                    deleted++;
                    competition.Fighters.Remove(fighter);
                    foreach (var phase in competition.Phases)
                    {
                        if (!phase.Fighters.Contains(fighter))
                            continue;
                        phase.Fighters.Remove(fighter);
                        if (!phasesToUpdate.Contains(phase))
                        {
                            phasesToUpdate.Add(phase);
                        }

                        foreach (var pool in phase.Pools)
                        {
                            if(!pool.Fighters.Contains(fighter))
                                continue;
                            pool.Fighters.Remove(fighter);
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
                if(phase == null)
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

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
                    return;

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                    return;

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
                if(pool == null)
                    return;

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                    return;

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
        public void CompetitionAddFighter(Guid competiotionId, string firstName, string lastNamePrefix, string lastName, string orgainzationName, string country)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var competition = session.Get<Competition>(competiotionId);
                    if (competition == null || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                        return;
                    if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                        return;

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
                    if(competition.Fighters.All(x => x.Id != person.Id))
                    {
                        competition.Fighters.Add(person);
                        session.Update(competition);
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

        public void CompetitionAddFight(Guid competiotionId, string matchName, DateTime? plannedDateTime, Guid blueFighterId, Guid redFighterId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var competition = session.Get<Competition>(competiotionId);
                if (competition == null)
                    return;

                if (!HasOrganizationRights(session, competition.Organization, UserRoles.Admin))
                    return;

                var blueFighter = session.Get<Person>(blueFighterId);
                if (blueFighter == null)
                    return;

                var redFighter = session.Get<Person>(redFighterId);
                if (redFighter == null)
                    return;

                if (blueFighter == redFighter)
                    return;

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
                    return;

                if (!HasOrganizationRights(session, phase.Competition.Organization, UserRoles.Admin))
                    return;

                var fighters = Service.SortFightersByRanking(session, phase.Fighters, Service.GetPreviousPhase(phase));

                GenerateMatches(session, phase.PhaseType, phase.Matches, fighters, phase);
            }
        }




        public void PoolGenerateMatches(Guid poolId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.Get<Pool>(poolId);
                if (pool == null)
                    return;

                if(pool.Fighters.Count <= 1)
                    return;

                if (!HasOrganizationRights(session, pool.Phase.Competition.Organization, UserRoles.Admin))
                    return;

                var fighters = Service.SortFightersByRanking(session, pool.Fighters, Service.GetPreviousPhase(pool.Phase));
                GenerateMatches(session, pool.Phase.PhaseType, pool.Matches, fighters, pool.Phase, pool);
            }
        }



        private void GenerateMatches(ISession session, PhaseType phaseType, IList<Match> matches, IList<Person> fighters, Phase phase, Pool pool = null)
        {
            if(fighters.Count <= 1)
                return;

            if (!matches.Any(x => x.Started))
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

            if (phaseType == PhaseType.SingleRoundRobin)
            {
                GenerateSingleRoundRobinMatches(session, matches, fighters, phase, pool);
            }
            else if (phaseType == PhaseType.SingleElimination)
            {
                GenerateSingleEliminationMatches(session, matches, fighters.Count, phase, pool);
                AssignFightersToSingleEliminationMatches(session, matches, fighters);
            }
        }

        private void AssignFightersToSingleEliminationMatches(ISession session, IList<Match> matches, IList<Person> fighters)
        {
            var sortedFighters = Service.SortFightersByRanking(session, fighters, Service.GetPreviousPhase(matches[0].Phase));
            var matchedFighters = Service.SingleEliminationMatchedFighters(sortedFighters);
            
            var roundCount = 0;
            while (2<<roundCount < fighters.Count)
                roundCount++;


            for (var i = 0; i < matchedFighters.Count - 1; i += 2)
            {
                var matchNumber = (i >> 1) +1;
                var matchName = Service.GetMatchName(roundCount, matchNumber).Trim();
                var match = matches.SingleOrDefault(x => x.Name.Trim() == matchName);
                if (matchedFighters[i] == null || matchedFighters[i + 1] == null)
                {
                    var fighter = matchedFighters[i] ?? matchedFighters[i + 1];
                    if(fighter == null)
                        continue;
                    matchName = Service.GetMatchName(roundCount-1, (i>>2)+1).Trim();
                    match = matches.SingleOrDefault(x => x.Name.Trim() == matchName);
                    if(match == null)
                        continue;
                    if (matchNumber % 2 == 1)
                    {
                        match.FighterBlue = fighter;
                    }
                    else
                    {
                        match.FighterRed = fighter;
                    }
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Update(match);
                        transaction.Commit();
                    }
                    Clients.All.updateMatch(new MatchView(match));
                    continue;
                }
                if(match == null)
                    continue;
                match.FighterBlue = matchedFighters[i];
                match.FighterRed = matchedFighters[i + 1];
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(match);
                    transaction.Commit();
                }
                Clients.All.updateMatch(new MatchView(match));
            }
        }

        private void GenerateSingleEliminationMatches(ISession session, IList<Match> matches, int fighterCount, Phase phase, Pool pool)
        {
            var roundCount = 0;
            while (2<<roundCount < fighterCount)
                roundCount++;

            for (var round = roundCount; round > 0; round--)
            {
                for (var matchNumber = 1; matchNumber <= (1 << round); matchNumber++)
                {
                    var match = new Match
                    {
                        Name = Service.GetMatchName(round, matchNumber),
                        Competition = phase.Competition,
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
                }
            }
            var finalmatch = new Match
            {
                Name = Service.GetMatchName(0, 2),
                Competition = phase.Competition,
                Phase = phase,
                Pool = pool
            };
            matches.Add(finalmatch);
            using (var transaction = session.BeginTransaction())
            {
                session.Save(finalmatch);
                transaction.Commit();
            }
            Clients.All.addMatch(new MatchView(finalmatch));
            finalmatch = new Match
            {
                Name = Service.GetMatchName(0, 1),
                Competition = phase.Competition,
                Phase = phase,
                Pool = pool
            };
            matches.Add(finalmatch);
            using (var transaction = session.BeginTransaction())
            {
                session.Save(finalmatch);
                transaction.Commit();
            }
            Clients.All.addMatch(new MatchView(finalmatch));
        }

        private void GenerateSingleRoundRobinMatches(ISession session, IList<Match> matches, IList<Person> fighters,
            Phase phase, Pool pool)
        {

            var matchCounter = 1;
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
            }
        }
    }
}