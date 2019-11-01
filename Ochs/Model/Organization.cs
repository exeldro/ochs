using System;
using System.Collections.Generic;

namespace Ochs
{
    public class Organization
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }   
        public virtual IList<Competition> Competitions { get; set; } = new List<Competition>();
        public virtual IList<string> Aliases { get; set; } = new List<string>();
        public virtual string CountryCode { get; set; }
        public virtual IList<Person> Persons { get; set; }
    }
}