using Jtext103.JDBC.Core.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using WebAPI.Models;
using Jtext103.Identity.Models;
using System.Collections.Specialized;
using System.Linq;

namespace WebAPI.Controllers
{
    /// <summary>
    /// ApiController父类
    /// </summary>
    public class BaseController : ApiController
    {
        /// <summary>
        /// CoreApi实例
        /// </summary>
        public static CoreApi MyCoreApi
        {
            get { return BusinessConfig.MyCoreApi; }
        }
        /// <summary>
        /// session
        /// </summary>
        public static string COOKIENAME = @".Jtext103.AppCookie";
        /// <summary>
        /// Cros请求
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public string Options()
        {
            return null; // HTTP 200 response with empty body
        }
        /// <summary>
        /// 从NameValueCollection中取值
        /// </summary>
        /// <param name="form"></param>
        /// <param name="key"></param>
        /// <param name="allowEmpty"></param>
        /// <returns></returns>
        public string GetValueFromForm(NameValueCollection form, string key, bool allowEmpty = false)
        {
            if (!allowEmpty && !form.AllKeys.Contains(key))
            {
                throw new Exception("["+ key + "] value/key must be included in form!");
            }
            return form.GetValues(key).FirstOrDefault();
        }
        /// <summary>
        /// 从NameValueCollection中取值
        /// </summary>
        /// <param name="form"></param>
        /// <param name="key"></param>
        /// <param name="allowEmpty"></param>
        /// <returns></returns>
        public string[] GetValuesFromForm(NameValueCollection form, string key, bool allowEmpty = false) {
            if (!allowEmpty && !form.AllKeys.Contains(key))
            {
                throw new Exception("[" + key + "] value/key must be included in form!");
            }
            return form.GetValues(key);
        }
        /// <summary>
        /// 将url查询转换为字典
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseToQueryDictionary(Uri url)
        {
            Dictionary<string, string> splitDic = new Dictionary<string, string>();
            if (!url.Query.StartsWith("?"))
            {
                return splitDic;
            }
            string[] splitArray = url.Query.Substring(1).Trim().Split('&');
            foreach (var item in splitArray)
            {
                int startIndex = item.IndexOf("=");
                string key = item.Substring(0, startIndex).ToLower();
                string value = item.Substring(startIndex + 1);
                splitDic.Add(key, value);
            }
            return splitDic;
        }
        /// <summary>
        /// Uri解析方法
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Uri ParseToQueryUri(Uri url)
        {
            var slash = url.PathAndQuery.IndexOf("/", 1);
            var uriString = url.PathAndQuery.Substring(slash).Trim();
            string[] queryArray = { };
            if (uriString.Contains("?"))
            {
                queryArray = uriString.Substring(uriString.IndexOf("?") + 1).Trim().Split('&');
                uriString = uriString.Substring(0, uriString.IndexOf("?"));
            }
            var queryString = "?";
            var fragmentString = "#";
            foreach (var s in queryArray)
            {
                if(s.StartsWith("_="))
                {
                    continue;
                }
                else if (s.StartsWith("__"))
                {
                    fragmentString = fragmentString + (fragmentString.Equals("#") ? "" : "&") + s.Substring(2);
                }
                else if (s.StartsWith("_"))
                {
                    queryString = queryString + (queryString.Equals("?") ? "" : "&") + s.Substring(1);
                }
            }

            Uri uri = new Uri("jdbc://" + uriString + (queryString.Equals("?") ? "" : queryString) + (fragmentString.Equals("#") ? "" : fragmentString), UriKind.Absolute);
            return uri;
        }
        /// <summary>
        /// 从url里剥离出查询字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Uri ParseToPureUrl(Uri url)
        {
            var slash = url.PathAndQuery.IndexOf("/", 1);
            var urlString = url.PathAndQuery.Substring(0, slash).Trim();
            var uriString = url.PathAndQuery.Substring(slash).Trim();
            string[] queryArray = { };
            if (uriString.Contains("?"))
            {
                queryArray = uriString.Substring(uriString.IndexOf("?") + 1).Trim().Split('&');
                uriString = uriString.Substring(0, uriString.IndexOf("?"));
            }
            var queryString = "?";
            foreach (var s in queryArray)
            {
                if (!s.StartsWith("__") && !s.StartsWith("_"))
                {
                    queryString = queryString + (queryString.Equals("?") ? "" : "&") + s;
                }
            }

            Uri uri = new Uri(url.OriginalString.Substring(0, url.OriginalString.IndexOf("/", 7)) + urlString + (queryString.Equals("?") ? "" : queryString) + url.Fragment, UriKind.Absolute);
            return uri;
        }
        /// <summary>
        /// 将对象转换为json字符串
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string SerializeObjectToString(Object instance)
        {
            StringWriter tw = new StringWriter();
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(tw, instance, instance.GetType());
            return tw.ToString();
        }
        /// <summary>
        /// 返回存放序列化后的数据缓冲区
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Serialize(object data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }
        /// <summary>
        /// 生成随机int数组
        /// </summary>
        /// <returns></returns>
        public static int[] GenerateIntArray(int count = 100) 
        {
            List<int> result = new List<int>();
            var rand = new Random();
            for (int i = 0; i < count; i++) {
                var item = (int)(Math.Sinh(rand.NextDouble()) + Math.Sin(i));
                result.Add(item);
            }
            return result.ToArray();
        }
        /// <summary>
        /// 生成随机double数组
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static double[] GenerateDoubleArray(int count = 100) {
            List<double> result = new List<double>();
            var rand = new Random();
            for (int i = 0; i < count; i++) {
                var item = rand.NextDouble()*10 + Math.Sinh(rand.NextDouble()) + Math.Sin(i);
                result.Add(item);
            }
            return result.ToArray();
        }
        /// <summary>
        /// 从cookie中获取UserId
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static LoginClaim GetSessionUser(CookieHeaderValue cookie) {
            LoginClaim user = new LoginClaim {
                UserName = "",//匿名用户
                Roles = new HashSet<string>()
            };
            if (cookie != null) {
                var session = cookie[COOKIENAME];
                var userCookie = cookie["user"];
                if (session != null && !session.Value.Equals("")) {
                    byte[] bytes = Convert.FromBase64String(session.Value);
                    using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length)) {
                        ms.Write(bytes, 0, bytes.Length);
                        ms.Position = 0;
                        var obj = new BinaryFormatter().Deserialize(ms);
                        user = (LoginClaim)obj;
                    }
                }else if (userCookie != null && userCookie.Value.Equals("root")) {
                    user.Roles.Add("Root");
                    user.UserName = "root";
                }
            }
            return user;
        }
    }
}