using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(api.standard.ejucloud.com.Startup))]
namespace api.standard.ejucloud.com
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
