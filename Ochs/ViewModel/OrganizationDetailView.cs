using System.Collections.Generic;

namespace Ochs
{
    public class OrganizationDetailView : OrganizationView
    {
        public OrganizationDetailView(Organization organization) : base(organization)
        {
        }
        public virtual IList<string> Aliases => _organization.Aliases;
    }
}