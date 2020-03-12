using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;

namespace Jtext103.JDBC.Core.Interfaces
{
    public interface ITypedSignal
    {
        //     IDataProcesser MyDataProcesser { get; }
         IDataTypePlugin MyPlugin { get; }

        Task<ICursor> GetCursorAsync(string fragment);

        /// <summary>
        /// format is "array"(default) or "complex" 
        /// review 你改了定义，主是不改？看得我一愣一愣的。。。。。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment">fragment: "start=t1&end=t2&decimation=2&count=1000" decimation is optional, if end is no bigger then start then return all</param>
        /// <param name="format">format is "array"(default) or "complex" </param>
        /// <returns></returns>
        Task<object> GetDataAsync(string fragment, string format);


        /// <summary>
        /// the data is a T[], this append data to the signal, the signal should aready contains the metadata
        /// review 你改了定义，主是不改？看得我一愣一愣的。。。。。
        /// </summary>
        /// <typeparam name="T">sample type</typeparam>
        /// <param name="signal">the signal you need to append</param>
        /// <param name="fragment"> not used</param>
        /// <param name="data">T[], the data to append</param>
        /// <returns></returns>
        Task PutDataAsync(string fragment, object data);

        IEnumerable<object> IterateData(string fragment);
    }
}
