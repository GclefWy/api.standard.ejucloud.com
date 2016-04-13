using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace api.standard.ejucloud.com
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
           AreaRegistration.RegisterAllAreas();
            // 默认情况下对 Entity Framework 使用 LocalDB
            //Database.DefaultConnectionFactory = new SqlConnectionFactory(@"Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "Default", // 路由名称
            //    "{controller}/{action}/{id}", // 带有参数的 URL
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional } // 参数默认值
            //);

            MvcHelper helper = MvcHelper.Create("api.standard.ejucloud.com.Controllers");
            //用户信息接口路径
            helper.MapRoute("api/standarduseraddress/", "StandardAddress", "StandardUserAddress");
            helper.MapRoute("api/addressmatch/", "StandardAddress", "AddressMatch");


        }


    }
}

