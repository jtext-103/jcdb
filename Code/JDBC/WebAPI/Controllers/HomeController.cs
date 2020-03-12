using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 调试面板
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            Response.SetCookie(new HttpCookie("user", "root"));
            return View();
        }
        /// <summary>
        /// 错误页面
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        public ActionResult Error(Exception exception, int errorType) {
            Response.TrySkipIisCustomErrors = true;
            var httpException = exception as HttpException;
            Response.StatusCode = (httpException != null ? httpException.GetHttpCode() : (int)HttpStatusCode.InternalServerError);
            Response.ContentType = "text/html;charset=utf-8";
            System.Diagnostics.Debug.WriteLine(exception);
            ViewBag.Error = exception.Message;
            return View();
        }
    }
}
