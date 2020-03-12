using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Text.RegularExpressions;
using BasicPlugins.TypedSignal;
using System.Diagnostics;
using System.Web.Http;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 统一返回结果
    /// </summary>
    public class ResponseDataMessage
    {
        /// <summary>
        /// 首次处理失败的信息
        /// </summary>
        public object Fail { get; set; }
        /// <summary>
        /// 处理成功的信息
        /// </summary>
        public List<object> Success { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public ResponseDataMessage()
        {
            Success = new List<object>();
        }
    }

    /// <summary>
    /// 处理Entity相关请求
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    public class DataController : BaseController
    {
        /// <summary>
        /// #根据要求获取Data#
        /// 查询根节点：/data/path/?_name=*
        /// </summary>
        /// <returns></returns>
        [Route("data/{*value}")]
        public async Task<HttpResponseMessage> Get(string format="array")
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                Uri uri = ParseToQueryUri(Request.RequestUri);
                List<JDBCEntity> findNode = (await MyCoreApi.FindNodeByUriAsync(uri)).ToList();//根据uri查找父节点
                if (findNode.Count != 1)//节点数不唯一
                {
                    throw new Exception("Please specify one Node!");
                } 
                else
                {
                    currentEntity  = findNode.FirstOrDefault();
                }
                if (!await MyCoreApi.Authorization(currentEntity.Id, user, "4")) {
                    throw new Exception("Not authorization!");
                }
                if (currentEntity.EntityType != JDBCEntityType.Signal)
                {
                    throw new Exception("The specifed node is not signal!");
                }

                //get data
                ITypedSignal typedSignal = currentEntity as ITypedSignal;
                var result = await typedSignal.GetDataAsync(uri.Fragment, format);
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(SerializeObjectToString(result), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if(currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseDataMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    }
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            
        }
        /// <summary>
        /// #根据要求提交Data#
        /// 目前支持int、double类型的数组
        /// </summary>
        /// <returns></returns>
        [Route("data/{*value}")]
        public async Task<HttpResponseMessage> Post()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            try {
                Uri uri = ParseToQueryUri(Request.RequestUri);
                Signal signal = (Signal)(await MyCoreApi.FindNodeByUriAsync(uri)).FirstOrDefault();//根据uri查找entity
                if (!await MyCoreApi.Authorization(signal.Id, user, "2")) {
                    throw new Exception("Not authorization!");
                }
                var re = Request;
                var con1 = Request.Content;
                var conent = Request.Content.ReadAsStringAsync().Result;
                Regex reg = new Regex(@"^(\[)|(\])$");
                conent = reg.Replace(conent, "");
                switch (signal.SampleType) {
                    case "int":
                        int[] ints = Array.ConvertAll<string, int>(conent.Replace(" ", "").Split(new char[] { '，', ',' }), s => int.Parse(s));
                        await ((ITypedSignal)signal).PutDataAsync("", ints);
                        await ((FixedIntervalWaveSignal)signal).DisposeAsync();
                        break;
                    case "double":
                        double[] doubles = Array.ConvertAll<string, double>(conent.Replace(" ", "").Split(new char[] { '，', ',' }), s => double.Parse(s));
                     //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "  start put data");
                        await ((ITypedSignal)signal).PutDataAsync("", doubles);
                     //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "  finish put data");
                        await ((FixedIntervalWaveSignal)signal).DisposeAsync();
                     //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "  dispose put data");
                        break;
                    default:
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("The SampleType is not supported!") };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Write data successfully!") };
            } catch (Exception e) {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
            }
        }
        /// <summary>
        /// #根据要求修改Data#
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("data/{*value}")]
        public HttpResponseMessage Put()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
        /// <summary>
        /// #根据要求删除Data#
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("data/{*value}")]
        public HttpResponseMessage Delete()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
    }
}