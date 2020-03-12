using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using BasicPlugins.TypedSignal;


namespace FixedWaveDataTypePluginTestDll
{
    /// <summary>
    /// basic fixed interval timebase wave data plugin ,
    /// signal requiement: FixedIntervalWaveSignal or derived from it
    /// fragment: "start=t1&end=t2&decimation=2&count=1000" decimation is optional, if end is no bigger then start then return all
    /// count over decimation, if count is set, decimation will be ignored, you may not get the exact count as you specified
    /// </summary>
    public class FixedWaveDataTypePlugin : IDataTypePlugin
    {
        public string Name
        {
            get { return "FixedIntervalTimebaseWaveDataTypePlugIn"; }
        }
        private CoreService myCoreService { get; set; }
        private IMatrixStorageEngineInterface myStorageEngine { get; set; }

        public IDataProcesser MyDataProcesser
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FixedWaveDataTypePlugin(CoreService coreService)
        {
            myCoreService = coreService;
            myStorageEngine = coreService.StorageEngine as IMatrixStorageEngineInterface;
            if (myStorageEngine == null)
            {
                throw new Exception(ErrorMessages.NotValidStorageEngine);
            }
        }
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
        /// 使用Cursor读取大数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment"></param>
        /// <returns></returns>
        public async Task<ICursor> GetCursorAsync<T>(Signal signal, string fragment)
        {
            //var waveSig = signal as FixedIntervalWaveSignal<T>;
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception(ErrorMessages.NotValidSignalError);
            }
            WaveFragment frag = null;
            try
            {
                frag = WaveFragment.Parse(fragment);
            }
            catch (Exception)
            {
                throw new Exception(ErrorMessages.NotValidSignalFragmentError);
            }
            //end smaller then start then read all
            if (frag.Start >= frag.End)
            {
                frag.Start = waveSig.StartTime;
                frag.End = waveSig.EndTime;
            }
            //calc the decimationfactor is count valid
            if (frag.Count > 0)
            {
                //todo can be optimized a little
                frag.DecimationFactor = (long)Math.Floor(((frag.End - frag.Start) / waveSig.SampleInterval + 1) / frag.Count);
            }
            if (frag.DecimationFactor < 1)
            {
                frag.DecimationFactor = 1;
            }

            var startIndex = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
            var count = (long)Math.Floor((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor) + 1;

            return await myStorageEngine.GetCursorAsync<T>(waveSig.Id, new List<long> { startIndex }, new List<long> { count }, new List<long> { frag.DecimationFactor });
        }
        /// <summary>
        /// get information for signal，review 这个是干嘛用的？忘了，好像没用为是阿布直接用signal里面的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetInformation<T>(Signal signal)
        {
            //var waveSig = signal as FixedIntervalWaveSignal<T>;
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception(ErrorMessages.NotValidSignalError);
            }

            var count = (long)Math.Floor((waveSig.EndTime - waveSig.StartTime) / waveSig.SampleInterval) + 1;
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("Start", waveSig.StartTime);
            result.Add("End", waveSig.EndTime);
            result.Add("Count", count);
            result.Add("OrignalSampleInterval", waveSig.SampleInterval);
            result.Add("Unit", waveSig.Unit);
            return result;
        }
        /// <summary>
        /// format is "array"(default) or "complex" 
        /// review 这个下面很多代码和get curser重复了，可以修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment">fragment: "start=t1&end=t2&decimation=2&count=1000" decimation is optional, if end is no bigger then start then return all</param>
        /// <param name="format">format is "array"(default) or "complex" </param>
        /// <returns></returns>
        public async Task<object> GetDataAsync<T>(Signal signal, string fragment, string format)
        {
            //var waveSig = signal as FixedIntervalWaveSignal<T>;
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception(ErrorMessages.NotValidSignalError);
            }
            WaveFragment frag = null;
            try
            {
                frag = WaveFragment.Parse(fragment);
            }
            catch (Exception)
            {
                throw new Exception("Fragment parse error!");
            }
            //end smaller then start then read all
            if (frag.Start >= frag.End)
            {
                frag.Start = waveSig.StartTime;
                frag.End = waveSig.EndTime;
            }
            if (frag.Start < waveSig.StartTime)
            {
                frag.Start = waveSig.StartTime;
            }
            if (frag.End > waveSig.EndTime)
            {
                frag.End = waveSig.EndTime;
            }
            //calc the decimationfactor is count valid
            if (frag.Count > 0)
            {
                //todo can be optimized a little
                frag.DecimationFactor = (long)Math.Floor(((frag.End - frag.Start) / waveSig.SampleInterval + 1) / frag.Count);
            }
            if (frag.DecimationFactor < 1)
            {
                frag.DecimationFactor = 1;
            }

            var startIndex = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
            var count = (long)Math.Floor((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor) + 1;

            ICursor<T> cursor = await myStorageEngine.GetCursorAsync<T>(waveSig.Id, new List<long> { startIndex }, new List<long> { count }, new List<long> { frag.DecimationFactor });
            List<T> resultArray = new List<T>();
            //防止读时越界 review cursor 目前一次只能读1000个点？
            while (cursor.LeftPoint > 1000)
            {
                resultArray.AddRange(await cursor.Read(1000));
            }
            if (cursor.LeftPoint > 0)
            {
                resultArray.AddRange(await cursor.Read(cursor.LeftPoint));
            }
            if (format == "complex")
            {
                return new FixedIntervalWaveComplex<T>
                {
                    Title = waveSig.Path,
                    Start = startIndex * waveSig.SampleInterval + waveSig.StartTime,
                    End = waveSig.StartTime + ((count - 1) * frag.DecimationFactor + startIndex) * waveSig.SampleInterval,
                    Count = count,
                    Data = resultArray,
                    DecimatedSampleInterval = waveSig.SampleInterval * frag.DecimationFactor,
                    OrignalSampleInterval = waveSig.SampleInterval,
                    DecimationFactor = frag.DecimationFactor,
                    StartIndex = startIndex,
                    Unit = waveSig.Unit
                };
            }
            else if (format == "point")
            {
                var points = new List<SignalPoint<T>>();
                // 计算首尾所需取的采样间隔（一致）、点数
                //bug review 这里是不是也有点重复？，我也没看懂，我大概的理解是，不管用户请求什么数据，都把全部的数据返回给用户，不过用户请求区域以外的数据，点数更加稀疏一些（100个点？）？这个逻辑和其他的读取不一样呀，有问题！！！
                long headtailDecimationFactor = 0;
                long headtailMaxCount = 100;
                if ((frag.Start - waveSig.StartTime) > (waveSig.EndTime - frag.End))
                {
                    headtailDecimationFactor = (long)Math.Floor(((frag.Start - waveSig.StartTime) / waveSig.SampleInterval + 1) / headtailMaxCount);
                }
                else
                {
                    headtailDecimationFactor = (long)Math.Floor(((waveSig.EndTime - frag.End) / waveSig.SampleInterval + 1) / headtailMaxCount);
                }
                if (headtailDecimationFactor < 1)
                {
                    headtailDecimationFactor = 1;
                }
                var headCount = (long)Math.Floor((frag.Start - waveSig.StartTime) / waveSig.SampleInterval / headtailDecimationFactor) + 1;
                var tailCount = (long)Math.Floor((waveSig.EndTime - frag.End) / waveSig.SampleInterval / headtailDecimationFactor) + 1;
                //添加前段概略点
                double x = waveSig.StartTime;
                if (headCount > 1)
                {
                    ICursor<T> headCursor = await myStorageEngine.GetCursorAsync<T>(waveSig.Id, new List<long> { 0 }, new List<long> { headCount }, new List<long> { headtailDecimationFactor });
                    //review 这里不应该用foreach，顺序不定，而且即使用循环也不太好，效率有点低，不过好像也没什么别的办法，最好还是不要生成x序列，太浪费服务器资源了，以后，以后我们前端最好还是不要用这个point
                    foreach (var data in await headCursor.Read(headCursor.LeftPoint))
                    {
                        points.Add(new SignalPoint<T>(x, data));
                        x += waveSig.SampleInterval * headtailDecimationFactor;
                    }
                }
                //添加中间详细点
                x = frag.Start;
                foreach (var data in resultArray)
                {
                    points.Add(new SignalPoint<T>(x, data));
                    x += waveSig.SampleInterval * frag.DecimationFactor;
                }
                //添加后段概略点
                if (tailCount > 1)
                {
                    ICursor<T> tailCursor = await myStorageEngine.GetCursorAsync<T>(waveSig.Id, new List<long> { 0 }, new List<long> { tailCount }, new List<long> { headtailDecimationFactor });
                    x = frag.End;
                    foreach (var data in await tailCursor.Read(tailCursor.LeftPoint))
                    {
                        points.Add(new SignalPoint<T>(x, data));
                        x += waveSig.SampleInterval * headtailDecimationFactor;
                    }
                }
                return points;
            }
            else
            {
                return resultArray;
            }
        }
   
        public  IWriter<T> GetWriter<T>(Signal signal)
        {
            return  myStorageEngine.GetWriter<T>(signal);
        }
        /// <summary>
        /// the data is a T[], this append data to the signal, the signal should aready contains the metadata
        /// </summary>
        /// <typeparam name="T">sample type</typeparam>
        /// <param name="signal">the signal you need to append</param>
        /// <param name="fragment"> not used</param>
        /// <param name="data">T[], the data to append</param>
        /// <returns></returns>
        public async Task PutDataAsync<T>(Signal signal, string fragment, object data)
        {
            var waveSig = signal as FixedIntervalWaveSignal;
            //var waveSig = signal as FixedIntervalWaveSignal<T>;
            if (waveSig == null)
            {
                throw new Exception("Signal data type not supported!");
            }
            var dataArray = data as T[];
            if (dataArray == null)
            {
                throw new Exception("Data Type not supported");
            }

            //todo use array instead of list
            //using (IWriter<T> writer = myStorageEngine.GetWriter<T>(waveSig))
            //{
            //    await writer.AppendSampleAsync(new List<long>(), dataArray.ToList());
            //}
            IWriter<T> writer = myStorageEngine.GetWriter<T>(waveSig);
            await writer.AppendSampleAsync(new List<long>(), dataArray.ToList());
            await writer.DisposeAsync();
            //      await myStorageEngine.AppendSampleAsync(waveSig.Id, new List<long>(), dataArray.ToList());
            //update the signal
            if (waveSig.EndTime == waveSig.StartTime)
            {
                waveSig.EndTime += (dataArray.Count() - 1) * waveSig.SampleInterval;
            }
            else
            {
                waveSig.EndTime += dataArray.Count() * waveSig.SampleInterval;
            }

            //todo change to update, this may even not work
            //    await MyCoreService.AddJdbcEntityToAsync(waveSig.ParentId, waveSig);
            await myCoreService.SaveAsync(waveSig);  //modified by liuqiang
            //    await MyCoreService.AddJdbcEntityToAsync(waveSig.ParentId, waveSig);
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

        public Task<object> GetDataAsync(Signal signal, string fragment, string format)
        {
            throw new NotImplementedException();
        }

        public Task PutDataAsync(Signal signal, string fragment, object data)
        {
            throw new NotImplementedException();
        }

        public Task<ICursor> GetCursorAsync(Signal signal, string dataFragment)
        {
            throw new NotImplementedException();
        }

        public void DisposeAsync()
        {
            throw new NotImplementedException();
        }

        Task IDataTypePlugin.DisposeAsync(Guid Id)
        {
            throw new NotImplementedException();
        }
    }

    public class WaveFragment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public long DecimationFactor { get; set; }
        public long Count { get; set; }

        /// <summary>
        /// parse the fragment string, 
        /// "start=t1&end=t2&decimation=2" decimation is optional
        /// </summary>
        /// <param name="fragment"></param>
        /// <returns></returns>
        public static WaveFragment Parse(string fragment)
        {
            //"start=t1&end=t2&decimation=2&count=1000" decimation is optional
            if (fragment.StartsWith("#"))
            {
                fragment = fragment.Substring(1);
            }
            var sections = fragment.Split(new char[] { '&' });
            var result = new WaveFragment { DecimationFactor = 1, Start = 0, End = 0, Count = 0 };
            foreach (var sec in sections)
            {
                if (sec.Contains("start"))
                {
                    result.Start = double.Parse(sec.Substring(6));
                }
                if (sec.Contains("end"))
                {
                    result.End = double.Parse(sec.Substring(4));
                }
                if (sec.Contains("decimation"))
                {
                    result.DecimationFactor = long.Parse(sec.Substring(11));
                }
                if (sec.Contains("count"))
                {
                    result.Count = long.Parse(sec.Substring(6)) > 0 ? long.Parse(sec.Substring(6)) : 0;
                }
            }
            return result;
        }
    }
}
