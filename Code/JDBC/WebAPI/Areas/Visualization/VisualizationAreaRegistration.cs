using System.Web.Mvc;

namespace WebAPI.Areas.Visualization
{
    /// <summary>
    /// 可视化MVC模块
    /// </summary>
    public class VisualizationAreaRegistration : AreaRegistration 
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public override string AreaName 
        {
            get 
            {
                return "Visualization";
            }
        }
        /// <summary>
        /// 注册模块路由
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Visualization_default",
                "Visualization/{controller}/{action}/{id}",
                new { controller = "Visualization", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}