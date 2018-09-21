using System;
using System.IO;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;

namespace Ochs
{
    public static class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory != null) return _sessionFactory;
                if (!File.Exists("hibernate.cfg.xml") && File.Exists("hibernate.cfg.SQLite.xml"))
                {
                    File.Copy("hibernate.cfg.SQLite.xml", "hibernate.cfg.xml");
                }
                _sessionFactory = Fluently.Configure(new Configuration().Configure())
                    .Mappings(m =>m.FluentMappings.AddFromAssemblyOf<Program>())
                    .ExposeConfiguration(c => new SchemaUpdate(c).Execute(true,true))
                    .BuildSessionFactory();
                using (var session = _sessionFactory.OpenSession())
                {
                    if (session.QueryOver<User>().RowCount() == 0)
                    {
                        using (var transaction = session.BeginTransaction())
                        {
                            var password = System.Web.Security.Membership.GeneratePassword(8, 0);
                            Console.WriteLine("Admin password: " + password);
                            var user = new User {Name = "Admin", HashedPassword = Hash.getHashSha256(password)};
                            session.Save(user);
                            session.Save(new UserRole {Role = UserRoles.Admin, User = user});
                            transaction.Commit();
                        }
                    }
                }
                return _sessionFactory;
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}