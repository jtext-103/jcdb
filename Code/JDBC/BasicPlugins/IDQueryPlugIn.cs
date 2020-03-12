using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicPlugins
{
    /// <summary>
    /// 根据ID查询entity
    /// </summary>
    public class IDQueryPlugIn : IQueryPlugIn
    {
        public string Name
        {
            get { return "IDQueryPlugin"; }
        }
        private CoreService myCoreService { get; set; }
        public IDQueryPlugIn(CoreService coreService)
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
            return query.StartsWith("/id/") ? 100 : 0;
        }
        /// <summary>
        /// 支持以下查询：
        /// /id/{id}?name=* 查询所有根节点
        /// /id/{id}  查询对应路径的节点
        /// /id/{id}?name=*   查询对应路径下的所有直接子节点
        /// /id/{id}?name=*&recursive=true   查询对应路径下的所有子节点
        /// /id/{id}?name=sig1   查询对应路径下的所有直接子节点
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JDBCEntity>> FindJdbcEntityAsync(string query)
        {
            // ?后为子节点查询字段
            int index = query.IndexOf("?");
            JDBCEntity parent = null;
            List<JDBCEntity> result = new List<JDBCEntity>();

            if(query.Length == 4) // 返回空列表
            {
                return result;
            }

            if (index != 4) // 存在{id}，查找父节点
            {
                try {
                    string strID = index == -1 ? query.Substring(4) : query.Substring(4, index - 4);
                    Guid parentID = Guid.Parse(strID); // 获取{id}
                    parent = await myCoreService.GetOneByIdAsync(parentID);
                } catch (Exception) { // {id}解析错误
                    throw new Exception(ErrorMessages.NotValidIdError);
                }
                if (parent == null) {
                    throw new Exception(ErrorMessages.ExperimentOrSignalNotFoundError);
                }
            }

            if (index > 0) //存在?(子节点查询)
            {
                // 解析?后的子节点查询条件
                string[] splitArray = query.Substring(index+1).Split('&');
                Dictionary<string,string> splitDic = new Dictionary<string,string>();
                foreach (var item in splitArray)
                {
                    int startIndex=item.IndexOf("=");
                    string key = item.Substring(0, startIndex).Trim();
                    string value = item.Substring(startIndex + 1).Trim();
                    splitDic.Add(key,value);
                }

                // 执行子节点查询条件
                if(splitDic.ContainsKey("name"))
                {
                    if(splitDic.ContainsKey("recursive") && splitDic["recursive"].Equals("true"))
                    {
                        return await myCoreService.FindJdbcEntityAsync(parent,splitDic["name"],true);
                    }
                    else
                    {
                        return await myCoreService.FindJdbcEntityAsync(parent,splitDic["name"],false);
                    }
                }
                else
                {
                    result.Add(parent);
                    return result;
                }
            }
            else // 不存在?，仅根据{id}查询节点
            {
                result.Add(parent);
                return result;
            }
        }
    }
}
