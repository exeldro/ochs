using System;
using System.Collections.Generic;

namespace Ochs
{
    public class Organization
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }   
        public virtual IList<Competition> Competitions { get; set; } = new List<Competition>();
    }
}