using System;
using System.Linq;

namespace Ochs
{
    public class PersonView
    {
        private Person _person;

        public PersonView(Person person)
        {
            _person = person;
        }
        public virtual Guid Id => _person.Id;
        public virtual string FirstName => _person.FirstName;
        public virtual string LastNamePrefix => _person.LastNamePrefix;
        public virtual string LastName => _person.LastName;
        public virtual string FullName => _person.FullName;
        public virtual string DisplayName => _person.DisplayName;
        public virtual string CountryCode => _person.CountryCode;

        public virtual string CountryName => string.IsNullOrWhiteSpace(_person.CountryCode) || !Country.Countries.ContainsKey(_person.CountryCode)
            ? null
            : Country.Countries[_person.CountryCode];
        public virtual string Organization => string.Join(" / ",_person.Organizations.Select(x=>x.Name));
    }
}