using System.Collections.Generic;
using Jtext103.JDBC.Core.Models;
using System.Threading.Tasks;
using System;

namespace Jtext103.JDBC.Core.Interfaces
{
   public interface IDataTypePlugin
    {
        /// <summary>
        /// 插件名称，应保证其唯一性 
        /// </summary>
        string Name { get; }

        //IDataProcesser MyDataProcesser { get; }
        /// <summary>
        /// 对某一数据类型进行评分，上层API根据分值决定使用哪一个数据类插件
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        int ScoreDatatype(string dataType);

        /// <summary>
        /// get information for signal，review 这个是干嘛用的？忘了，好像没用为是阿布直接用signal里面的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <returns></returns>
        // Dictionary<string, object> GetInformation<T>(Signal signal);

        /// <summary>
        /// review todo  建议在写一个新的CreateSignal方法，可以动态的从反射获取的signal type中动态实例化，用ActivatorActivator.CreateInstence方法，可以bimianhardcode
        /// </summary>
        /// <param name="datatype"></param>
        /// <param name="name"></param>
        /// <param name="initString">StartTime=0&SampleInterval=1&Unit=s</param>
        /// <returns></returns>
        Signal CreateSignal(string datatype, string name, string initString);
        Task<object> GetDataAsync(Signal signal, string fragment, string format);
        Task PutDataAsync(Signal signal, string fragment, object data);
        Task<ICursor> GetCursorAsync(Signal signal, string dataFragment);
        Task DisposeAsync(Guid id);
    }
}

