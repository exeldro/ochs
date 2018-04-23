using System;
using System.Collections.Generic;

namespace Ochs
{
    public class Competition
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Start { get; set; }
        public virtual DateTime? Stop { get; set; }
        public virtual MatchRules MatchRules { get; set; }
        public virtual RankingRules RankingRules { get; set; }
        public virtual IList<Phase> Phases { get; set; } = new List<Phase>();
        public virtual IList<Match> Matches { get; set; } = new List<Match>();
        public virtual IList<Person> Fighters { get; set; } = new List<Person>();
        public virtual Organization Organization { get; set; }
    }
}