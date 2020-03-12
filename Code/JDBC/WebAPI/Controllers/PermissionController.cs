using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json.Linq;

namespace WebAPI.Controllers {
    /// <summary>
    /// 权限管理与验证
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    public class PermissionController : BaseController {
        /// <summary>
        /// 修改权限
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("permission/modify")]
        public async Task<HttpResponseMessage> ModifyPermission([FromBody]PermissionModel model) {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            try
            {
                Guid NodeId = Guid.Empty;
                var currentEntity = await MyCoreApi.FindNodeByIdAsync(model.nodeid);
                if(currentEntity != null)
                {
                    NodeId = currentEntity.Id;
                }
                if (!await MyCoreApi.Authorization(NodeId, user, "1"))
                {
                    throw new Exception("Not authorization!");
                }
                currentEntity.SetUser(model.user);
                currentEntity.QueryToParentPermission = model.inherit;
                currentEntity.OthersPermission = model.others;
                currentEntity.GroupPermission.Clear();
                foreach (var item in model.groups)
                {
                    var index = item.IndexOf(":::");
                    if (index < 0)
                    { return new HttpResponseMessage(HttpStatusCode.Forbidden); }
                    var key = item.Substring(0, index);
                    var value = item.Substring(index + 3);
                    if (key.Equals("") || value.Equals(""))
                    { return new HttpResponseMessage(HttpStatusCode.Forbidden); }
                    currentEntity.GroupPermission.Add(key,value);
                }
                await MyCoreApi.CoreService.SaveAsync(currentEntity);
                return new HttpResponseMessage(HttpStatusCode.OK);
            } catch (Exception e)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message) };
            }
        }

        /// <summary>
        /// 授权
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("permission/{*value}")]
        public async Task<HttpResponseMessage> Authorization() {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            try {
                JObject queryJson = null;
                if (Request.RequestUri.TryReadQueryAsJson(out queryJson)) {
                    KeyValuePair<string, string> permisssion = new KeyValuePair<string, string>();
                    if (queryJson["user"] != null) {
                        permisssion = new KeyValuePair<string, string>("user", queryJson["user"].ToString());
                    }
                    else if(queryJson["group"] != null) {
                        permisssion = new KeyValuePair<string, string>("group", queryJson["group"].ToString());
                    }
                    else if (queryJson["others"] != null) {
                        permisssion = new KeyValuePair<string, string>("others", queryJson["others"].ToString());
                    }
                    await MyCoreApi.AssignPermission(new Guid(queryJson["id"].ToString()), user, queryJson["new"].ToString(), permisssion);
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("参数错误") };
            } catch (Exception e) {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message) };
            }
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("permission/{*value}")]
        public async Task<HttpResponseMessage> Accreditation() {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            try {
                JObject queryJson = null;
                if (Request.RequestUri.TryReadQueryAsJson(out queryJson)) {
                    await MyCoreApi.Authorization(new Guid(queryJson["id"].ToString()), user, queryJson["operation"].ToString());
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("参数错误") };
            } catch (Exception e) {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message) };
            }
        }
    }

    /// <summary>
    /// Permission表单模型
    /// </summary>
    public class PermissionModel
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public Guid nodeid { get; set; }
        /// <summary>
        /// 节点所有者
        /// </summary>
        public string user { get; set; }
        /// <summary>
        /// 权限白名单
        /// </summary>
        public string[] groups { get; set; }
        /// <summary>
        /// 其他用户权限
        /// </summary>
        public string others { get; set; }
        /// <summary>
        /// 是否继承父级权限
        /// </summary>
        public bool inherit { get; set; }
    }
}