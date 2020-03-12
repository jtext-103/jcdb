using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using System.IO;
using Cassandra.Mapping;
using ProtoBuf;
namespace Jtext103.JDBC.CassandraStorageEngine.Models
{
    public class Cursor<T> : ICursor<T>
    {
        //review cassandra我不是很懂，到时候讨论，先你学习一下
        private List<IPage<SEPayload>> listIPage { get; set; }
        private List<long> dimensionPage=new List<long>();
        private List<long> dimPage=new List<long>();
        private int k = 0;
        private long lastReadPage { get; set; }
        private long startIndex { get; set; }
        private long count { get; set; }
        private long countIndex { get; set; }
        private long decimation { get; set; }
        private long lastReadIndex { get; set; }
        private long leftPoint { get; set; }
        public long LeftPoint { get { return leftPoint; } } 

        internal Cursor(List<IPage<SEPayload>> listIPage, long start, List<long> countNum,List<long> dimPage,long decimationFactor = 1)
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

            dimensionPage.AddRange(dimPage);
            this.dimPage.AddRange(dimPage);
        }
      
        public async Task<IEnumerable<T>> Read(long resultNum)
        {
            if (resultNum > leftPoint)
            {
                resultNum = leftPoint;
            }
            T[] resultArray = new T[resultNum];
             long resultIndex = 0;
            
            for (int i = (int)lastReadPage; i < listIPage.Count(); )
            {
                List<SEPayload> partialList = new List<SEPayload>();
                IPage<SEPayload> iPage = listIPage[i];

                partialList = iPage.ToList();
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
        public IEnumerable<object> IterateCursor(long resultNum) {
            foreach (var data in Read(resultNum).Result) {
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

                //review 如果不满足这个payload就丢了是吧？
                if (index <= item.end && index >= item.start)
                {
             //       long getnum = fetchnum * factor;
                    long getnum = (fetchnum - 1) * factor + 1;
                    long pointer = index + getnum;
                    Int32 indexStart = (Int32)(index - item.start);
                    Int32 realfetch = 0;
                    var restmp = new List<T>();
                    
                    if ((pointer) <= item.end)
                    {
                        var om = new MemoryStream(item.samples);
                        var output = Serializer.Deserialize<List<T>>(om);
                        restmp = output.GetRange(indexStart, (Int32)getnum);
                        om.Dispose();
                     //   index = index + getnum;
                        index = index + fetchnum * factor;
                        realfetch = (Int32)fetchnum;
                    }
                    else
                    {
                        Int32 fetch = (Int32)(item.end - index + 1);
                        var om = new MemoryStream(item.samples);
                        var output = Serializer.Deserialize<List<T>>(om);
                        restmp = output.GetRange(indexStart, fetch);
                        om.Dispose();
                        realfetch = (Int32)((fetch - 1) / factor + 1);
                        fetchnum = fetchnum - realfetch;
                       // index = index + fetch;
                        index = index + realfetch*factor;
                    }
                    for (int k = 0; k < realfetch; k++)
                    {
                        resultArray[resultIndex++] = restmp[(Int32)factor * k];
                    }
                    resultNum -= realfetch;
                    countIndex -= realfetch;
                    if ( countIndex == 0)
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
        public async Task<IEnumerable<T>> Read(long resultNum,bool initial=false)
        {
            if (initial==true)
            {
                #region
                if (resultNum > leftPoint)
                {
                    resultNum = leftPoint;
                }
                T[] resultArray = new T[resultNum];
                long resultIndex = 0;
                for (int i = (int)lastReadPage; i < listIPage.Count(); )
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
