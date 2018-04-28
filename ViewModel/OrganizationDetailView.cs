using System.Collections.Generic;

namespace Ochs
{
    public class OrganizationDetailView
    {
        public virtual string Name { get; set; }
        public virtual IList<string> Aliases { get; set; }
    }
}