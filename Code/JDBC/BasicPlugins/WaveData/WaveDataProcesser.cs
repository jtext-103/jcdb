using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using BasicPlugins.TypedSignal;
using Jtext103.JDBC.Core.Api;
using System.Threading;

namespace BasicPlugins
{
    /// <summary>
    /// basic fixed interval timebase wave data plugin ,
    /// signal requiement: FixedIntervalWaveSignal or derived from it
    /// fragment: "start=t1&end=t2&decimation=2&count=1000" decimation is optional, if end is no bigger then start then return all
    /// count over decimation, if count is set, decimation will be ignored, you may not get the exact count as you specified
    /// </summary>
    public class WaveDataProcesser<T> : IDataProcesser
    {
        private CoreService myCoreService { get; set; }
        private IMatrixStorageEngineInterface myStorageEngine { get; set; }

        public WaveDataProcesser(CoreService coreService,string dataType,string name)
        {
            myCoreService = coreService;
            myStorageEngine = coreService.StorageEngine as IMatrixStorageEngineInterface;
            if (myStorageEngine == null)
            {
                throw new Exception(ErrorMessages.NotValidStorageEngine);
            }
            DataType = dataType;
            Name = name;
        }
        //可删除不用
        public string Name { get; set; }
        public string DataType { get; set; }
        private IWriter<T> myWriter { get; set; }
        //  public async Task<ICursor> GetCursorAsync<T>(Signal signal, string fragment)
        /// <summary>
        /// 使用Cursor读取大数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment"></param>
        /// <returns></returns>
        public async Task<ICursor> GetCursorAsync(Signal signal, string dataFragment)
        {
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception(ErrorMessages.NotValidSignalError);
            }
            WaveFragment frag = null;
            try
            {
                frag = WaveFragment.Parse(waveSig, dataFragment);
            }
            catch (Exception)
            {
                throw new Exception(ErrorMessages.NotValidSignalFragmentError);
            }

            var startIndex = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
            var count = (long)Math.Floor((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor) + 1;

            return await myStorageEngine.GetCursorAsync<T>(waveSig.Id, new List<long> { startIndex }, new List<long> { count }, new List<long> { frag.DecimationFactor });
        }

        /// <summary>
        /// format is "array"(default) or "complex" 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signal"></param>
        /// <param name="fragment">fragment: "start=t1&end=t2&decimation=2&count=1000" decimation is optional, if end is no bigger then start then return all</param>
        /// <param name="format">format is "array"(default) or "complex" </param>
        /// <returns></returns>
        public async Task<object> GetDataAsync(Signal signal, string fragment, string format)
        {
          //  var waveSig = signal as FixedIntervalWaveSignal<T>;
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception(ErrorMessages.NotValidSignalError);
            }
            WaveFragment frag = null;
            try
            {
                frag = WaveFragment.Parse(waveSig, fragment);
            }
            catch (Exception)
            {
                throw new Exception("Fragment parse error!");
            }

            var startPoint = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
            var count = (long)Math.Floor((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor)+1;
         //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")+ "  getCursor");
            ICursor<T> cursor = await myStorageEngine.GetCursorAsync<T>(waveSig.Id, new List<long> { startPoint }, new List<long> { count }, new List<long> { frag.DecimationFactor });
            List<T> resultArray = new List<T>();
         //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   getData");
            //防止读时越界 review cursor 目前一次只能读1000个点？
            while (cursor.LeftPoint > 1000)
            {
                resultArray.AddRange(await cursor.Read(1000));
            }
            if (cursor.LeftPoint > 0)
            {
                resultArray.AddRange(await cursor.Read(cursor.LeftPoint));
            }
         //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   readFinish");
            if (format == "complex")
            {
                return new FixedIntervalWaveComplex<T>
                {
                    Title = waveSig.Path,
                    Start = startPoint * waveSig.SampleInterval + waveSig.StartTime,
                    End = waveSig.StartTime + ((count - 1) * frag.DecimationFactor + startPoint) * waveSig.SampleInterval,
                    Count = count,
                    Data = resultArray,
                    DecimatedSampleInterval = waveSig.SampleInterval * frag.DecimationFactor,
                    OrignalSampleInterval = waveSig.SampleInterval,
                    DecimationFactor = frag.DecimationFactor,
                    StartIndex = startPoint,
                    Unit = waveSig.Unit
                };
            }
            else if (format == "point")
            {
                var points = new List<SignalPoint<T>>();
                //添加中间详细点
                double x = frag.Start;
                foreach (var data in resultArray)
                {
                    points.Add(new SignalPoint<T>(x, data));
                    x += waveSig.SampleInterval * frag.DecimationFactor;
                }
                return points;
            }
            else
            {
                return resultArray;
            }
        }
        /// <summary>
        /// the data is a T[], this append data to the signal, the signal should aready contains the metadata
        /// </summary>
        /// <typeparam name="T">sample type</typeparam>
        /// <param name="signal">the signal you need to append</param>
        /// <param name="fragment"> not used</param>
        /// <param name="data">T[], the data to append</param>
        /// <returns></returns>

        // public async Task PutDataAsync<T>(Signal signal, string fragment, object data)
        public async Task PutDataAsync(Signal signal, string fragment, object data)
        {
            if (myWriter == null)
            {
                myWriter = myStorageEngine.GetWriter<T>(signal);
            }
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception("Signal data type not supported!");
            }
            var dataArray = data as T[];
            if (dataArray == null)
            {
                throw new Exception("Data Type not supported");
            }

            await myWriter.AppendSampleAsync(new List<long>(), dataArray.ToList());

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
            await myCoreService.SaveAsync(waveSig);
        }

        public async Task DisposeAsync()
        {
            await myWriter.DisposeAsync();
            myWriter = null;
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
        public static WaveFragment Parse(FixedIntervalWaveSignal waveSig, string fragment)
        {
            //"start=t1&end=t2&decimation=2&count=1000" decimation is optional
            if (fragment.StartsWith("#"))
            {
                fragment = fragment.Substring(1);
            }
            var sections = fragment.Split(new char[] { '&' });
            var result = new WaveFragment { DecimationFactor = 1, Start = waveSig.StartTime, End = waveSig.EndTime + waveSig.SampleInterval, Count = 0 };
            foreach (var sec in sections)
            {
                if (sec.Contains("start"))
                {
                    result.Start = double.Parse(sec.Substring(6));
                }
                if (sec.Contains("end"))
                {
                    result.End = double.Parse(sec.Substring(4)) + waveSig.SampleInterval;
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
            //end smaller then start then read all
            if (result.Start >= result.End) {
                result.Start = waveSig.StartTime;
                result.End = waveSig.EndTime;
            }
            if (result.Start < waveSig.StartTime) {
                result.Start = waveSig.StartTime;
            }
            if (result.End > waveSig.EndTime) {
                result.End = waveSig.EndTime;
            }
            //calc the decimationfactor is count valid
            if (result.Count > 0) {
                //todo can be optimized a little
                result.DecimationFactor = (long)Math.Floor(((result.End - result.Start) / waveSig.SampleInterval + 1) / result.Count);
            }
            if (result.DecimationFactor < 1) {
                result.DecimationFactor = 1;
            }
            return result;
        }
    }
}