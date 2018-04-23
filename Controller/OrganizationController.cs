using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate.Transform;

namespace Ochs
{
    public class OrganizationController : ApiController
    {
        [HttpGet]
        public IList<string> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Organization>().List().Select(x => x.Name).ToList();
            }
        }

        [HttpGet]
        public string Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Organization>().Where(x => x.Id == id).List().Select(x => x.Name).SingleOrDefault();
            }
        }
    }
}