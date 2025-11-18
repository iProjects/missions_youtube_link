using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(missions_youtube_links.UI.Web.Startup))]
namespace missions_youtube_links.UI.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
