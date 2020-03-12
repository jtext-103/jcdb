using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;

namespace PathQueryPlugInTestDll
{
    /// <summary>
    /// for basic path and child parent navigation.
    /// pattern: no query or only one query key "child"
    /// return the entities of the specified path
    /// </summary>
    public class PathQueryPlugIn : IQueryPlugIn
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name
        {
            get { return "PathQueryPlugIn"; }
        }
        /// <summary>
        /// CoreService实例
        /// </summary>
        private CoreService myCoreService { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="coreService"></param>
        public PathQueryPlugIn(CoreService coreService)
        {
            myCoreService = coreService;
        }
        /// <summary>
        /// 根据查询格式计算插件得分
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int ScoreQuery(string query)
        {
            return query.StartsWith("/path/") ? 100 : 0;
        }
        /// <summary>
        /// 支持以下查询：
        /// /path/?name=* 查询所有根节点
        /// /path/root/exp1/ip  查询对应路径的节点
        /// /path/root/exp1?name=*   查询对应路径下的所有直接子节点
        /// /path/root/exp1?name=*&recursive=true   查询对应路径下的所有子节点
        /// /path/root/exp1?name=sig1   查询对应路径下的所有直接子节点
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JDBCEntity>> FindJdbcEntityAsync(string query)
        {
            // ?后为子节点查询字段
            int index = query.IndexOf("?");
            JDBCEntity parent = null;
            List<JDBCEntity> result = new List<JDBCEntity>();
            if (index > 0) //存在?(子节点查询)
            {
                var path = query.Substring(5, index - 5);
                if (!path.Equals("/"))
                {
                    parent = await myCoreService.GetOneByPathAsync(path);
                    if (parent == null)
                    {
                        throw new Exception(ErrorMessages.ExperimentOrSignalNotFoundError);
                    }
                }

                // 解析?后的子节点查询条件
                string[] splitArray = query.Substring(index + 1).Split('&');
                Dictionary<string, string> splitDic = new Dictionary<string, string>();
                foreach (var item in splitArray)
                {
                    int startIndex = item.IndexOf("=");
                    string key = item.Substring(0, startIndex).Trim();
                    string value = item.Substring(startIndex + 1).Trim();
                    splitDic.Add(key, value);
                }

                // 执行子节点查询条件
                if (splitDic.ContainsKey("name"))
                {
                    if (splitDic.ContainsKey("recursive") && splitDic["recursive"].Equals("true"))
                    {
                        return await myCoreService.FindJdbcEntityAsync(parent, splitDic["name"], true);
                    }
                    else
                    {
                        return await myCoreService.FindJdbcEntityAsync(parent, splitDic["name"], false);
                    }
                }
                else
                {
                    result.Add(parent);
                    return result;
                }
            }
            else if (query.LastOrDefault().Equals('/') && query.Length == 6) //返回空列表
            {
                return result;
            }
            else // 不存在?，仅根据{id}查询节点
            {
                var node = await myCoreService.GetOneByPathAsync(query.Substring(5));
                if (node != null)
                {
                    result.Add(node);
                }
                return result;
            }
        }
    }
}
