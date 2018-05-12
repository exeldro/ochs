using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin.Security;
using NHibernate;
using NHibernate.Criterion;

namespace Ochs
{
    public class AuthController : ApiController
    {
        [HttpPost]
        [AllowAnonymous]
        public bool Login([FromBody]LoginRequest login)
        {
            if (string.IsNullOrWhiteSpace(login?.Username) || string.IsNullOrWhiteSpace(login.Password))
                return false;
            using (var session = NHibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var user = session.QueryOver<User>()
                        .Where(x => x.Name.IsInsensitiveLike(login.Username) && x.HashedPassword == Hash.getHashSha256(login.Password))
                        .SingleOrDefault();
                    if (user == null)
                        return false;

                    user.LastLogin = DateTime.Now;
                    session.Update(user);
                    transaction.Commit();

                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, user.Name));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    //claims.Add(new Claim(ClaimTypes.Email, "brockallen@gmail.com"));
                    var roles = session.QueryOver<UserRole>().Where(x => x.User == user && x.Organization == null).List();
                    claims.AddRange(roles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.ToString())));
                    var id = new ClaimsIdentity(claims, "ApplicationCookie");
                    var authenticationManager = Request.GetOwinContext().Authentication;
                    authenticationManager.SignOut("ApplicationCookie");
                    authenticationManager.SignIn(id);
                    return true;
                }
            }
        }

        [HttpGet]
        public void Logout()
        {
            var authenticationManager = Request.GetOwinContext().Authentication;
            authenticationManager.SignOut("ApplicationCookie");
        }

        [HttpGet]
        [AllowAnonymous]
        public IList<UserView> All()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.QueryOver<User>().List().Select(x => new UserView(x)).ToList();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public UserDetailView Get(Guid id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var user = session.QueryOver<User>().Where(x => x.Id == id).SingleOrDefault();
                if (user == null)
                    return null;
                NHibernateUtil.Initialize(user.Roles);
                return new UserDetailView(user);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public UserView Create([FromBody] LoginRequest login)
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
                return null;

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var user = session.QueryOver<User>().Where(x => x.Name.IsInsensitiveLike(login.Username)).SingleOrDefault();
                if (user != null)
                    return null;
                user = new User{Name = login.Username, HashedPassword = Hash.getHashSha256(login.Password) };
                session.Save(user);
                transaction.Commit();
                return new UserView(user);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public UserRoleView AddRole([FromBody] AddRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OrganizationName))
                return null;

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var user = session.QueryOver<User>().Where(x => x.Id == request.UserId).SingleOrDefault();
                if (user == null)
                    return null;

                var organization = session.QueryOver<Organization>().Where(x => x.Name.IsInsensitiveLike(request.OrganizationName))
                    .SingleOrDefault();
                if (organization == null)
                {
                    return null;
                }

                //check rights
                var i = Request.GetOwinContext().Authentication?.User?.Identity as ClaimsIdentity;
                var idstring = i?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(idstring))
                    return null;
                var id = new Guid(idstring);
                var authUser = session.QueryOver<User>().Where(x => x.Id == id).SingleOrDefault();
                if (authUser == null)
                    return null;

                if(!authUser.Roles.Any(x=>x.Role == UserRoles.Admin && (x.Organization == null || x.Organization.Id == organization.Id)))
                    return null;

                var userRole = user.Roles.SingleOrDefault(x => x.Organization == organization && x.Role == request.Role);
                if (userRole != null)
                    return null;

                userRole = new UserRole{Organization = organization, User = user, Role = request.Role};
                session.Save(userRole);
                transaction.Commit();
                return new UserRoleView(userRole);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public bool DeleteRole([FromBody] DeleteRoleRequest request)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var userRole = session.QueryOver<UserRole>().Where(x => x.Id == request.RoleId).SingleOrDefault();
                if (userRole == null)
                    return false;
                if (userRole.Role == UserRoles.Admin)
                    return false;

                session.Delete(userRole);
                transaction.Commit();
                return true;
            }
        }

        [HttpGet]
        public string CurrentUser()
        {            
            var i = Request.GetOwinContext().Authentication?.User?.Identity as ClaimsIdentity;
            return i?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        }
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AddRoleRequest
    {
        public Guid UserId { get; set; }
        public UserRoles Role { get; set; }
        public virtual string OrganizationName { get; set; }
    }

    public class DeleteRoleRequest
    {
        public Guid RoleId { get; set; }
    }
}