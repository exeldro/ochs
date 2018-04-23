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
                return session.QueryOver<Competition>().List().Select(x => new CompetitionView(x)).ToList();
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
                NHibernateUtil.Initialize(competition.Matches);
                NHibernateUtil.Initialize(competition.Phases);
                return new CompetitionDetailView(competition);
            }
        }
        [HttpGet]
        public CompetitionDetailView GetRules(Guid id)
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
                NHibernateUtil.Initialize(competition.Matches);
                NHibernateUtil.Initialize(competition.Phases);
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
                            if(fields == null || fields.Length < 3)
                                continue;
                            if(string.IsNullOrWhiteSpace(fields[0]) || string.IsNullOrWhiteSpace(fields[2]))
                                continue;
                            var fighter = session.QueryOver<Person>().Where(x =>
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
                            else
                            {

                            }
                            if (!competition.Fighters.Contains(fighter))
                            {
                                competition.Fighters.Add(fighter);
                            }
                            if(fields.Length <= 4 || string.IsNullOrWhiteSpace(fields[4]))
                                continue;

                            fighter.Organizations.Clear();

                            var organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(fields[4])).SingleOrDefault();
                            if (organization == null)
                            {
                                organization = new Organization
                                {
                                    Name = fields[4]
                                };
                                using (var transaction = session.BeginTransaction())
                                {
                                    session.Save(organization);
                                    transaction.Commit();
                                }
                            }
                            fighter.Organizations.Add(organization);
                            if (fields.Length > 5 && !string.IsNullOrWhiteSpace(fields[5]))
                            {
                                organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(fields[5])).SingleOrDefault();
                                if (organization == null)
                                {
                                    organization = new Organization
                                    {
                                        Name = fields[5]
                                    };
                                    using (var transaction = session.BeginTransaction())
                                    {
                                        session.Save(organization);
                                        transaction.Commit();
                                    }
                                }
                                fighter.Organizations.Add(organization);
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
                return new CompetitionDetailView(competition);
            }
            //using (var transaction = session.BeginTransaction())
        }

    }

    public class CompetitionCreateRequest
    {
        public virtual string CompetitionName { get; set; }
        public virtual string OrganizationName { get; set; }
    }
}