using AutoMapper;
using Jtext103.JDBC.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using WebAPI.Models;
using BasicPlugins.TypedSignal;
using System.Web.Http;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 统一返回结果
    /// </summary>
    public class ResponseEntityMessage
    {
        /// <summary>
        /// 首次处理失败的信息
        /// </summary>
        public object Fail { get; set; }
        /// <summary>
        /// 处理成功的信息
        /// </summary>
        public List<NodeDto> Success { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public ResponseEntityMessage() 
        {
            Success = new List<NodeDto>();
        }
    }
    /// <summary>
    /// 处理Entity相关请求
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = false)]
    public class EntityController : BaseController
    {
        /// <summary>
        /// 获取节点的子节点数目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("entity/childrencount/{id}")]
        public async Task<HttpResponseMessage> ChildrenCount(Guid id)
        {
            var result = new {
                ChildrenCount = (await MyCoreApi.FindAllChildrenIdsByParentIdAsync(id)).Count()
            };
            return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(result), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        /// <summary>
        /// #根据要求查询Entity#
        /// 查询根节点：/entity/path/?_name=*
        /// 查询所有节点：/entity/path/?_name=*&amp;_recursive=true
        /// 按路径查询某一结点：/entity/path/exp1/exp12
        /// 查询某一结点下的子节点：/entity/id/************?_name=sig121&amp;_recursive=true
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("entity/{*value}")]
        public async Task<HttpResponseMessage> Query(string value)
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            try
            {
                Uri uri = ParseToQueryUri(Request.RequestUri);
                var result = new List<NodeDto>();//根据uri查找entity
                foreach (var node in await MyCoreApi.FindNodeByUriAsync(uri))
                {
                    if (node.Name.Equals("expression")) {
                        break;
                    }else if(node.EntityType == JDBCEntityType.Experiment)
                    {
                        var item = Mapper.Map<ExperimentDto>(node);
                        //item.ChildrenCount = (await MyCoreApi.FindAllChildrenIdsByParentIdAsync(item.Id)).Count();
                        result.Add(item);
                    }else if(node.EntityType == JDBCEntityType.Signal)
                    {
                        var item = Mapper.Map<FixedIntervalWaveSignalDto>(node);
                        result.Add(item);
                    }
                }
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(result), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch(Exception e)
            {
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
            }
        }

        /// <summary>
        /// #根据要求新建Entity#
        /// 添加根节点：/entity/path/【name=exp1&amp;type=experiment】
        /// 添加子节点：/entity/path/exp1【name=exp12&amp;type=experiment】
        /// 递归添加信号节点：/entity/id/?_name=*&amp;_recursive=true【name=sig121&amp;type=signal】
        /// </summary>
        [HttpPost]
        [Route("entity/{*value}")]
        public async Task<HttpResponseMessage> Insert()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                Uri uri = ParseToQueryUri(Request.RequestUri);
                List<JDBCEntity> parents = (await MyCoreApi.FindNodeByUriAsync(uri)).ToList();//根据uri查找父节点
                if (uri.PathAndQuery.Equals("/path/") || uri.PathAndQuery.Equals("/id/"))//添加根节点的情形
                {
                    parents.Add(new Experiment { Id = Guid.Empty });
                }
                else if (parents.Count() == 0)//根节点不存在
                {
                    throw new Exception("Parent node not exist!");
                }
                NameValueCollection form = Request.Content.ReadAsFormDataAsync().Result;
                JDBCEntity newNode = null;
                var type = GetValueFromForm(form, "type");
                var name = GetValueFromForm(form, "name");
                var extras = GetValuesFromForm(form, "extra[]", true);
                if (string.IsNullOrWhiteSpace(name)) {
                    throw new Exception("Entity name can not be empty!");
                }
                switch (type.ToLower())
                {
                    case "experiment":
                        {
                            newNode = new Experiment(name);
                        }
                        break;
                    case "signal":
                        {
                            var init = GetValueFromForm(form, "init");
                            var datatype = GetValueFromForm(form, "datatype");
                            newNode = MyCoreApi.CreateSignal(datatype, name, init.Replace(";", "&").Replace(",", "&").Trim("'\"()[]{}".ToCharArray()));
                        }
                        break;
                    default:
                        throw new Exception("Entity type is not supported!");
                }
                if(extras != null)//添加ExtraInformation
                {
                    foreach (var extra in extras)
                    {
                        var index = extra.IndexOf(":::");
                        if (index < 0) { return new HttpResponseMessage(HttpStatusCode.Forbidden); }
                        var key = extra.Substring(0, index);
                        var value = extra.Substring(index + 3);
                        if (key.Equals("") || value.Equals("") || newNode.IfExtraInformationContainsKey(key)) { return new HttpResponseMessage(HttpStatusCode.Forbidden); }
                        newNode.AddExtraInformation(key, value);
                    }
                }
                
                if (newNode == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent("Failed to create new node!") };
                }
                foreach (var parent in parents)
                {
                    if (!await MyCoreApi.Authorization(parent.Id, user, "1")) {
                        throw new Exception("Not authorization!");
                    }
                    currentEntity = parent;
                    newNode.Id = Guid.NewGuid();
                    newNode.SetUser(user.UserName);
                    var node = await MyCoreApi.AddOneToExperimentAsync(parent.Id, newNode);//添加signal
                    successEntity.Add(Mapper.Map<NodeDto>(node));//保存处理结果
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if(currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.Message) };
                }
                var response = new ResponseEntityMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// 重命名节点：/entity/rename【node='************';name=diag】
        /// </summary>
        [HttpPut]
        [Route("entity/rename")]
        public async Task<HttpResponseMessage> Rename()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                var dict = ParseToQueryDictionary(Request.RequestUri);
                NameValueCollection form = Request.Content.ReadAsFormDataAsync().Result;
                var newname = GetValueFromForm(form, "name");
                currentEntity = await MyCoreApi.FindNodeByIdAsync(new Guid(GetValueFromForm(form, "node")));
                if (!await MyCoreApi.Authorization(currentEntity.Id, user, "1")) {
                    throw new Exception("Not authorization!");
                }
                var newNode = await MyCoreApi.ReNameNodeAsync(currentEntity.Id, newname);
                successEntity.Add(Mapper.Map<NodeDto>(newNode));//保存处理结果
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseEntityMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// 复制节点：/entity/duplicate?recursive=false【newname=diag;from='/path/exp1/exp2';to='/path/exp2?name=exp21'】
        /// </summary>
        [HttpPut]
        [Route("entity/duplicate")]
        public async Task<HttpResponseMessage> Duplicate()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                var dict = ParseToQueryDictionary(Request.RequestUri);
                NameValueCollection form = Request.Content.ReadAsFormDataAsync().Result;
                var recursive = false;
                if (dict.ContainsKey("recursive") && dict["recursive"] == "true")
                {
                    recursive = true;
                }
                var copydata = false;
                if (dict.ContainsKey("copydata") && dict["copydata"] == "true")
                {
                    copydata = true;
                }
                var fromNodes = await MyCoreApi.FindNodeByUriAsync(new Uri("jdbc://" + GetValueFromForm(form, "from").Replace(";", "&").Replace(",", "&").Trim("'\"()[]{}".ToCharArray())));//获取源entity
                var toNodes = await MyCoreApi.FindNodeByUriAsync(new Uri("jdbc://" + GetValueFromForm(form, "to").Replace(";", "&").Replace(",", "&").Trim("'\"()[]{}".ToCharArray())));//获取目的entity
                var newname = GetValueFromForm(form,"newname");
                foreach (var fromNode in fromNodes)
                {
                    foreach (var toNode in toNodes)
                    {
                        currentEntity = toNode;
                        if (!await MyCoreApi.Authorization(currentEntity.Id, user, "1"))
                        {
                            throw new Exception("Not authorization!");
                        }
                        var newNode = await MyCoreApi.DuplicateAsync(fromNode.Id, toNode.Id, newname, recursive, copydata);//复制entity到新的父节点下
                        successEntity.Add(Mapper.Map<NodeDto>(newNode));//保存处理结果
                    }
                }
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseEntityMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// 移动节点：/entity/move?copydata=true【newname=diag;from='/id/************';to='/path/exp2?name=sig21'】
        /// </summary>
        [HttpPut]
        [Route("entity/move")]
        public async Task<HttpResponseMessage> Move()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                var dict = ParseToQueryDictionary(Request.RequestUri);
                NameValueCollection form = Request.Content.ReadAsFormDataAsync().Result;
                var fromNodes = await MyCoreApi.FindNodeByUriAsync(new Uri("jdbc://" + GetValueFromForm(form,"from").Replace(";", "&").Replace(",", "&").Trim("'\"()[]{}".ToCharArray())));//获取源entity
                var toNodes = await MyCoreApi.FindNodeByUriAsync(new Uri("jdbc://" + GetValueFromForm(form,"to").Replace(";", "&").Replace(",", "&").Trim("'\"()[]{}".ToCharArray())));//获取目的entity
                foreach (var fromNode in fromNodes)
                {
                    foreach (var toNode in toNodes)
                    {
                        currentEntity = toNode;
                        if (!await MyCoreApi.Authorization(currentEntity.Id, user, "1"))
                        {
                            throw new Exception("Not authorization!");
                        }
                        var newNode = await MyCoreApi.MoveJdbcEntityAsync(toNode.Id, fromNode.Id);//迁移entity到新的父节点下
                        successEntity.Add(Mapper.Map<NodeDto>(toNode));//保存处理结果
                    }
                }
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseEntityMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// 修改元数据：/entity/extra【node='************';extra=[]】
        /// </summary>
        [HttpPut]
        [Route("entity/extra")]
        public async Task<HttpResponseMessage> Extra()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                var dict = ParseToQueryDictionary(Request.RequestUri);
                NameValueCollection form = Request.Content.ReadAsFormDataAsync().Result;
                var id = new Guid(GetValueFromForm(form, "node"));
                currentEntity = await MyCoreApi.FindNodeByIdAsync(id);//获取源entity
                if (!await MyCoreApi.Authorization(currentEntity.Id, user, "1"))
                {
                    throw new Exception("Not authorization!");
                }
                var extras = GetValuesFromForm(form, "extra[]", true);
                if (extras != null)
                {
                    currentEntity.ExtraInformation.Clear();
                    foreach (var extra in extras)
                    {
                        var index = extra.IndexOf(":::");
                        if (index < 0) { return new HttpResponseMessage(HttpStatusCode.Forbidden); }
                        var key = extra.Substring(0, index);
                        var value = extra.Substring(index + 3);
                        if (key.Equals("") || value.Equals("")) { return new HttpResponseMessage(HttpStatusCode.Forbidden); }
                        currentEntity.AddExtraInformation(key, value);
                    }
                    await MyCoreApi.CoreService.SaveAsync(currentEntity);
                }
                successEntity.Add(Mapper.Map<NodeDto>(currentEntity));
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseEntityMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// 重置数据库【慎用】：/entity/restore
        /// </summary>
        [HttpPut]
        [Route("entity/restore")]
        public async Task<HttpResponseMessage> Restore()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                if (!await MyCoreApi.Authorization(Guid.Empty, user, "1"))
                {
                    throw new Exception("Not authorization!");
                }
                await MyCoreApi.clearDb(true);
                MyCoreApi.CoreService.StorageEngine.Init(BusinessConfig.cassandraInit);
                JDBCEntity root = new Experiment("jtext");
                root.SetUser("root");
                await MyCoreApi.AddOneToExperimentAsync(Guid.Empty, root);
                successEntity.Add(Mapper.Map<NodeDto>(root));
                for (var shot = 1; shot < 3; shot++)
                {
                    JDBCEntity exp = new Experiment(shot.ToString());
                    await MyCoreApi.AddOneToExperimentAsync(root.Id, exp);
                    successEntity.Add(Mapper.Map<NodeDto>(exp));
                    //normal data
                    var waveSignal1 = (FixedIntervalWaveSignal)MyCoreApi.CreateSignal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
                    await MyCoreApi.AddOneToExperimentAsync(exp.Id, waveSignal1);
                    await waveSignal1.PutDataAsync("", GenerateIntArray());
                    await waveSignal1.DisposeAsync();
                    successEntity.Add(Mapper.Map<NodeDto>(waveSignal1));
                    //big data
                    var waveSignal2 = (FixedIntervalWaveSignal)MyCoreApi.CreateSignal("FixedWave-double", "ws2", @"StartTime=-2&SampleInterval=0.001");
                    waveSignal2.NumberOfSamples = 2000;
                    await MyCoreApi.AddOneToExperimentAsync(exp.Id, waveSignal2);
                    for (int i = 0; i < 20; i++)
                    {
                        var array = GenerateDoubleArray(2000);
                        await waveSignal2.PutDataAsync("", array);
                    }
                    await waveSignal2.DisposeAsync();
                    successEntity.Add(Mapper.Map<NodeDto>(waveSignal2));
                }
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseEntityMessage
                {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        /// <summary>
        /// #根据要求删除Entity#
        /// 递归删除某一个根节点：/entity/path/exp2?_recursive=true
        /// 删除某一个子节点：/entity/path/exp1?_name=exp12
        /// 批量删除查询到的所有节点：/entity/id/************?recursive=true
        /// </summary>
        [HttpDelete]
        [Route("entity/{*value}")]
        public async Task<HttpResponseMessage> Delete()
        {
            var user = GetSessionUser(Request.Headers.GetCookies().FirstOrDefault());
          
            List<NodeDto> successEntity = new List<NodeDto>();//成功处理的Entity信息
            JDBCEntity currentEntity = null;//当前处理的Entity信息
            try
            {
                if(Request.RequestUri.PathAndQuery.ToLower().Equals("/entity/all"))//清空数据库
                {
                    await MyCoreApi.clearDb();
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Clear database successfully!") };
                }
                Uri uri = ParseToQueryUri(Request.RequestUri);
                var nodes = await MyCoreApi.FindNodeByUriAsync(uri);//获取被删除entity
                var queryDict = ParseToQueryDictionary(Request.RequestUri);
                if ((!queryDict.ContainsKey("recursive") || queryDict["recursive"] == "false") && nodes.Count() > 1)//递归删除查询到的所有entity
                {
                    throw new Exception("Please use [recursive=true] to delete queried entities!");
                }
                
                if (nodes.Count() == 0)//entity不存在
                {
                    throw new Exception("No entity is found!");
                }
                var deleteChildren = false;
                if(queryDict.ContainsKey("_recursive") && queryDict["_recursive"] == "true") {
                    deleteChildren = true;
                }

                foreach (var entity in nodes)
                {
                    currentEntity = entity;
                    if (!await MyCoreApi.Authorization(currentEntity.Id, user, "1")) {
                        throw new Exception("Not authorization!");
                    }
                    await MyCoreApi.DeletAsync(entity.Id, deleteChildren);
                    successEntity.Add(Mapper.Map<NodeDto>(currentEntity));
                }
                return new HttpResponseMessage { Content = new StringContent(SerializeObjectToString(successEntity), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception e)
            {
                if (currentEntity == null)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(e.InnerException != null ? e.InnerException.Message : e.Message) };
                }
                var response = new ResponseEntityMessage {
                    Fail = new
                    {
                        Description = e.InnerException != null ? e.InnerException.Message : e.Message,
                        Id = currentEntity.Id,
                        Path = currentEntity.Path
                    },
                    Success = successEntity
                };
                return new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden, Content = new StringContent(SerializeObjectToString(response), System.Text.Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }
    }
}