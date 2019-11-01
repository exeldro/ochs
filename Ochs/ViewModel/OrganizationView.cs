using System;

namespace Ochs
{
    public class OrganizationView
    {
        protected readonly Organization _organization;

        public OrganizationView(Organization organization)
        {
            _organization = organization;
        }
        public virtual Guid Id => _organization.Id;
        public virtual string Name => _organization.Name;
        public virtual string CountryCode => _organization.CountryCode;

        public virtual string CountryName => string.IsNullOrWhiteSpace(_organization.CountryCode) || !Country.Countries.ContainsKey(_organization.CountryCode)
            ? null
            : Country.Countries[_organization.CountryCode];
    }
}