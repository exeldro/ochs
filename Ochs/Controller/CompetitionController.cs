using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualBasic.FileIO;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;

namespace Ochs
{
    public class CompetitionController : ApiController
    {
        [HttpGet]
        public IList<CompetitionView> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Competition>().List().Select(x =>
                {
                    NHibernateUtil.Initialize(x.Organization);
                    return new CompetitionView(x);
                }).ToList();
            }
        }

        [HttpGet]
        public CompetitionDetailView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var competition = session.QueryOver<Competition>().Where(x => x.Id == id).SingleOrDefault();
                if (competition == null)
                    return null;
                NHibernateUtil.Initialize(competition.Organization);
                NHibernateUtil.Initialize(competition.Fighters);
                foreach (var person in competition.Fighters)
                {
                    NHibernateUtil.Initialize(person.Fighter);
                    NHibernateUtil.Initialize(person.Fighter.Organizations);
                }

                foreach (var match in competition.Matches)
                {
                    NHibernateUtil.Initialize(match.FighterBlue);
                    NHibernateUtil.Initialize(match.FighterBlue?.Organizations);
                    NHibernateUtil.Initialize(match.FighterRed);
                    NHibernateUtil.Initialize(match.FighterRed?.Organizations);
                    NHibernateUtil.Initialize(match.Phase);
                    NHibernateUtil.Initialize(match.Pool);
                }
                NHibernateUtil.Initialize(competition.Matches);
                NHibernateUtil.Initialize(competition.Phases);
                NHibernateUtil.Initialize(competition.MatchRules);
                NHibernateUtil.Initialize(competition.RankingRules);
                return new CompetitionDetailView(competition);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public CompetitionView Create([FromBody]CompetitionCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CompetitionName))
                return null;

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                Organization organization = null;
                if (!string.IsNullOrWhiteSpace(request.OrganizationName))
                {
                    organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(request.OrganizationName)).SingleOrDefault();
                    if (organization == null)
                    {
                        organization = new Organization{Name = request.OrganizationName};
                        session.Save(organization);
                    }
                }

                var competition = new Competition {Name = request.CompetitionName, Organization = organization};
                session.Save(competition);
                transaction.Commit();
                return new CompetitionView(competition);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public CompetitionDetailView UploadFighters(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var competition = session.QueryOver<Competition>().Where(x => x.Id == id).SingleOrDefault();
                if (competition == null)
                    return null;

                var organizations = session.QueryOver<Organization>().List();

                using (var parser = new TextFieldParser(Request.Content.ReadAsStreamAsync().Result))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters("\t", ";");
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.TrimWhiteSpace = true;
                    var headerFields = parser.ReadFields()?.ToList();
                    if (headerFields != null)
                    {
                        var firstNameIndex = FindIndex(headerFields, 0, "First name", "Given name");
                        var lastNamePrefixIndex = FindIndex(headerFields, 0, "Prefix", "Last name prefix","Family name prefix");
                        var lastNameIndex = FindIndex(headerFields, 0, "Last name", "Family name");
                        var fullNameIndex = FindIndex(headerFields, 0, "Full name", "Personal name", "Name");
                        var countryIndex = FindIndex(headerFields, 0, "Country");
                        var seedIndex = FindIndex(headerFields, 0, "Seed","Ranking","Rank");
                        var organizationIndexes = new List<int>();
                        var organizationIndexItem = FindIndex(headerFields, 0, "Organization","Club");
                        while (organizationIndexItem != -1)
                        {
                            organizationIndexes.Add(organizationIndexItem);
                            organizationIndexItem = FindIndex(headerFields, organizationIndexItem + 1,"Organization","Club");
                        }

                        while (!parser.EndOfData)
                        {
                            //Process row
                            try
                            {
                                var fields = parser.ReadFields();
                                if (fields == null || fields.Length < 2)
                                    continue;

                                Person fighter;

                                if (firstNameIndex != -1 && lastNameIndex != -1 && fields.Length > firstNameIndex &&
                                    fields.Length > lastNameIndex &&
                                    (!string.IsNullOrWhiteSpace(fields[firstNameIndex]) ||
                                     !string.IsNullOrWhiteSpace(fields[lastNameIndex])))
                                {
                                    if (lastNamePrefixIndex != -1 && fields.Length > lastNamePrefixIndex)
                                    {
                                        fighter = session.QueryOver<Person>().Where(x =>
                                            x.FirstName.IsInsensitiveLike(fields[firstNameIndex]) &&
                                            x.LastNamePrefix.IsInsensitiveLike(fields[lastNamePrefixIndex]) &&
                                            x.LastName.IsInsensitiveLike(fields[lastNameIndex])).SingleOrDefault();
                                    }
                                    else
                                    {
                                        fighter = session.QueryOver<Person>().Where(x =>
                                            x.FirstName.IsInsensitiveLike(fields[firstNameIndex]) &&
                                            x.LastName.IsInsensitiveLike(fields[lastNameIndex])).SingleOrDefault();
                                    }

                                    if (fighter == null)
                                    {
                                        string fullName;
                                        if (fullNameIndex != -1 && fields.Length > fullNameIndex && !string.IsNullOrWhiteSpace(fields[fullNameIndex]))
                                        {
                                            fullName = fields[fullNameIndex];
                                        }
                                        else
                                        {
                                            fullName = fields[firstNameIndex];
                                            if (lastNamePrefixIndex != -1 && fields.Length > lastNamePrefixIndex &&
                                                !string.IsNullOrWhiteSpace(fields[lastNamePrefixIndex]))
                                            {
                                                if (string.IsNullOrWhiteSpace(fullName))
                                                    fullName += " ";
                                                fullName += fields[lastNamePrefixIndex];
                                            }

                                            if (!string.IsNullOrWhiteSpace(fields[lastNameIndex]))
                                            {
                                                if (string.IsNullOrWhiteSpace(fullName))
                                                    fullName += " ";
                                                fullName += fields[lastNameIndex];
                                            }
                                        }

                                        fighter = session.QueryOver<Person>()
                                            .Where(x => x.FullName.IsInsensitiveLike(fullName)).SingleOrDefault();
                                        if (fighter != null)
                                        {
                                            fighter.FirstName = fields[firstNameIndex];
                                            fighter.LastName = fields[lastNameIndex];
                                            if (lastNamePrefixIndex != -1 && fields.Length > lastNamePrefixIndex)
                                            {
                                                fighter.LastNamePrefix = fields[lastNamePrefixIndex];
                                            }
                                        }
                                    }

                                    if (fighter == null)
                                    {
                                        fighter = new Person
                                        {
                                            FirstName = fields[firstNameIndex],
                                            LastName = fields[lastNameIndex],
                                        };
                                        if (lastNamePrefixIndex != -1 && fields.Length > lastNamePrefixIndex)
                                        {
                                            fighter.LastNamePrefix = fields[lastNamePrefixIndex];
                                        }
                                        if (fullNameIndex != -1 && fields.Length > fullNameIndex)
                                        {
                                            fighter.FullName = fields[fullNameIndex];
                                        }
                                        using (var transaction = session.BeginTransaction())
                                        {
                                            session.Save(fighter);
                                            transaction.Commit();
                                        }
                                    }
                                }
                                else if (fullNameIndex != -1 && fields.Length > fullNameIndex &&
                                         !string.IsNullOrWhiteSpace(fields[fullNameIndex]))
                                {
                                    fighter = session.QueryOver<Person>()
                                        .Where(x => x.FullName.IsInsensitiveLike(fields[fullNameIndex]))
                                        .SingleOrDefault();
                                    if (fighter == null)
                                    {
                                        fighter = new Person
                                        {
                                            FullName = fields[0]
                                        };
                                        using (var transaction = session.BeginTransaction())
                                        {
                                            session.Save(fighter);
                                            transaction.Commit();
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                var competitionFighter =
                                    competition.Fighters.SingleOrDefault(x => x.Fighter.Id == fighter.Id);
                                if (competitionFighter == null)
                                {
                                    competitionFighter = new CompetitionFighter
                                        {Competition = competition, Fighter = fighter};
                                    using (var transaction = session.BeginTransaction())
                                    {
                                        session.Save(competitionFighter);
                                        transaction.Commit();
                                    }

                                    competition.Fighters.Add(competitionFighter);
                                }

                                if (seedIndex != -1 && fields.Length > seedIndex)
                                {
                                    double? newSeed = null;
                                    if (double.TryParse(fields[seedIndex], NumberStyles.Any,
                                        CultureInfo.InvariantCulture, out var seed))
                                    {
                                        newSeed = seed;
                                    }

                                    if (competitionFighter.Seed != newSeed)
                                    {
                                        competitionFighter.Seed = newSeed;
                                        using (var transaction = session.BeginTransaction())
                                        {
                                            session.Update(competitionFighter);
                                            transaction.Commit();
                                        }
                                    }

                                }

                                if (countryIndex != -1 && fields.Length > countryIndex)
                                {
                                    var p = Country.Countries.SingleOrDefault(x => string.Equals(x.Key,
                                        fields[countryIndex],
                                        StringComparison.InvariantCultureIgnoreCase));
                                    if (!string.IsNullOrWhiteSpace(p.Key))
                                    {
                                        fighter.CountryCode = p.Key;
                                    }
                                    else
                                    {
                                        p = Country.Countries.SingleOrDefault(x => string.Equals(x.Value,
                                            fields[countryIndex],
                                            StringComparison.InvariantCultureIgnoreCase));
                                        if (!string.IsNullOrWhiteSpace(p.Key))
                                        {
                                            fighter.CountryCode = p.Key;
                                        }
                                    }
                                }

                                if (organizationIndexes.Any())
                                {
                                    fighter.Organizations.Clear();
                                    foreach (var organizationIndex in organizationIndexes)
                                    {
                                        if (organizationIndex >= fields.Length ||
                                            string.IsNullOrWhiteSpace(fields[organizationIndex]))
                                            continue;

                                        var organization = organizations.SingleOrDefault(x =>
                                            string.Equals(x.Name, fields[organizationIndex],
                                                StringComparison.InvariantCultureIgnoreCase) ||
                                            x.Aliases.Any(alias =>
                                                string.Equals(alias, fields[organizationIndex],
                                                    StringComparison.InvariantCultureIgnoreCase)));
                                        if (organization == null)
                                        {
                                            var multiOrganization =
                                                fields[organizationIndex].Split('/', '\\', ',', '+');
                                            if (multiOrganization.Length > 1 && organizations.Any(x =>
                                                    string.Equals(x.Name, multiOrganization[0].Trim(),
                                                        StringComparison.InvariantCultureIgnoreCase) ||
                                                    x.Aliases.Any(alias =>
                                                        string.Equals(alias, multiOrganization[0].Trim(),
                                                            StringComparison.InvariantCultureIgnoreCase))))
                                            {
                                                foreach (var org in multiOrganization)
                                                {
                                                    organization = organizations.SingleOrDefault(x =>
                                                        string.Equals(x.Name, org.Trim(),
                                                            StringComparison.InvariantCultureIgnoreCase) ||
                                                        x.Aliases.Any(alias =>
                                                            string.Equals(alias, org.Trim(),
                                                                StringComparison.InvariantCultureIgnoreCase)));
                                                    if (organization == null)
                                                    {
                                                        organization = new Organization
                                                        {
                                                            Name = org.Trim()
                                                        };
                                                        using (var transaction = session.BeginTransaction())
                                                        {
                                                            session.Save(organization);
                                                            transaction.Commit();
                                                        }

                                                        organizations.Add(organization);
                                                    }

                                                    if (!fighter.Organizations.Contains(organization))
                                                        fighter.Organizations.Add(organization);
                                                }
                                            }
                                            else
                                            {
                                                organization = new Organization
                                                {
                                                    Name = fields[organizationIndex]
                                                };
                                                using (var transaction = session.BeginTransaction())
                                                {
                                                    session.Save(organization);
                                                    transaction.Commit();
                                                }

                                                organizations.Add(organization);
                                                fighter.Organizations.Add(organization);
                                            }
                                        }
                                        else
                                        {
                                            if (!fighter.Organizations.Contains(organization))
                                                fighter.Organizations.Add(organization);
                                        }
                                    }
                                }

                                using (var transaction = session.BeginTransaction())
                                {
                                    session.Update(fighter);
                                    transaction.Commit();
                                }
                            }
                            catch (MalformedLineException ex)
                            {
                                //parser.ErrorLine;
                                //parser.ErrorLineNumber;
                            }
                        }
                    }
                }
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(competition);
                    transaction.Commit();
                }

                NHibernateUtil.Initialize(competition.Fighters);
                foreach (var person in competition.Fighters)
                {
                    NHibernateUtil.Initialize(person.Fighter);
                    NHibernateUtil.Initialize(person.Fighter.Organizations);
                }
                NHibernateUtil.Initialize(competition.Matches);
                NHibernateUtil.Initialize(competition.Phases);
                NHibernateUtil.Initialize(competition.MatchRules);
                NHibernateUtil.Initialize(competition.RankingRules);
                return new CompetitionDetailView(competition);
            }
        }

        private int FindIndex(IList<string> fields, int start, params string[] options)
        {
            for (var i = start; i < fields.Count; i++)
            {
                foreach (var option in options)
                {
                    if (fields[i].Trim().ToLowerInvariant() == option.Trim().ToLowerInvariant())
                        return i;
                }
            }
            return -1;
        }
    }

    public class CompetitionCreateRequest
    {
        public virtual string CompetitionName { get; set; }
        public virtual string OrganizationName { get; set; }
    }
}