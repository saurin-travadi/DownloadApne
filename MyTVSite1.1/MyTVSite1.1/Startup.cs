using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyTVSite1._1.Startup))]
namespace MyTVSite1._1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
