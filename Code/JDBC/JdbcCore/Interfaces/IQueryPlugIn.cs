using Jtext103.JDBC.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Services;

namespace Jtext103.JDBC.Core.Interfaces
{
    /// <summary>
    /// 所有查询类插件的统一接口
    /// </summary>
    public interface IQueryPlugIn
    {
        /// <summary>
        /// 插件名称，应保证其唯一性 
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 对查询类型进行评分，上层API根据分值决定使用哪一个查询类插件
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int ScoreQuery(string query);
        /// <summary>
        /// 异步查找符合条件的节点
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<IEnumerable<JDBCEntity>> FindJdbcEntityAsync(string query);
    }
}
