using System;
using System.Collections.Generic;
using System.Linq;

namespace Ochs
{
    public class PhaseFighterView: PersonMatchesView
    {
        private Pool _pool;
        public PhaseFighterView(Person person, IList<Match> matches, IList<Pool> pools) : base(person, matches)
        {
            _pool = pools.SingleOrDefault(x => x.Fighters.Any(y=>y.Id == person.Id));
        }

        public virtual Guid? PoolId => _pool?.Id;
        public virtual string Pool => _pool?.Name;
    }
}