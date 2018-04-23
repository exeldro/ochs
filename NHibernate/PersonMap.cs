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
            Map(x => x.CountryCode);
            HasManyToMany(x => x.Organizations);
        }
        
    }
}