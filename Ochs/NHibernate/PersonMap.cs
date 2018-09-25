using FluentNHibernate.Mapping;

namespace Ochs
{
    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            Id(x => x.Id);
            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.LastNamePrefix);
            Map(x => x.FullName);
            Map(x => x.CountryCode);
            Map(x => x.Gender);
            HasManyToMany(x => x.Organizations);
        }
        
    }
}