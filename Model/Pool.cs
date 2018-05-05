using System;
using System.Collections.Generic;

namespace Ochs
{
    public class Pool
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Location { get; set; }
        public virtual IList<Match> Matches { get; set; } = new List<Match>();
        public virtual IList<Person> Fighters { get; set; } = new List<Person>();
        public virtual Phase Phase { get; set; }
        public virtual DateTime? PlannedDateTime { get; set; }
    }
}