using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BKS_Sys.App.Startup))]

namespace BKS_Sys.App
{
    public class Startup
    {
        public static void ConfigureSignalR(IAppBuilder app)
        {
            app.MapSignalR();
        }
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            BKS_Sys.App.Startup.ConfigureSignalR(app);
        }
    }
}
