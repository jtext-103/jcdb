using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebAPI.Controllers;

namespace WebAPI
{
    /// <summary>
    /// 程序入口
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// 注册功能区、路由等
        /// 运行初始化Business代码
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();//注册应用程序中的所有区域
            GlobalConfiguration.Configure(WebApiConfig.Register);//注册Web API路由
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);//注册全局过滤器
            RouteConfig.RegisterRoutes(RouteTable.Routes);//注册MVC路由
            BundleConfig.RegisterBundles(BundleTable.Bundles);//注册捆绑

            BusinessConfig.ConfigBusiness();//执行初始化逻辑代码
        }
        /// <summary>
        /// 错误处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e) {
            var exception = Server.GetLastError();

            var httpContext = ((HttpApplication)sender).Context;
            httpContext.Response.Clear();
            httpContext.ClearError();
            ExecuteErrorController(httpContext, exception);
        }
        /// <summary>
        /// 返回错误控制程序
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="exception"></param>
        private void ExecuteErrorController(HttpContext httpContext, Exception exception) {
            var routeData = new RouteData();
            routeData.Values["controller"] = "Home";
            routeData.Values["action"] = "Error";
            routeData.Values["errorType"] = 10; //this is your error code. Can this be retrieved from your error controller instead?
            routeData.Values["exception"] = exception;

            using (Controller controller = new HomeController()) {
                ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
            }
        }
    }
}
