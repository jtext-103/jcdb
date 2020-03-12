using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using MongoDB.Driver;

namespace Jtext103.JDBC.MongoStorageEngine.Models
{
    public class Cursor<T> : ICursor<T>
    {
        //review 这里用复数，而且不是jdcb cursor吧应该是mongodb cursor，代码写的要可读！！
        private List<IAsyncCursor<SEPayload<T>>> JdbcCursor { get; set; }
        private long startIndex { get; set; }
        private long count { get; set; }
        private long countIndex { get; set; }
        private long decimation { get; set; }
        private long lastReadIndex { get; set; }
        private long leftPoint { get; set; }
        public long LeftPoint { get { return leftPoint; } }

        internal Cursor(List<IAsyncCursor<SEPayload<T>>> jdbcCursor, long start, List<long> countNum, long decimationFactor = 1)
        {
            JdbcCursor = jdbcCursor;
            startIndex = start;
            count = countNum.LastOrDefault();
            decimation = decimationFactor;
            countIndex = countNum.LastOrDefault();
            leftPoint = 1;
            for (int i = 0; i < countNum.Count(); i++)
            {
                leftPoint *= countNum[i];
            }
        }

        public async Task<IEnumerable<T>> Read(long resultNum)
        {
            if (resultNum > leftPoint)
            {
                resultNum = leftPoint;
            }
            T[] resultArray = new T[resultNum];
            long resultIndex = 0;
            for (int i = 0; i < JdbcCursor.Count(); )
            {
                //有严重 bug review 这个循环变量i就没有用过，也没增加
                var cursor = JdbcCursor[0];
                List<SEPayload<T>> partialList = new List<SEPayload<T>>();
                //review 下面这个代码也真是重复的厉害，，用do while不就完了吗
                if (countIndex < count)
                {
                    //读过了至少一次
                    partialList = cursor.Current.ToList();
                    readPartialPayload(partialList, resultArray, ref resultNum, ref resultIndex);
                    if (resultNum == 0)
                    {
                        if (countIndex == 0)
                        {
                            countIndex = count;
                        }
                        leftPoint = leftPoint - resultArray.Count();
                        return resultArray;
                    }
                }
                while (await cursor.MoveNextAsync())
                {
                    partialList = cursor.Current.ToList();
                    readPartialPayload(partialList, resultArray, ref resultNum, ref resultIndex);
                    if (resultNum == 0)
                    {
                        //readFlag = true;
                        if (countIndex == 0)
                        {
                            countIndex = count;
                        }
                        leftPoint = leftPoint - resultArray.Count();
                        return resultArray;
                    }
                }
                countIndex = count;
            }
            leftPoint = leftPoint - resultArray.Count();
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
        //review 这个方法太长了，十分不可读。。而且一条注释也没有。。。和理由重采样的代码吧，没法看。。变脸是干嘛的也没有注释解释一下，，，
        private void readPartialPayload(List<SEPayload<T>> batch, T[] resultArray, ref long resultNum, ref long resultIndex)
        {
            long index;//review 啥index呀。。。。注释注释。很多变量都没有注释看不懂。。。我知道我以前写的时候业主是不全。。。我错了
            bool lastReadIndexFlag = false;
            bool removeFlag = false;
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
                removeFlag = true;
                lastReadIndexFlag = true;
            }
            else
            {
                if (resultNum >= countIndex)
                {
                    fetchnum = countIndex;
                    removeFlag = true;
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

                if (index <= item.End && index >= item.Start)
                {
                    long getnum = (fetchnum - 1) * factor + 1;
                  //  long getnum = fetchnum  * factor;
                    long pointer = index + getnum;
                    Int32 indexStart = (Int32)(index - item.Start);
                    Int32 realfetch = 0;
                    var restmp = new List<T>();
                    if ((pointer) <= item.End)
                    {
                        restmp = item.Samples.GetRange(indexStart, (Int32)getnum);
                        index = index + getnum;
                        realfetch = (Int32)fetchnum;
                    }
                    else
                    {
                        Int32 fetch = (Int32)(item.End - index + 1);
                        restmp = item.Samples.GetRange(indexStart, fetch);
                        realfetch = (Int32)((fetch-1) / factor+1);
                        fetchnum = fetchnum - realfetch;
                        index = index + fetch;
                    }
                    for (int k = 0; k < realfetch; k++)
                    {
                        resultArray[resultIndex++] = restmp[(Int32)factor * k];
                    }
                    resultNum -= realfetch;
                    countIndex -= realfetch;
                    if (removeFlag && countIndex == 0)
                    {
                        JdbcCursor.RemoveAt(0);
                        break;
                    }
                }
            }
            if (!lastReadIndexFlag)
            {
                lastReadIndex = index;
            }
        }
    }
}
