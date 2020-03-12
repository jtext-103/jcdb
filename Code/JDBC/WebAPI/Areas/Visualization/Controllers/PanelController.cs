using Jtext103.JDBC.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAPI.Areas.Visualization.Controllers
{
    /// <summary>
    /// 可视化工具控制器
    /// </summary>
    public class PanelController : Controller
    {
        /// <summary>
        /// 获取BusinessConfig中的CoreApi实例
        /// </summary>
        public static CoreApi MyCoreApi
        {
            get { return BusinessConfig.MyCoreApi; }
        }
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            Response.SetCookie(new HttpCookie("user", "root"));
            return View();
        }
        /// <summary>
        /// 概况面板
        /// </summary>
        /// <returns></returns>
        public ActionResult Dashboard()
        {
            return View();
        }
        /// <summary>
        /// 电子表格
        /// </summary>
        /// <returns></returns>
        public ActionResult Spreadsheet()
        {
            return View();
        }
        /// <summary>
        /// 曲线图
        /// </summary>
        /// <returns></returns>
        public ActionResult LineChart()
        {
            return View();
        }
        /// <summary>
        /// 散点图
        /// </summary>
        /// <returns></returns>
        public ActionResult ScatterChart()
        {
            return View();
        }
        /// <summary>
        /// 面积图
        /// </summary>
        /// <returns></returns>
        public ActionResult AreaChart()
        {
            return View();
        }
        /// <summary>
        /// 条形图
        /// </summary>
        /// <returns></returns>
        public ActionResult BarChart()
        {
            return View();
        }
        /// <summary>
        /// 柱形图
        /// </summary>
        /// <returns></returns>
        public ActionResult ColumnChart()
        {
            return View();
        }
        /// <summary>
        /// 饼状图
        /// </summary>
        /// <returns></returns>
        public ActionResult PieChart()
        {
            return View();
        }
        /// <summary>
        /// 圆环图
        /// </summary>
        /// <returns></returns>
        public ActionResult DonutChart()
        {
            return View();
        }
        /// <summary>
        /// 3D图
        /// </summary>
        public ActionResult ThreeDChart() {
            return View();
        }
        /// <summary>
        /// 轮廓图
        /// </summary>
        /// <returns></returns>
        public ActionResult ContourChart() {
            return View();
        }
    }
}