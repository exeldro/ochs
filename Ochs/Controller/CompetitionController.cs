using System;
using System.Collections.Generic;
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
                NHibernateUtil.Initialize(competition.Fighters);
                foreach (var person in competition.Fighters)
                {
                    NHibernateUtil.Initialize(person.Organizations);
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
                    while (!parser.EndOfData)
                    {
                        //Process row
                        try
                        {
                            var fields = parser.ReadFields();
                            if(fields == null || fields.Length < 2)
                                continue;
                            Person fighter;
                            var countryIndex = 1;
                            if (fields.Length >= 4)
                            {
                                countryIndex = 3;
                                if (string.IsNullOrWhiteSpace(fields[0]) || string.IsNullOrWhiteSpace(fields[2]))
                                    continue;
                                fighter = session.QueryOver<Person>().Where(x =>
                                    x.FirstName.IsInsensitiveLike(fields[0]) &&
                                    x.LastNamePrefix.IsInsensitiveLike(fields[1]) &&
                                    x.LastName.IsInsensitiveLike(fields[2])).SingleOrDefault();
                                if (fighter == null)
                                {
                                    fighter = new Person
                                    {
                                        FirstName = fields[0],
                                        LastNamePrefix = fields[1],
                                        LastName = fields[2],
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
                                if (string.IsNullOrWhiteSpace(fields[0]))
                                    continue;
                                fighter = session.QueryOver<Person>().Where(x =>
                                    x.FullName.IsInsensitiveLike(fields[0])).SingleOrDefault();
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
                            if (!competition.Fighters.Contains(fighter))
                            {
                                competition.Fighters.Add(fighter);
                            }

                            if (fields.Length > countryIndex)
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

                            fighter.Organizations.Clear();
                            for (int organizationIndex = countryIndex+1; organizationIndex < fields.Length; organizationIndex++)
                            {
                                if(string.IsNullOrWhiteSpace(fields[organizationIndex]))
                                    continue;

                                var organization = organizations.SingleOrDefault(x =>
                                    string.Equals(x.Name, fields[organizationIndex], StringComparison.InvariantCultureIgnoreCase) ||
                                    x.Aliases.Any(alias =>
                                        string.Equals(alias, fields[organizationIndex], StringComparison.InvariantCultureIgnoreCase)));
                                if (organization == null)
                                {
                                    var multiOrganization = fields[organizationIndex].Split('/', '\\', ',', '+');
                                    if (multiOrganization.Length > 1 && organizations.Any(x =>
                                            string.Equals(x.Name, multiOrganization[0].Trim(), StringComparison.InvariantCultureIgnoreCase) ||
                                            x.Aliases.Any(alias =>
                                                string.Equals(alias, multiOrganization[0].Trim(), StringComparison.InvariantCultureIgnoreCase))))
                                    {
                                        foreach (var org in multiOrganization)
                                        {
                                            organization = organizations.SingleOrDefault(x =>
                                                string.Equals(x.Name, org.Trim(), StringComparison.InvariantCultureIgnoreCase) ||
                                                x.Aliases.Any(alias =>
                                                    string.Equals(alias, org.Trim(), StringComparison.InvariantCultureIgnoreCase)));
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
                                            if(!fighter.Organizations.Contains(organization))
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
                                    if(!fighter.Organizations.Contains(organization))
                                        fighter.Organizations.Add(organization);
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
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(competition);
                    transaction.Commit();
                }

                NHibernateUtil.Initialize(competition.Fighters);
                foreach (var person in competition.Fighters)
                {
                    NHibernateUtil.Initialize(person.Organizations);
                }
                NHibernateUtil.Initialize(competition.Matches);
                NHibernateUtil.Initialize(competition.Phases);
                NHibernateUtil.Initialize(competition.MatchRules);
                return new CompetitionDetailView(competition);
            }
        }
    }

    public class CompetitionCreateRequest
    {
        public virtual string CompetitionName { get; set; }
        public virtual string OrganizationName { get; set; }
    }
}