using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.UI;
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
                    if(match == null)
                        return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
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
                    UpdateMatchData(match);
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
                    if(lastEvent.CreatedDateTime < DateTime.Now.AddSeconds(-300))
                        return;

                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                        return;

                    match.Events.Remove(lastEvent);
                    UpdateMatchData(match);
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

        private void UpdateMatchData(Match match)
        {
            //TODO check matchrules for CountUnclearExchange
            match.ExchangeCount = match.Events.Count(x => x.Type != MatchEventType.WarningBlue &&
                                                          x.Type != MatchEventType.WarningRed);
            match.DoubleCount = match.Events.Count(x => x.Type == MatchEventType.DoubleHit);
            match.ScoreRed = match.Events.Sum(x => x.PointsRed < 0 ? x.PointsRed : (x.PointsRed > x.PointsBlue ? x.PointsRed - x.PointsBlue : 0));
            match.ScoreBlue = match.Events.Sum(x => x.PointsBlue < 0 ? x.PointsBlue : (x.PointsBlue > x.PointsRed ? x.PointsBlue - x.PointsRed : 0));
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
                    }
                    session.Update(match);
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
                }
            }
        }

        public void ChangeTime(Guid matchGuid, TimeSpan time)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    //TODO check rights
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
                using (var transaction = session.BeginTransaction())
                {
                    var match = session.Get<Match>(matchGuid);
                    if (!HasMatchRights(session, match, UserRoles.Scorekeeper))
                        return;

                    if (match.Finished && !HasMatchRights(session, match, UserRoles.ScoreValidator))
                        return;

                    if (match.Validated && !HasMatchRights(session, match, UserRoles.Admin))
                        return;

                    match.Result = matchResult;
                    if (matchResult == MatchResult.None)
                    {
                        if (!HasMatchRights(session, match, UserRoles.Admin))
                            return;
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
                    transaction.Commit();
                    Clients.All.updateMatch(new MatchWithEventsView(match));
                }
            }
        }

        public void CreateCompetitionPhase(Guid competiotionId, string phaseName, string location = null)
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
                            Location = location
                        };
                        session.Save(phase);
                        transaction.Commit();
                        Clients.All.addPhase(new PhaseView(phase));
                    }else if (!string.IsNullOrWhiteSpace(location) && phase.Location != location)
                    {
                        phase.Location = location;
                        session.Update(phase);
                        transaction.Commit();
                        Clients.All.updatePhase(new PhaseView(phase));
                    }
                }
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

        public void CreatePhasePool(Guid phaseId, string poolName, string location = null)
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
                        Location = location
                    };
                    session.Save(pool);
                    transaction.Commit();
                    Clients.All.addPool(new PhasePoolView(pool));
                }
                else if (!string.IsNullOrWhiteSpace(location) && pool.Location != location)
                {
                    pool.Location = location;
                    session.Update(pool);
                    transaction.Commit();
                    Clients.All.updatePool(new PoolDetailView(pool));
                }
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
                        person.Organizations.Add(organization);
                        session.Update(person);
                    }
                    transaction.Commit();
                    Clients.All.updateCompetition(new CompetitionDetailView(competition));
                }
            }
        }

        public void CompetitionAddFight(Guid competiotionId, string matchName, string phaseName, string poolName,
            DateTime? plannedDateTime, Guid blueFighterId, Guid redFighterId)
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

                Phase phase = null;
                if (!string.IsNullOrWhiteSpace(phaseName))
                {
                    phase = session.QueryOver<Phase>()
                        .Where(x => x.Competition == competition && x.Name.IsInsensitiveLike(phaseName))
                        .SingleOrDefault();
                    if (phase == null)
                    {
                        phase = new Phase
                        {
                            Name = phaseName,
                            Competition = competition
                        };
                        session.Save(phase);
                        competition.Phases.Add(phase);
                    }

                    if (phase.Fighters.All(x => x.Id != blueFighter.Id))
                    {
                        phase.Fighters.Add(blueFighter);
                        session.Update(phase);
                    }

                    if (phase.Fighters.All(x => x.Id != redFighter.Id))
                    {
                        phase.Fighters.Add(redFighter);
                        session.Update(phase);
                    }
                }

                Pool pool = null;
                if (!string.IsNullOrWhiteSpace(poolName) && phase != null)
                {
                    pool = session.QueryOver<Pool>().Where(x => x.Phase == phase && x.Name.IsInsensitiveLike(poolName))
                        .SingleOrDefault();
                    if (pool == null)
                    {
                        pool = new Pool
                        {
                            Name = poolName,
                            Phase = phase
                        };
                        session.Save(pool);
                        phase.Pools.Add(pool);
                    }

                    if (pool.Fighters.All(x => x.Id != blueFighter.Id))
                    {
                        pool.Fighters.Add(blueFighter);
                        session.Update(pool);
                    }

                    if (pool.Fighters.All(x => x.Id != redFighter.Id))
                    {
                        pool.Fighters.Add(redFighter);
                        session.Update(pool);
                    }
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
                    Phase = phase,
                    Pool = pool,
                    PlannedDateTime = plannedDateTime
                };
                session.Save(match);
                competition.Matches.Add(match);
                transaction.Commit();
                Clients.All.addMatch(new MatchView(match));
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

                if (!pool.Matches.Any(x => x.Started))
                {
                    while (pool.Matches.Count > 0)
                    {
                        var match = pool.Matches.First();
                        Clients.All.removeMatch(match.Id);
                        pool.Matches.Remove(match);
                        using (var transaction = session.BeginTransaction())
                        {
                            session.Delete(match);
                            transaction.Commit();
                        }
                    }
                }

                var matchCounter = 1;
                while (true)
                {
                    var fighterBlue = pool.Fighters
                        .OrderBy(x => pool.Matches.Count(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id))
                        .First();
                    var fighterRed = pool.Fighters.Where(x =>
                            x.Id != fighterBlue.Id && !pool.Matches.Any(y =>
                                (y.FighterBlue.Id == fighterBlue.Id && y.FighterRed.Id == x.Id) ||
                                (y.FighterRed.Id == fighterBlue.Id && y.FighterBlue.Id == x.Id)))
                        .OrderBy(x => pool.Matches.Count(y => y.FighterBlue?.Id == x.Id || y.FighterRed?.Id == x.Id))
                        .FirstOrDefault();
                    if (fighterRed == null)
                        break;


                    var match = new Match
                    {
                        Name = "Match " + matchCounter.ToString().PadLeft(4),
                        FighterBlue = fighterBlue,
                        FighterRed = fighterRed,
                        Competition = pool.Phase.Competition,
                        Phase = pool.Phase,
                        Pool = pool
                    };
                    pool.Matches.Add(match);
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
}