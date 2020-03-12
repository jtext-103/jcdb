using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicPlugins.TypedSignal;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Api;

namespace BasicPlugins.TypedSignal
{
    //  public class FixedIntervalWaveSignal<T>:Signal,ITypedSignal
    public class FixedIntervalWaveSignal : Signal, ITypedSignal
    {
        /// <summary>
        /// 缓存相应的数据类插件实例
        /// </summary>
        private IDataTypePlugin myPlugin;

        /// <summary>
        /// 返回相应的数据类插件实例，如为空则通过核心API获取最佳处理插件并缓存
        /// </summary>
        public IDataTypePlugin MyPlugin
        {
            get
            {
                if (myPlugin == null)
                {
                    //务必保证使用前，已经创建了CoreApi实例，并对CoreService进行了初始化，添加了相关的PlugIn
                    var myCoreApi = CoreApi.GetInstance();
                    myPlugin = myCoreApi.GetBestDataTypePlugin(this.DataType);
                }
                return myPlugin;
            }
        }

        /// <summary>
        /// 起始时间
        /// </summary>
        public double StartTime { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public double EndTime { get; set; }
        /// <summary>
        /// 采样间隔
        /// </summary>
        public double SampleInterval { get; set; }
        /// <summary>
        /// 时间单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 构造函数，对相关属性进行初始化
        /// review 应该把init string放歌解释和凡例在这里
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="initString">if dataType="Expression",initString="/exp/sig",
        /// dataType="FixedWave",initString="StartTime=-2&SampleInterval=0.5"</param>
        public FixedIntervalWaveSignal(string name, string dataType, string initString)
            : base(name)
        {
            //StartTime=0&EndTime=0&SampleInterval=1&Unit=s
            double startTime = 0;
            double endTime = 0;
            double sampleInterval = 0;
            string unit = "s";
            var sections = initString.Split(new char[] { '&' });

            foreach (var sec in sections)
            {
                if (sec.Contains("StartTime"))
                {
                    startTime = double.Parse(sec.Substring(10));
                }
                if (sec.Contains("EndTime"))
                {
                    endTime = double.Parse(sec.Substring(8));
                }
                if (sec.Contains("SampleInterval"))
                {
                    sampleInterval = double.Parse(sec.Substring(15));
                }
                if (sec.Contains("Unit"))
                {
                    unit = sec.Substring(5);
                }
            }
            this.DataType = dataType;
            this.StartTime = startTime;
            this.EndTime = this.StartTime;
            this.SampleInterval = sampleInterval;
            this.Unit = unit;
            if (dataType.Equals("Expression"))
            {
                this.EndTime = endTime;
            }
            else if (sampleInterval <= 0)
            {
                throw new Exception(ErrorMessages.NotValidInitStringError);
            }
        }

        /// <summary>
        /// 异步写入某段数据
        /// </summary>
        /// <param name="fragment"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task PutDataAsync(string fragment, object data)
        {
            await MyPlugin.PutDataAsync(this, fragment, data);
        }
        /// <summary>
        /// 异步获取某段数据的游标
        /// </summary>
        /// <param name="fragment"></param>
        /// <returns></returns>
        public async Task<ICursor> GetCursorAsync(string fragment)
        {
            return await MyPlugin.GetCursorAsync(this, fragment);
        }
        /// <summary>
        /// 异步获取某段数据
        /// </summary>
        /// <param name="fragment"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public async Task<object> GetDataAsync(string fragment, string format)
        {
            return await MyPlugin.GetDataAsync(this, fragment, format);
        }

        public IEnumerable<object> IterateData(string fragment)
        {
            ICursor cursor = MyPlugin.GetCursorAsync(this, fragment).Result;

            //outputStream.SetLength(cursor.LeftPoint);
            while (cursor.LeftPoint > 0)
            {
                var length = cursor.LeftPoint > 1000 ? 1000 : cursor.LeftPoint;//每次从数据库读1k点
                foreach (var data in cursor.IterateCursor(length))
                {
                    yield return data;
                }
            }
        }

        public async Task DisposeAsync()
        {
            await MyPlugin.DisposeAsync(this.Id);
        }
    }
}
