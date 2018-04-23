using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NHibernate;

namespace Ochs
{
    public class PhaseController : ApiController
    {

        [HttpGet]
        public PhaseDetailView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var phase = session.QueryOver<Phase>().Where(x => x.Id == id).SingleOrDefault();
                if (phase == null)
                    return null;
                NHibernateUtil.Initialize(phase.Fighters);
                foreach (var person in phase.Fighters)
                {
                    NHibernateUtil.Initialize(person.Organizations);
                }
                NHibernateUtil.Initialize(phase.Matches);
                NHibernateUtil.Initialize(phase.Pools);
                return new PhaseDetailView(phase);
            }
        }
    }
}