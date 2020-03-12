using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using ProtoBuf;
using Jtext103.JDBC.JdbcCassandraIndexEngine.Models;
using Jtext103.JDBC.Core.Models;
using Cassandra.Mapping;
using System.IO;
using System.Diagnostics;
using ZeroFormatter;

namespace Jtext103.JDBC.JdbcCassandraIndexEngine.Models
{
    public class Cursor<T> : ICursor<T>
    {
        //review cassandra我不是很懂，到时候讨论，先你学习一下
        private List<IPage<SEPayload>> listIPage { get; set; }
        private List<long> dimensionPage = new List<long>();
        private List<long> dimPage = new List<long>();
        private int k = 0;
        private long lastReadPage { get; set; }

        /// <summary>
        /// 起始点游标
        /// </summary>
        private long startIndex { get; set; }

        /// <summary>
        /// 每个维度读取点的数量
        /// </summary>
        private long count { get; set; }
        /// <summary>
        /// 已经读取点的游标
        /// </summary>
        private long countIndex { get; set; }

        /// <summary>
        /// 间隔点
        /// </summary>
        private long decimation { get; set; }

        /// <summary>
        /// 上次读取位置
        /// </summary>
        private long lastReadIndex { get; set; }

        /// <summary>
        /// 剩余点
        /// </summary>
        private long leftPoint { get; set; }

        private long sampleCount { get; set; }
        public long LeftPoint { get { return leftPoint; } }

        internal Cursor(List<IPage<SEPayload>> listIPage, long start, List<long> countNum, List<long> dimPage, long sampleCount,long decimationFactor = 1)
        {
            lastReadPage = 0;
            this.listIPage = listIPage;
            leftPoint = 1;
            for (int i = 0; i < countNum.Count(); i++)
            {
                leftPoint *= countNum[i];
            }
            startIndex = start;
            count = countNum.LastOrDefault();
            countIndex = countNum.LastOrDefault();
            decimation = decimationFactor;
            this.sampleCount = sampleCount;

            dimensionPage.AddRange(dimPage);
            this.dimPage.AddRange(dimPage);
        }
        internal Cursor(List<IPage<SEPayload>> listIPage)
        {
            this.listIPage = listIPage;
        }

        public async Task<IEnumerable<T>> Read(long resultNum)
        {
            if (resultNum > leftPoint)
            {
                resultNum = leftPoint;
            }
            T[] resultArray = new T[resultNum];
            long resultIndex = 0;

            for (int i = (int)lastReadPage; i < listIPage.Count();)
            {
                List<SEPayload> partialList = new List<SEPayload>();
                IPage<SEPayload> iPage = listIPage[i];

                if (iPage.Count==0)
                {
                    throw new Exception(ErrorMessages.DataNotFoundError); 
                }
                partialList = iPage.ToList();
              //  Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")+ "   readData");
                readPartialPayload(partialList, resultArray, ref resultNum, ref resultIndex);

                if (resultNum == 0)
                {
                    if (countIndex == 0)
                    {
                        countIndex = count;
                        long sum = 0;
                        for (int q = 0; q <= k; q++)
                        {
                            sum = sum + dimPage[q];
                        }
                        //  i++;
                        i = (int)sum;
                        k++;
                        lastReadIndex = startIndex;
                    }
                    leftPoint = leftPoint - resultArray.Count();
                    lastReadPage = i;
                    return resultArray;
                }
                dimensionPage[k]--;
                if (dimensionPage[k] == 0)
                {
                    k++;
                    if (countIndex > 0)
                    {
                        throw new Exception(ErrorMessages.OutOfRangeError);
                    }
                    countIndex = count;
                    //  continue;
                }
                if (countIndex == 0)
                {
                    countIndex = count;
                    long sum = 0;
                    for (int q = 0; q <= k; q++)
                    {
                        sum = sum + dimPage[q];
                    }
                    i = (int)sum;
                    k++;
                    lastReadIndex = startIndex;
                    continue;
                }
                i++;
            }
            return resultArray;
        }
        public async Task<object> ReadObject(long resultNum)
        {
            return await Read(resultNum);
        }
        public IEnumerable<object> IterateCursor(long resultNum) 
        {
            foreach(var data in Read(resultNum).Result) 
            {
                yield return data;
            }
        }
        private void readPartialPayload(List<SEPayload> batch, T[] resultArray, ref long resultNum, ref long resultIndex)
        {
            long index;
            if (countIndex < count)
            {
                index = lastReadIndex;
            }
            else
            {
                index = startIndex;
            }

            long fetchnum = 0;
            if (resultNum >= count)
            {
                if (countIndex < count)
                {
                    fetchnum = countIndex;
                }
                else
                {
                    fetchnum = count;
                }
            }
            else
            {
                if (resultNum >= countIndex)
                {
                    fetchnum = countIndex;
                }
                else
                {
                    fetchnum = resultNum;
                }
            }
            long factor = decimation;
            
            for (int j = 0; j < batch.Count(); j++)
            {
                var item = batch[j];
                long start = sampleCount * (item.indexes);
                long end = sampleCount * (item.indexes + 1)-1;
               
                if (index <= end && index >= start)
                {
                    long getnum = (fetchnum - 1) * factor + 1;
                    long pointer = index + getnum;
                    Int32 indexStart = (Int32)(index - start);
                    Int32 realfetch = 0;
                    var restmp = new List<T>();

                    if ((pointer) <= end)
                    {
                        try
                        {
                            //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   反序列化");
                            var output = ZeroFormatterSerializer.Deserialize<List<T>>(item.samples);
                            //var om = new MemoryStream(item.samples);
                            //var output = Serializer.Deserialize<List<T>>(om);
                            //om.Dispose();
                            restmp = output.GetRange(indexStart, (Int32)getnum);
                            
                         //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   反序列化");
                            //   index = index + getnum;
                            index = index + fetchnum * factor;
                            realfetch = (Int32)fetchnum;
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                    else
                    {
                     //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   反序列化");
                        Int32 fetch = (Int32)(end - index + 1);
                        var output = ZeroFormatterSerializer.Deserialize<List<T>>(item.samples);
                        //var om = new MemoryStream(item.samples);
                        //var output = Serializer.Deserialize<List<T>>(om);
                        //om.Dispose();
                        restmp = output.GetRange(indexStart, fetch);
                        
                     //   Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "   反序列化");
                        realfetch = (Int32)((fetch - 1) / factor + 1);
                        fetchnum = fetchnum - realfetch;
                        index = index + realfetch * factor;
                    }
                    for (int k = 0; k < realfetch; k++)
                    {
                        resultArray[resultIndex++] = restmp[(Int32)factor * k];
                    }
                    resultNum -= realfetch;
                    countIndex -= realfetch;
                    if (countIndex == 0)
                    {
                        break;
                    }
                    if (resultNum == 0)
                    {
                        break;
                    }
                }
            }
            lastReadIndex = index;
        }
        public async Task<IEnumerable<T>> Read(long resultNum, bool initial = false)
        {
            if (initial == true)
            {
                #region
                if (resultNum > leftPoint)
                {
                    resultNum = leftPoint;
                }
                T[] resultArray = new T[resultNum];
                long resultIndex = 0;
                for (int i = (int)lastReadPage; i < listIPage.Count();)
                {
                    List<SEPayload> partialList = new List<SEPayload>();
                    IPage<SEPayload> iPage = listIPage[i];
                    partialList = iPage.ToList();
                    readPartialPayload(partialList, resultArray, ref resultNum, ref resultIndex);
                }
                return resultArray;
                #endregion
            }
            else
            {
                return await Read(resultNum);
            }
        }
    }
}
