using System;
using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class PhaseFighterView: PersonView
    {
        private Pool _pool;
        public PhaseFighterView(Person person, IList<Pool> pools) : base(person)
        {
            _pool = pools.SingleOrDefault(x => x.Fighters.Any(y=>y.Id == person.Id));
        }

        public virtual Guid? PoolId => _pool?.Id;
        public virtual string Pool => _pool?.Name;
    }
}