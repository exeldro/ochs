using System;
using System.Collections.Generic;

namespace Ochs
{
    public class Phase
    {
        public virtual Guid Id { get; set; }
        public virtual Competition Competition { get; set; }
        public virtual string Name { get; set; }
        public virtual string Location { get; set; }
        public virtual IList<Match> Matches { get; set; } = new List<Match>();
        public virtual IList<Person> Fighters { get; set; } = new List<Person>();
        public virtual MatchRules MatchRules { get; set; }
        public virtual IList<Pool> Pools { get; set; } = new List<Pool>();
    }
}