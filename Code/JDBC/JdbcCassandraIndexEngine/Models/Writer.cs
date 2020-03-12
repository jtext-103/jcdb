using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
using ZeroFormatter;
using System.IO;
using Cassandra.Mapping;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using System.Diagnostics;
using System.Threading;

namespace Jtext103.JDBC.JdbcCassandraIndexEngine.Models
{
    public class Writer<T> : IWriter<T>
    {
        private Guid signalId;

        private IMapper mapper;

        private long sampleCount;

        private string lastDimension;

        private JDBCEntity mySignal;

        private Dictionary<string, PayloadCache<T>> cacheBuffer;

        internal Writer(JDBCEntity signal, IMapper myMapper)
        {
            mySignal = signal;
            signalId = signal.Id;
            mapper = myMapper;
            this.sampleCount = signal.NumberOfSamples;
            lastDimension = "START";
            cacheBuffer = new Dictionary<string, PayloadCache<T>>();
        }

        /// <summary>
        /// 把一维的数组变成字符串形式，中间用逗号隔开
        /// </summary>
        /// <param name="dim"></param>
        /// <returns></returns>
        private string DimensionsToText(List<long> dim)
        {
            string dimension = "";
            for (int i = 0; i < dim.Count(); i++)
            {
                dimension = dimension + dim[i].ToString() + ',';
            }
            return dimension;
        }

        public async Task AppendSampleAsync(List<long> dim, List<T> samples)
        {
            //   mySignal.IsWritting = true;
            long index;
            string dimension = DimensionsToText(dim);
            if (mySignal.IsWritting == true)
            {
                cacheBuffer[dimension].templeSample.AddRange(samples);
            }
            mySignal.IsWritting = true;
            if (lastDimension.Equals(dimension))
            {
                List<T> templeSamples = cacheBuffer[dimension].templeSample;
                long templeIndex = cacheBuffer[dimension].templeIndex;

                if (samples.Count + templeSamples.Count >= sampleCount)
                {
                    //补充完再添加
                    List<T> data = new List<T>();
                    templeSamples.AddRange(samples);
                    //把缓存Buffer里的数据写入数据库
                    while (templeSamples.Count >= sampleCount)
                    {
                        index = templeIndex + 1;
                        //保证每个Payload大小为sampleCount
                        for (int i = 0; i < sampleCount; i++)
                        {
                            data.Add(templeSamples[i]);
                        }
                        templeSamples.RemoveRange(0, (int)sampleCount);
                        await writeDataAsync(data, dimension, index);
                        templeIndex = index;
                    }
                    cacheBuffer[dimension].templeIndex = templeIndex;
                }
                else
                {
                    //把数据保存在缓存里
                    cacheBuffer[dimension].templeSample.AddRange(samples);
                }
            }
            else
            {
                List<T> templeSamples = new List<T>();
                long templeIndex = -1;
                if (cacheBuffer.ContainsKey(dimension))
                {
                    templeSamples.AddRange(cacheBuffer[dimension].templeSample);
                    templeIndex = cacheBuffer[dimension].templeIndex;
                }
                else
                {
                    SEPayload lastPayload = (await mapper.FetchAsync<SEPayload>("SELECT * FROM sepayload where parentid=? and dimensions=?", signalId, dimension)).LastOrDefault();
                    PayloadCache<T> payloadCache = new PayloadCache<T>();
                    if (lastPayload != null)
                    {
                        templeSamples=ZeroFormatterSerializer.Deserialize<List<T>>(lastPayload.samples);
                        //var om = new MemoryStream(lastPayload.samples);
                        //templeSamples = Serializer.Deserialize<List<T>>(om);
                        templeIndex = lastPayload.indexes - 1;

                        payloadCache.templeIndex = templeIndex;
                        payloadCache.templeSample = templeSamples;
                    }
                    cacheBuffer.Add(dimension, payloadCache);
                }
                if (samples.Count + templeSamples.Count >= sampleCount)
                {
                    List<T> data = new List<T>();
                    templeSamples.AddRange(samples);
                    while (templeSamples.Count >= sampleCount)
                    {
                        index = templeIndex + 1;
                        for (int i = 0; i < sampleCount; i++)
                        {
                            data.Add(templeSamples[i]);
                        }
                        templeSamples.RemoveRange(0, (int)sampleCount);
                        await writeDataAsync(data, dimension, index);
                        templeIndex = index;
                    }
                    cacheBuffer[dimension].templeSample.Clear();
                    cacheBuffer[dimension].templeSample.AddRange(templeSamples);
                    cacheBuffer[dimension].templeIndex = templeIndex;
                    lastDimension = dimension;
                }
                else
                {
                    cacheBuffer[dimension].templeSample.AddRange(samples);
                    lastDimension = dimension;
                }
            }
            mySignal.IsWritting = false;
        }

        private async Task writeDataAsync(List<T> samples, string dim, long index)
        {
            //    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   序列化payload");
            //var ms = new MemoryStream();
            //Serializer.Serialize(ms, samples);
            //byte[] result = ms.ToArray();
            byte[] result = ZeroFormatterSerializer.Serialize(samples);
        //    Debug.WriteLine("size: "+result.Count());
        //    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   结束序列化payload");
            SEPayload newPayload = new SEPayload();
            newPayload.parentid = signalId;
            newPayload.indexes = index;
            newPayload.samples = result;
            newPayload.dimensions = dim;
        //    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   插入payload");
            await mapper.InsertAsync<SEPayload>(newPayload);
        //    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   结束插入payload");
        }

        public async Task DisposeAsync()
        {
            // 把缓存数据写入数据库
            List<T> templeSamples = new List<T>();
            long templeIndex = -1;
            foreach (var item in cacheBuffer)
            {
                templeSamples = item.Value.templeSample;
                templeIndex = item.Value.templeIndex;

                if (templeSamples.Count > 0)
                {
                    long index = templeIndex + 1;
                    await writeDataAsync(templeSamples, item.Key, index);
                }
            }
            //清空缓存，释放“写”锁
            cacheBuffer = null;
            mySignal.IsWritting = false;
            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   结束插入payload");
        }

        #region IDisposable Support
        //private bool disposedValue = false; // 要检测冗余调用

        //protected virtual async Task Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: 释放托管状态(托管对象)。
        //        }
        //        // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
        //        // TODO: 将大型字段设置为 null。
        //        List<T> templeSamples = new List<T>();
        //        long templeIndex = -1;
        //        foreach (var item in cacheBuffer)
        //        {
        //            templeSamples = item.Value.templeSample;
        //            templeIndex = item.Value.templeIndex;

        //            if (templeSamples.Count > 0)
        //            {
        //                long index = templeIndex + 1;
        //                await writeDataAsync(templeSamples, item.Key, index);
        //            }
        //        }
        //        cacheBuffer = null;
        //        mySignal.IsWritting = false;
        //        disposedValue = true;
        //    }
        //}

        //// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        //~Writer()
        //{
        //    // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //    Dispose(false);
        //}

        //// 添加此代码以正确实现可处置模式。                                                                                                                                                                                                                              
        //public async void Dispose()
        //{
        //    // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //    await Dispose(true);
        //    // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
        //    GC.SuppressFinalize(this);
        //}
        #endregion
    }

    internal class PayloadCache<T>
    {
        public long templeIndex;
        public List<T> templeSample;

        public PayloadCache()
        {
            templeIndex = -1;
            templeSample = new List<T>();
        }
    }
}
