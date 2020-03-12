using System.Web.Http;
using System.Web.Mvc;

namespace WebAPI.Areas.HelpPage
{
    /// <summary>
    /// 文档帮助模块
    /// </summary>
    public class HelpPageAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }
        /// <summary>
        /// 注册模块路由
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HelpPage_Default",
                "Help/{action}/{apiId}",
                new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}