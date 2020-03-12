using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebAPI
{
    public class NotVerbRouteConstraint : IRouteConstraint
    {
        public bool Match(
          HttpContextBase httpContext, Route route, string parameterName,
          RouteValueDictionary values, RouteDirection routeDirection)
        {
            var verbs = new List<string> { "get", "post", "delete", "put"};
            return !verbs.Contains(values[parameterName].ToString().ToLower());
        }
    }
    /// <summary>
    /// Web API配置
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// 注册路由
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
            config.EnableCors();
            // Web API 路由
            config.MapHttpAttributeRoutes();
            /*
            config.Routes.MapHttpRoute(
                name: "NormalDefault",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
                constraints: new { action = new NotVerbRouteConstraint() }
            );

            config.Routes.MapHttpRoute(
                name: "EntityDefaultApi",
                routeTemplate: "Entity/{*value}",
                defaults: new { controller = "Entity" }
            );
            config.Routes.MapHttpRoute(
                name: "DataDefaultApi",
                routeTemplate: "Data/{*value}",
                defaults: new { controller = "Data" }
            );
            config.Routes.MapHttpRoute(
                name: "StreamDefaultApi",
                routeTemplate: "Stream/{*value}",
                defaults: new { controller = "Stream" }
            );
            config.Routes.MapHttpRoute(
                name: "PermissionDefaultApi",
                routeTemplate: "Permission/{*value}",
                defaults: new { controller = "Permission" }
            );
            config.Routes.MapHttpRoute(
                name: "ExpressionDefaultApi",
                routeTemplate: "Expression/{*value}",
                defaults: new { controller = "Expression" }
            );*/
        }
    }
}
