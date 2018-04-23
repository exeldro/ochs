using System;
using System.Collections.Generic;

namespace Ochs
{
    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string LastNamePrefix { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual IList<Organization> Organizations { get; set; } = new List<Organization>();

        public virtual string FullName
        {
            get
            {
                var fullName = "";
                if (!string.IsNullOrWhiteSpace(FirstName))
                {
                    fullName += FirstName;
                }
                if (!string.IsNullOrWhiteSpace(LastNamePrefix))
                {
                    fullName += " " + LastNamePrefix;
                }
                if (!string.IsNullOrWhiteSpace(LastName))
                {
                    fullName += " " + LastName;
                }
                return fullName.Trim();
            }
        }

        public override string ToString() => FullName;
    }
}