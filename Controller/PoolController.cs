using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate;

namespace Ochs
{
    public class PoolController : ApiController
    {

        [HttpGet]
        public PoolDetailView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var pool = session.QueryOver<Pool>().Where(x => x.Id == id).SingleOrDefault();
                if (pool == null)
                    return null;
                NHibernateUtil.Initialize(pool.Fighters);
                foreach (var person in pool.Fighters)
                {
                    NHibernateUtil.Initialize(person.Organizations);
                }
                NHibernateUtil.Initialize(pool.Matches);
                return new PoolDetailView(pool);
            }
        }
    }
}