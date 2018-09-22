using System;
using System.Web.Http;
using NHibernate;

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