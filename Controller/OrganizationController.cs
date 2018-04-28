using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using Microsoft.VisualBasic.FileIO;
using NHibernate.Transform;

namespace Ochs
{
    public class OrganizationController : ApiController
    {
        [HttpGet]
        public IList<string> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Organization>().List().Select(x => x.Name).ToList();
            }
        }

        [HttpGet]
        public IList<OrganizationDetailView> AllWithDetails()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Organization>().Fetch(x => x.Aliases).Eager.TransformUsing(Transformers.DistinctRootEntity).List().Select(x=> new OrganizationDetailView(x)).ToList();
            }
        }

        [HttpGet]
        public string Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<Organization>().Where(x => x.Id == id).List().Select(x => x.Name)
                    .SingleOrDefault();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IList<OrganizationDetailView> UploadOrganizations()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var organizations = session.QueryOver<Organization>().List();
                using (var parser = new TextFieldParser(Request.Content.ReadAsStreamAsync().Result))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters("\t", ";");
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.TrimWhiteSpace = true;
                    while (!parser.EndOfData)
                    {
                        //Process row
                        try
                        {
                            var fields = parser.ReadFields();
                            if (fields == null || fields.Length < 1 || string.IsNullOrWhiteSpace(fields[0]))
                                continue;

                            var organization = organizations.SingleOrDefault(x =>
                                string.Equals(x.Name, fields[0], StringComparison.InvariantCultureIgnoreCase) ||
                                x.Aliases.Any(alias =>
                                    string.Equals(alias, fields[0], StringComparison.InvariantCultureIgnoreCase)));
                            var aliasIndex = 1;
                            while (organization == null && aliasIndex < fields.Length)
                            {
                                organization = organizations.SingleOrDefault(x =>
                                    string.Equals(x.Name, fields[aliasIndex], StringComparison.InvariantCultureIgnoreCase) ||
                                    x.Aliases.Any(alias =>
                                        string.Equals(alias, fields[aliasIndex], StringComparison.InvariantCultureIgnoreCase)));
                                aliasIndex++;
                            }
                            if (organization == null)
                            {
                                organization = new Organization
                                {
                                    Name = fields[0]
                                };
                                using (var transaction = session.BeginTransaction())
                                {
                                    session.Save(organization);
                                    transaction.Commit();
                                }
                            }
                            organization.Name = fields[0];
                            for (aliasIndex = 1; aliasIndex < fields.Length; aliasIndex++)
                            {
                                if(string.IsNullOrWhiteSpace(fields[aliasIndex]))
                                    continue;
                                if (!organization.Aliases.Any(alias =>
                                    string.Equals(alias, fields[aliasIndex],
                                        StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    organization.Aliases.Add(fields[aliasIndex]);
                                }
                            }

                            using (var transaction = session.BeginTransaction())
                            {
                                session.Update(organization);
                                transaction.Commit();
                            }
                        }
                        catch (MalformedLineException ex)
                        {
                            //parser.ErrorLine;
                            //parser.ErrorLineNumber;
                        }
                    }
                }
                return session.QueryOver<Organization>().Fetch(x => x.Aliases).Eager.TransformUsing(Transformers.DistinctRootEntity).List().Select(x=> new OrganizationDetailView(x)).ToList();
            }
        }
    }
}