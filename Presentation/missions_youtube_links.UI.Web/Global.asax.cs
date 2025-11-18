using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace missions_youtube_links.UI.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
        public void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

            var assembly_version = Assembly.GetAssembly(typeof(MvcApplication)).GetName().Version.ToString();
            //var dll_ver = System.Reflection.Assembly.GetAssembly(typeof(MvcApplication)).GetName().Version.ToString();
            Session.Add("version", assembly_version);

        }





    }
}
