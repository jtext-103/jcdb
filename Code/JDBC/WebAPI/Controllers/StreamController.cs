using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebAPI.Models;
using System.Text.RegularExpressions;
using System.Web.Http;
using ProtoBuf;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 处理Stream相关请求
    /// </summary>
    public class StreamController : BaseController
    {
        /// <summary>
        /// #根据要求获取Stream#
        /// 查询根节点：/stream/path/?_name=*
        /// </summary>
        /// <returns></returns>
        [Route("stream/{*value}")]
        public async Task<HttpResponseMessage> Get()
        {
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
                    currentEntity = findNode.FirstOrDefault();
                }
                if (currentEntity.EntityType != JDBCEntityType.Signal)
                {
                    throw new Exception("The specifed node is not signal!");
                }
                var stream = new SignalDataStream((ITypedSignal)currentEntity, uri.Fragment);

                var response = Request.CreateResponse();
                response.Content = new PushStreamContent(stream.WriteToStream, new MediaTypeHeaderValue("data/stream"));

                return response;
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message) };
                }
                var response = new ResponseDataMessage
                {
                    Fail = new
                    {
                        Description = e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    }
                };
                StringWriter tw = new StringWriter();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(tw, response, response.GetType());
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(tw.ToString(), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// #根据要求提交Stream#
        /// </summary>
        /// <returns></returns>
        [Route("stream/{*value}")]
        public async Task<HttpResponseMessage> Post()
        {
            try {
                // Verify that this is JSON content
                var content = Request.Content;
                if (content == null) {
                    throw new ArgumentNullException("content");
                }
                if (content.Headers == null || content.Headers.ContentType == null) {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                MediaTypeHeaderValue contentType = content.Headers.ContentType;
                if (!(contentType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) || contentType.MediaType.Equals("text/json", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                /*
                Uri uri = ParseToQueryUri(Request.RequestUri);
                Signal signal = (Signal)(await MyCoreApi.FindNodeByUriAsync(uri)).FirstOrDefault();//根据uri查找entity
                var conent = Request.Content.ReadAsStringAsync().Result;
                Regex reg = new Regex(@"^(\[)|(\])$");
                conent = reg.Replace(conent, "");
                switch (signal.SampleType) {
                    case "int":
                        int[] ints = Array.ConvertAll<string, int>(conent.Replace(" ", "").Split(new char[] { '，', ',' }), s => int.Parse(s));
                        await((ITypedSignal)signal).PutDataAsync("", ints);
                        break;
                    case "double":
                        double[] doubles = Array.ConvertAll<string, double>(conent.Replace(" ", "").Split(new char[] { '，', ',' }), s => double.Parse(s));
                        await((ITypedSignal)signal).PutDataAsync("", doubles);
                        break;
                    default:
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("The SampleType is not supported!") };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Write data successfully!") };
                */
                SummaryStream summaryStream = new SummaryStream();
                return await Request.Content.CopyToAsync(summaryStream).ContinueWith((readTask) => {
                    summaryStream.Close();

                    // Check whether we completed successfully and generate response
                    if (readTask.IsCompleted) {

                        return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Write data successfully!") };
                    } else {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }
                });
            } catch (Exception e) {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message) };
            }

        }
    }
}