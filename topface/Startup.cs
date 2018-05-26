using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(topface.Startup))]
namespace topface
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
