using System;
using System.Configuration;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Ochs
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpoint = ConfigurationManager.AppSettings["endpoint"] ?? "http://*/ochs/";
            using (WebApp.Start(endpoint,Startup))
            {
                Console.WriteLine("Server running at "+endpoint);
                if (endpoint.Contains("*"))
                {
                    Console.WriteLine("Browse to: " + endpoint.Replace("*", Dns.GetHostName()));
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                            Console.WriteLine("Browse to: " + endpoint.Replace("*", ip.ToString()));
                    }
                }

                Console.WriteLine("Enter Q to exit");
                using (NHibernateHelper.OpenSession()){}

                bool isRunning = true;
                while (isRunning)
                {
                    isRunning = (Console.ReadLine()?.ToUpper() != "Q");   
                }
            }
        }

        private static void Startup(IAppBuilder appBuilder)
        {
            //appBuilder.UseApplicationSignInCookie();
            /*appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnApplyRedirect = ctx =>
                    {
                        if (!IsAjaxRequest(ctx.Request))
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                    }
                }
            });*/
            appBuilder.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            var provider = new CookieAuthenticationProvider();
            var originalHandler = provider.OnApplyRedirect;
            provider.OnApplyRedirect = context =>
            {
                
                if (!context.Request.Context.Request.Path.ToString().StartsWith("/api/"))
                {
                    context.RedirectUri = new Uri(context.RedirectUri).PathAndQuery;
                    originalHandler.Invoke(context);
                }
            };

            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Home/Login"),
                Provider = provider,
                CookieName = "ApplicationCookie",
                CookieHttpOnly = true,
                ExpireTimeSpan = TimeSpan.FromDays(1),
            });

            appBuilder.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    EnableJSONP = true,
                    EnableDetailedErrors = true,
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch is already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });

            
            var fileSystem = new PhysicalFileSystem("");
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = false,
                FileSystem = fileSystem,
                RequestPath = new PathString(""),
                EnableDefaultFiles = false
            };
            appBuilder.Use((context, next) =>
            {
                if (!context.Request.Path.HasValue)
                {

                }else if (context.Request.Path.Value == "/" ||
                          context.Request.Path.Value == "" || !context.Request.Path.Value.Substring(1).Contains("/"))
                {
                    context.Request.Path = new PathString("/index.html");
                }

                return next();
            });
            appBuilder.UseFileServer(options);

            var config = new HttpConfiguration();
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            //config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            //config.DependencyResolver = new SpringWebApiDependencyResolver(applicationContextParent);
            //config.ParameterBindingRules
            //config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            //config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            //config.Formatters.FormUrlEncodedFormatter.SupportedEncodings
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var url = "api";
            config.Routes.MapHttpRoute("Api1", url + "/{controller}/{action}");
            config.Routes.MapHttpRoute("Api3", url + "/{controller}/{action}/{id}");  //Match/Get/Guid

            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);

        }
    }
}
