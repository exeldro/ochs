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
    }
}