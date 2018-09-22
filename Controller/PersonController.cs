using System;
using System.Linq;
using System.Web.Http;
using NHibernate;
using NHibernate.Criterion;

namespace Ochs
{
    public class PersonController : ApiController
    {
        [HttpGet]
        public PersonView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var person = session.QueryOver<Person>().Where(x => x.Id == id).SingleOrDefault();
                if (person == null)
                    return null;
                NHibernateUtil.Initialize(person.Organizations);
                return new PersonView(person);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public PersonView Save([FromBody]PersonDto personDto)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var person = session.QueryOver<Person>().Where(x => x.Id == personDto.Id).SingleOrDefault();
                if (person == null)
                    return null;

                person.FirstName = personDto.FirstName;
                if (!string.IsNullOrWhiteSpace(personDto.LastName))
                {
                    person.LastNamePrefix = personDto.LastNamePrefix;
                    person.LastName = personDto.LastName;
                }
                person.FullName = personDto.FullName;
                if (!string.IsNullOrWhiteSpace(personDto.CountryCode) &&
                    Country.Countries.ContainsKey(personDto.CountryCode))
                {
                    person.CountryCode = personDto.CountryCode;
                }

                using (var transaction = session.BeginTransaction())
                {
                    session.Update(person);
                    transaction.Commit();
                }
                NHibernateUtil.Initialize(person.Organizations);
                return new PersonView(person);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public PersonView AddOrganization([FromBody] PersonOrganizationDto personOrganizationDto)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var person = session.QueryOver<Person>().Where(x => x.Id == personOrganizationDto.PersonId).SingleOrDefault();
                if (person == null)
                    return null;
                NHibernateUtil.Initialize(person.Organizations);
                if(string.IsNullOrWhiteSpace(personOrganizationDto.Organization))
                    return new PersonView(person);
                var organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(personOrganizationDto.Organization)).SingleOrDefault();
                if (organization == null)
                    return new PersonView(person);
                if (person.Organizations.Contains(organization))
                    return new PersonView(person);
                person.Organizations.Add(organization);
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(person);
                    transaction.Commit();
                }
                return new PersonView(person);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public PersonView RemoveOrganization([FromBody] PersonOrganizationDto personOrganizationDto)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var person = session.QueryOver<Person>().Where(x => x.Id == personOrganizationDto.PersonId).SingleOrDefault();
                if (person == null)
                    return null;
                NHibernateUtil.Initialize(person.Organizations);
                if(string.IsNullOrWhiteSpace(personOrganizationDto.Organization))
                    return new PersonView(person);
                var organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(personOrganizationDto.Organization)).SingleOrDefault();
                if (organization == null)
                    return new PersonView(person);
                if (!person.Organizations.Contains(organization))
                    return new PersonView(person);
                person.Organizations.Remove(organization);
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(person);
                    transaction.Commit();
                }
                return new PersonView(person);
            }
        }
    }

    public class PersonOrganizationDto
    {
        public virtual Guid PersonId { get; set; }
        public virtual string Organization { get; set; }
    }

    public class PersonDto
    {
        public virtual Guid Id { get; set; }
        public virtual string FirstName{ get; set; }
        public virtual string LastNamePrefix { get; set; }
        public virtual string LastName { get; set; }
        public virtual string FullName { get; set; }
        public virtual string CountryCode { get; set; }
    }
}