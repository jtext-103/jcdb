using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using BasicPlugins.TypedSignal;
using System.Diagnostics;

namespace BasicPlugins
{
    public class FixedWaveDataTypePlugin : IDataTypePlugin
    {
        public string Name
        {
            get { return "FixedIntervalTimebaseWaveDataTypePlugin"; }
        }
        private CoreService myCoreService { get; set; }
        private IMatrixStorageEngineInterface myStorageEngine { get; set; }

        public FixedWaveDataTypePlugin(CoreService coreService)
        {
            myCoreService = coreService;
            myStorageEngine = coreService.StorageEngine as IMatrixStorageEngineInterface;
            if (myStorageEngine == null)
            {
                throw new Exception(ErrorMessages.NotValidStorageEngine);
            }
            DataProcesserDictionary = new ConcurrentDictionary<Guid, IDataProcesser>();
        }

        private ConcurrentDictionary<Guid, IDataProcesser>  DataProcesserDictionary { get; set; }

        /// <summary>
        /// if start with FixedWave then 100pt else 0pt
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public int ScoreDatatype(string dataType)
        {
            return dataType.IndexOf("FixedWave") == 0 ? 100 : 0;
        }

        /// <summary>
        /// review todo  建议在写一个新的CreateSignal方法，可以动态的从反射获取的signal type中动态实例化，用ActivatorActivator.CreateInstence方法，可以bimianhardcode
        /// </summary>
        /// <param name="datatype"></param>
        /// <param name="name"></param>
        /// <param name="initString">StartTime=0&SampleInterval=1&Unit=s</param>
        /// <returns></returns>
        public Signal CreateSignal(string datatype, string name, string initString)
        {
            Signal signal = null;
            switch (datatype)
            {
                case "FixedWave-int":
                    signal = new FixedIntervalWaveSignal(name, "FixedWave-int", initString);
                    break;
                case "FixedWave-double":
                    signal = new FixedIntervalWaveSignal(name, "FixedWave-double", initString);
                    break;
                default:
                    throw new Exception(ErrorMessages.NotValidSignalError);
            }
            return signal;
        }

        public async Task<object> GetDataAsync(Signal signal, string fragment, string format)
        {
            // Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")+ "   GetDataProcesser");
            IDataProcesser MyDataProcesser = GetDataProcesser(signal);
            return await MyDataProcesser.GetDataAsync(signal, fragment, format);
        }

        public async Task PutDataAsync(Signal signal, string fragment, object data)
        {
            //    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   GetDataProcesser");
            IDataProcesser MyDataProcesser = GetDataProcesser(signal);
            await MyDataProcesser.PutDataAsync(signal, fragment, data);
        }

        public async Task<ICursor> GetCursorAsync(Signal signal, string dataFragment)
        {
            IDataProcesser MyDataProcesser = GetDataProcesser(signal);
            return await MyDataProcesser.GetCursorAsync(signal, dataFragment);
        }
      
        private IDataProcesser GetDataProcesser(Signal signal)
        {
            //如果已经创建了DataProcesser，则直接返回，没有则创建相应DataProcesser
            if (DataProcesserDictionary.ContainsKey(signal.Id))
            {
                return DataProcesserDictionary[signal.Id];
            }
            else
            {
                IDataProcesser MyDataProcesser;
                // signal.Name可不要这个参数，稳定后删除
                switch (signal.DataType)
                {
                    case "FixedWave-int":
                        MyDataProcesser = new WaveDataProcesser<int>(myCoreService, signal.DataType,signal.Name);
                        DataProcesserDictionary.TryAdd(signal.Id, MyDataProcesser);
                        return MyDataProcesser;
                    case "FixedWave-double":
                        MyDataProcesser = new WaveDataProcesser<double>(myCoreService, signal.DataType,signal.Name);
                        DataProcesserDictionary.TryAdd(signal.Id, MyDataProcesser);
                        return MyDataProcesser;
                    default:
                        throw new Exception(ErrorMessages.NotValidSignalError);
                }
            }
        }
        
        public async Task DisposeAsync(Guid id)
        {
            IDataProcesser MyDataProcesser = DataProcesserDictionary[id];
            var bo=DataProcesserDictionary.TryRemove(id,out MyDataProcesser);
        //    Debug.WriteLine(id+":"+bo);
            await MyDataProcesser.DisposeAsync();
          //  Debug.WriteLine(MyDataProcesser.Name + " ended  ： "+  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
        }

    }
}

