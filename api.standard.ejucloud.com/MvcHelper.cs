using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace api.standard.ejucloud.com
{
    public class MvcHelper
    {
        private string[] namespaces;

        private MvcHelper(string[] namespaces)
        {
            this.namespaces = namespaces;
        }

        /// <summary>
        /// 创建一个新的MVC帮助对象
        /// </summary>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public static MvcHelper Create(params string[] namespaces)
        {
            return new MvcHelper(namespaces);
        }


        /// <summary>
        /// 映射一个路由
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controller"></param>
        public void MapRoute(string url, string controller)
        {
            string action = url;
            int index = url.IndexOf('/');
            if (index > 0)
            {
                action = url.Substring(0, index);
            }
            MapRoute(url, controller, action);
        }

        /// <summary>
        /// 映射一个路由
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        public void MapRoute(string url, string controller, string action)
        {
            action = action ?? "index";
            RouteTable.Routes.MapRoute(
                url, // Route name
                url, // URL with parameters
                new { controller = controller, action = action }, // Parameter defaults
                new { },
                namespaces
            );
        }

        /// <summary>
        /// 映射一个路由
        /// </summary>
        /// <param name="url"></param>
        /// <param name="routes"></param>
        public void MapRoute(string url, object routes, object constraints = null)
        {
            RouteTable.Routes.MapRoute(
                url, // Route name
                url, // URL with parameters
                routes, // Parameter defaults
                constraints,
                namespaces
            );
        }
    }
}