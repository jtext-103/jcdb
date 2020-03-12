using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Jtext103.JDBC.Core.Models;
using System.Collections.Specialized;
using System.Web.Http.Description;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 处理信号表达式
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExpressionController : BaseController {
        /// <summary>
        /// 计算信号表达式，返回数据
        /// </summary>
        /// <returns></returns>
        [Route("expression/{*value}")]
        public async Task<HttpResponseMessage> Post() {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            //如果ExpressionRoot创建时间不是今天，则进行重置
            if (BusinessConfig.ExpressionRoot.CreatedTime.DayOfYear != DateTime.Now.DayOfYear) {
                BusinessConfig.SetExpressionRoot();
            }
            try {
                if (user == null) {
                    throw new Exception("Not authorization!");
                }
                NameValueCollection form = Request.Content.ReadAsFormDataAsync().Result;
                //get data
                var type = form.GetValues("type").FirstOrDefault();
                var expression = form.GetValues("expression").FirstOrDefault();
                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(expression))
                {
                    throw new Exception("Arguments can not be empty!");
                }
                expression = expression.Replace("\r\n","");
                var newExpressionName = Guid.NewGuid().ToString();
                var newExpressionSignal = MyCoreApi.CreateSignal("Expression", newExpressionName);
                newExpressionSignal.AddExtraInformation("expression", expression);
                await MyCoreApi.AddOneToExperimentAsync(BusinessConfig.ExpressionRoot.Id, newExpressionSignal);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(SerializeObjectToString("/expression/"+newExpressionName), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            } catch (Exception e) {
                var message = e.InnerException != null ? e.InnerException.Message : e.Message;
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(message, System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }
    }
}
