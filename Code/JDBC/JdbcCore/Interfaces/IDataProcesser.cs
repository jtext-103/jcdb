using Jtext103.JDBC.Core.Models;
using System;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Interfaces
{
    /// <summary>
    /// 所有数据类插件的统一接口
    /// </summary>
    public interface IDataProcesser
    {
        string Name { get; set; }
        string DataType { get; set; }
        /// <summary>
        /// 按需异步获取信号数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment">指明获取哪一段数据等</param>
        /// <param name="format">指明数据的返回形式</param>
        /// <returns></returns>
        //Task<object> GetDataAsync<T>(Signal signal, string fragment, string format);
        Task<object> GetDataAsync(Signal signal, string fragment, string format);
        /// <summary>
        /// 按需异步获取信号数据游标
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="dataFragment"></param>
        /// <returns></returns>
    //    Task<ICursor> GetCursorAsync<T>(Signal signal, string dataFragment);
        Task<ICursor> GetCursorAsync(Signal signal, string dataFragment);
        /// <summary>
        /// 将数据异步写入信号节点中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        //    Task PutDataAsync<T>(Signal signal, string fragment, object data);
        Task PutDataAsync(Signal signal, string fragment, object data);

      //  void Dispose();
        Task DisposeAsync();
    }
}
