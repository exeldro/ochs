using System.Collections.Generic;

namespace Ochs
{
    public class OrganizationDetailView
    {
        private readonly Organization _organization;

        public OrganizationDetailView(Organization organization)
        {
            _organization = organization;
        }

        public virtual string Name => _organization.Name;
        public virtual IList<string> Aliases => _organization.Aliases;
    }
}