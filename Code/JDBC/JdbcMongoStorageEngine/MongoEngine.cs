using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.StorageEngineInterface;
using MongoDB.Driver;
using MongoDB.Bson;
using Jtext103.JDBC.MongoStorageEngine.Models;
using ProtoBuf;
using System.IO;

namespace Jtext103.JDBC.MongoStorageEngine
{
    public class MongoEngine: IMatrixStorageEngineInterface
    {
        private MongoClient client;
        private IMongoCollection<SEPayload> collectionSEPayload;
        private IMongoDatabase database;

        public MongoEngine() { }
        public async Task ClearDb()
        {
            await database.DropCollectionAsync("Payload");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configString">host=xxx&database=xxx&collection=xxx</param>
        public void Init(string configString)
        {
            string[] split = configString.Split('&');
            string hostName="", databaseName="", collectionName="";
            foreach (var item in split)
            {
                int startIndex=item.IndexOf("=");
                string head = item.Substring(0, startIndex).Trim();
                string name = item.Substring(startIndex + 1).Trim();
                switch (head)
                {
                        //review todo 很多地方都用了类似的字符串分割，可不可以顶一个格式，然后写个函数统一来parse？？
                        //mongodb和cassadnra的没法统一
                    case "host": hostName = name; break;
                    case "database": databaseName = name; break;
                    case "collection": collectionName = name; break;
                    default: throw new Exception(ErrorMessages.NotValidConfigDatabaseError);
                }
            }
            client = new MongoClient(hostName);
            database = client.GetDatabase(databaseName);
            collectionSEPayload = database.GetCollection<SEPayload>(collectionName);
            //review 这个是什么意思？
            //索引的key
            var keys = Builders<SEPayload>.IndexKeys.Ascending("ParentId").Ascending("Dimensions").Descending("End");
            collectionSEPayload.Indexes.CreateOneAsync(keys).Wait();
        }

        public void Connect(string configString)
        {
            string[] split = configString.Split('&');
            string hostName = "", databaseName = "", collectionName = "";
            foreach (var item in split)
            {
                int startIndex = item.IndexOf("=");
                string head = item.Substring(0, startIndex).Trim();
                string name = item.Substring(startIndex + 1).Trim();
                switch (head)
                {
                    //review todo 很多地方都用了类似的字符串分割，可不可以顶一个格式，然后写个函数统一来parse？？
                    //mongodb和cassadnra的没法统一
                    case "host": hostName = name; break;
                    case "database": databaseName = name; break;
                    case "collection": collectionName = name; break;
                    default: throw new Exception(ErrorMessages.NotValidConfigDatabaseError);
                }
            }
            client = new MongoClient(hostName);
            database = client.GetDatabase(databaseName);
            collectionSEPayload = database.GetCollection<SEPayload>(collectionName);
        }

        /// <summary>
        /// Delete all the data of the signal
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        public async Task DeleteDataAsync(Guid signalId)
        {
            var filter = Builders<SEPayload>.Filter.Eq("ParentId", signalId);
            await collectionSEPayload.DeleteManyAsync(filter);
        }

        #region get set samples

        //review 这个也是要增加注释，话流程图的
        public async Task<ICursor<T>> GetCursorAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            List<IAsyncCursor<SEPayload<T>>> listCursor = new List<IAsyncCursor<SEPayload<T>>>();
            if (decimationFactor == null)
            {
                decimationFactor = new List<long>();
                for (int i = 0; i < start.Count; i++)
                {
                    decimationFactor.Add(1);
                }
            }
            long num = count.LastOrDefault();
            long factor = decimationFactor[decimationFactor.Count() - 1];
            //from the second lowest rank find each line to read
            long lineCount = 1;
            //how many lines to read
            for (int i = 0; i < count.Count - 1; i++)
            {
                lineCount = lineCount * count[i];
            }

            List<long> lineCursor = new List<long>(start.Count - 1);
            List<long> lineCounter = new List<long>(start.Count - 1);
            //the line cursor has every but last rank of the start point, point to the start line
            lineCursor.AddRange(start.Where((v, i) => i < start.Count - 1));
            lineCounter.AddRange(count.Where((v, i) => i < count.Count - 1));
            for (long line = 0; line < lineCount; line++)
            {
                long index = start.LastOrDefault();
                //review    这个几个region都没有名字。？做啥的？
               
                #region 
                var builder = Builders<SEPayload>.Filter;
                var filters = new List<FilterDefinition<SEPayload>>();
                filters.Add(builder.Eq("ParentId", signalId));
                filters.Add(builder.Eq("Dimensions", lineCursor));
                var sot = Builders<SEPayload>.Sort.Ascending("End");
                var options = new FindOptions<SEPayload, SEPayload<T>> { Sort = sot };
                IAsyncCursor<SEPayload<T>> cursor = await collectionSEPayload.FindAsync(builder.And(filters), options);
                //review 为什么一次读取会有多个指针？？，为什么不把filter逻辑合并起来一次查询一个指针？
                //???
                listCursor.Add(cursor);

                #endregion
                for (var rankIndex = lineCursor.Count - 1; rankIndex >= 0; rankIndex--)
                {
                    //try read next line
                    lineCounter[rankIndex] -= 1;
                    if (lineCounter[rankIndex] == 0)
                    {
                        lineCounter[rankIndex] = count[rankIndex];
                        lineCursor[rankIndex] = start[rankIndex];
                    }
                    else
                    {
                        //move this rank forward 
                        lineCursor[rankIndex] += decimationFactor[rankIndex];
                        break;
                    }
                }
            }
            Cursor<T> jdbccursor = new Cursor<T>(listCursor, start.LastOrDefault(), count, decimationFactor.LastOrDefault());
            return jdbccursor;
        }
        
        /// <summary>
        /// get the samples of a signal in in multiple dimention,
        /// Important:this will return a block of signal in all dimentions make sure the start, count decimationFactor are with the same dimentions
        /// </summary>
        /// <typeparam name="T">the sample type</typeparam>
        /// <param name="start">the start point, hi rank dimension comes in first in the list</param>
        /// <param name="signalId">singal id</param>
        /// <param name="count">how many point in the EACH dim you want to get, hi rank dimension comes in first in the list</param>
        /// <param name="decimationFactor">the decimation  factor in EACH dimensions is larger than one the signal is down
        /// sample to this factor e.g. set to 3, then it will pick 1sample in 3, skip every 2, can not be set less than 1,
        /// hi rank dimension comes in first in the list</param>
        /// <returns>sample array</returns>
       [Obsolete]
        public async Task<IEnumerable<T>> GetSamplesAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            long flatLenth = 1;
            foreach (var rank in count)
            {
                flatLenth = flatLenth * rank;
            }
            var resultArray = new T[flatLenth];
            if (decimationFactor == null)
            {
                decimationFactor = new List<long>();
                for (int i = 0; i < start.Count; i++)
                {
                    decimationFactor.Add(1);
                }
            }
            long num = count.LastOrDefault();
            long factor = decimationFactor[decimationFactor.Count() - 1];
            //from the second lowest rank find each line to read
            long lineCount = 1;
            //how many lines to read
            for (int i = 0; i < count.Count - 1; i++)
            {
                lineCount = lineCount * count[i];
            }
            List<long> lineCursor = new List<long>(start.Count - 1);
            List<long> lineCounter = new List<long>(start.Count - 1);
            //the line cursor has every but last rank of the start point, point to the start line
            lineCursor.AddRange(start.Where((v, i) => i < start.Count - 1));
            lineCounter.AddRange(count.Where((v, i) => i < count.Count - 1));
            long resultIndex = 0;
            for (long line = 0; line < lineCount; line++)
            {
                //todo liuqiang read line lineCursor[], just read the SEPayload, may use multi-thread, use slice to decimate the line, then insert to the currect location of the result in this case just append
                //use Array.Copy
                long index = start.LastOrDefault();
                #region
                //可以只取sample，到时优化；用不了slice。。。
                var builder = Builders<SEPayload>.Filter;
                var filters = new List<FilterDefinition<SEPayload>>();
                filters.Add(builder.Eq("ParentId", signalId));
                filters.Add(builder.Eq("Dimensions", lineCursor));
                var sot = Builders<SEPayload>.Sort.Ascending("End");
                //     var pro = Builders<JDBCEntity>.Projection.Slice("Samples", index, num);
                var options = new FindOptions<SEPayload, SEPayload<T>> { Sort = sot };
                using (var cursor = await collectionSEPayload.FindAsync(builder.And(filters), options))
                {
                    var batch = await cursor.ToListAsync<SEPayload<T>>();
                    if (batch.Count() == 0)
                    {
                        throw new Exception(ErrorMessages.SampleNotExistsError);
                    }

                    for (int i = 0; i < batch.Count(); i++)
                    {
                        var item = batch[i];
                        if (index <= item.End && index >= item.Start)
                        {
                            long getnum = num * factor;
                            long pointer = index + getnum;
                            if ((pointer) <= item.End)
                            {
                                Int32 indexStart = (Int32)(index - item.Start);
                                var restmp = item.Samples.GetRange(indexStart, (Int32)getnum);
                                for (int j = 0; j < num; j++)
                                {
                                    resultArray[resultIndex++] = restmp[(Int32)factor * j];
                                    //  listresult.Add(restmp[factor * j]);
                                }
                            }
                            else
                            {
                                Int32 fetch = (Int32)(item.End - index + 1);
                                Int32 indexStart = (Int32)(index - item.Start);
                                var restmp = item.Samples.GetRange(indexStart, fetch);
                                num = num - fetch;
                                index = index + fetch;
                                for (int j = 0; j < fetch; j++)
                                {
                                    resultArray[resultIndex++] = restmp[(Int32)factor * j];
                                    // listresult.Add(restmp[factor * j]);
                                }
                            }
                        }
                    }
                }
                #endregion
                for (var rankIndex = lineCursor.Count - 1; rankIndex >= 0; rankIndex--)
                {
                    //try read next line
                    lineCounter[rankIndex] -= 1;
                    if (lineCounter[rankIndex] == 0)
                    {
                        //if there are no next line to read in this rank then reset this rank
                        //and continue the loop to move forward the next rank
                        lineCounter[rankIndex] = count[rankIndex];
                        lineCursor[rankIndex] = start[rankIndex];
                    }
                    else
                    {
                        //move this rank forward 
                        lineCursor[rankIndex] += decimationFactor[rankIndex];
                        break;
                    }
                }
            }
            return resultArray;
        }

        #endregion
        /// <summary>
        /// get a SEPayload entity by its id
        /// </summary>
        /// <param name="id">SEPayload id</param>
        /// <returns></returns>
        public async Task<SEPayload<T>> GetPayloadByIdAsync<T>(Guid id)
        {
            IFindFluent<SEPayload, SEPayload> findFluent = (collectionSEPayload.Find<SEPayload>(x => x.Id == id));
            SEPayload<T> entity = (SEPayload<T>)(await findFluent.FirstOrDefaultAsync());
            return entity;
        }

        //review 这个这么重要，也没注释，payload， dim这些关系我忘的差不多了也没个文档。我相信有一天你也会忘得差不多的。。到时候没法再改了
        public async Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, bool createNewSEPayload = false)
        {
            //存在Signal,找到子节点满足  父节点Id=signal，dimension=dim
            var builder = Builders<SEPayload>.Filter;
            var filter = new List<FilterDefinition<SEPayload>>();
            filter.Add(builder.Eq("ParentId", signalId));
            filter.Add(builder.Eq("Dimensions", dim));
            var sot = Builders<SEPayload>.Sort.Descending("End");
            var options = new FindOptions<SEPayload, SEPayload<T>> { Sort = sot, Limit = 1 };
            //review todo 每次append的时候还要读取，是这样的，append时读取时非常影响吸能的，如果可以避免一定要避免 
            using (var cursor = await collectionSEPayload.FindAsync(builder.And(filter), options))
            {
                await cursor.MoveNextAsync();
                if (cursor.Current.Count() == 0)
                {
                    //没有这样的SEPayload，不管createNewSEPayload是不是真，都要创建一个新的SEPayload，
                    //创建新的SEPayload时，是否会超出范围
                    await CreateNewPayloadAsync<T>(signalId, dim, samples, null);
                }
                else
                {
                    // 有这样的SEPayload，createNewSEPayload为真时，直接创建；为假时，先接上再创建
                    SEPayload<T> endSEPayload = (SEPayload<T>)cursor.Current.FirstOrDefault();
                    if (createNewSEPayload == true)
                    {
                        await CreateNewPayloadAsync<T>(signalId, dim, samples, null, endSEPayload.End + 1);
                    }
                    else
                    {
                        bool flag = false;
                        try
                        {
                            var end = endSEPayload.End + samples.Count();
                            var up = Builders<SEPayload>.Update.PushEach("Samples", samples).Set("End", end);
                            var endfilter = Builders<SEPayload>.Filter.Eq("Id", endSEPayload.Id);
                            await collectionSEPayload.UpdateOneAsync(endfilter, up);
                            flag = false;
                        }
                        catch (MongoWriteException)
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            await CreateNewPayloadAsync<T>(signalId, dim, samples, null, endSEPayload.End + 1);
                        }
                    }
                }
            }
          
        }
        /*
        public async Task<List<MatrixStorageEngine.Models.SESampleAppendResult>> AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, bool createNewSEPayload = false)
        {
            List<SESampleAppendResult> listResult = new List<SESampleAppendResult>();
            //存在Signal,找到子节点满足  父节点Id=signal，dimension=dim
            var builder = Builders<SEPayload>.Filter;
            var filter = new List<FilterDefinition<SEPayload>>();
            filter.Add(builder.Eq("ParentId", signalId));
            filter.Add(builder.Eq("Dimensions", dim));
            var sot = Builders<SEPayload>.Sort.Descending("End");
            var options = new FindOptions<SEPayload, SEPayload<T>> { Sort = sot, Limit = 1 };
            using (var cursor = await collectionSEPayload.FindAsync(builder.And(filter), options))
            {
                await cursor.MoveNextAsync();
                if (cursor.Current.Count() == 0)
                {
                    //没有这样的SEPayload，不管createNewSEPayload是不是真，都要创建一个新的SEPayload，
                    //创建新的SEPayload时，是否会超出范围
                    await CreateNewPayloadAsync<T>(signalId, dim, samples, listResult);
                }
                else
                {
                    // 有这样的SEPayload，createNewSEPayload为真时，直接创建；为假时，先接上再创建
                    SEPayload<T> endSEPayload = (SEPayload<T>)cursor.Current.FirstOrDefault();
                    if (createNewSEPayload == true)
                    {
                        await CreateNewPayloadAsync<T>(signalId, dim, samples, listResult, endSEPayload.End + 1);
                    }
                    else
                    {
                        bool flag = false;
                        try
                        {
                            var end = endSEPayload.End + samples.Count();
                            var up = Builders<SEPayload>.Update.PushEach("Samples", samples).Set("End", end);
                            var endfilter = Builders<SEPayload>.Filter.Eq("Id", endSEPayload.Id);
                            await collectionSEPayload.UpdateOneAsync(endfilter, up);
                            SESampleAppendResult res = new SESampleAppendResult(endSEPayload.Id, samples.Count());
                            listResult.Add(res);
                            flag = false;
                        }
                        catch (MongoWriteException)
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            await CreateNewPayloadAsync<T>(signalId, dim, samples, listResult, endSEPayload.End + 1);
                        }
                    }
                }
            }
            return listResult;
        }
        */
        private async Task CreateNewPayloadAsync<T>(Guid signalId, List<long> dim, List<T> samples, List<SESampleAppendResult> listResult, long start = 0)
        {

            Stack<List<T>> stack = new Stack<List<T>>();
            stack.Push(samples);
            long startIndex = start;
            while (stack.Count() > 0)  
            {
                List<T> write = stack.Pop();
                var SEPayload1 = new SEPayload<T>();
                SEPayload1.ParentId = signalId;
                SEPayload1.Dimensions = dim;
                SEPayload1.Start = startIndex;
                long end = startIndex + write.Count() - 1;
                SEPayload1.End = end;
                SEPayload1.Samples.AddRange(write);
                try
                {
                    await collectionSEPayload.InsertOneAsync(SEPayload1);
                    startIndex = end;
                    SESampleAppendResult res = new SESampleAppendResult(SEPayload1.Id, write.Count());
                    listResult.Add(res);
                }
                catch (MongoWriteException)
                {
                    Int32 indexcount = write.Count() / 2;
                    var later = write.GetRange(0, indexcount);
                    write.RemoveRange(0, indexcount);
                    stack.Push(write);
                    stack.Push(later);
                }
            }
        }
        
        public async Task<IEnumerable<Guid>> GetPayloadIdsByParentIdAsync(Guid parentId)
        {
            var projection = Builders<SEPayload>.Projection.Include(m => m.Id);
            var filter = Builders<SEPayload>.Filter.Eq("ParentId", parentId);
            //如果要排序，限制数量 这里加
            var options = new FindOptions<SEPayload, BsonDocument> { Projection = projection };
            using (var cursor = await collectionSEPayload.FindAsync(filter, options))
            {
                var list = cursor.ToListAsync().Result;
                List<Guid> listGuid = new List<Guid>();
                foreach (var item in list)
                {
                    listGuid.Add(item["_id"].AsGuid);
                }
                //如果没找到，返回长度为0的空list
                return listGuid;
            }
        }

        /// <summary>
        /// return the dimensions of the signal
        /// </summary>
        /// <param name="signalId">signal id, only applis to signal</param>
        /// <returns>the dimension of the signal, hi rank dimension comes in first in the list</returns>
        public async Task<List<long>> GetDimentionsAsync(Guid signalId)
        {
            //var filter = Builders<SEPayload>.Filter.Eq("ParentId", signalId);
            //var cursor = await collectionSEPayload.FindAsync(filter);
            //await cursor.MoveNextAsync();
            //var tm = (SEPayload)cursor.Current.FirstOrDefault();
            //return tm.Dimensions;
             var builder = Builders<SEPayload>.Filter;
            var filter = new List<FilterDefinition<SEPayload>>();
            filter.Add(builder.Eq("ParentId", signalId));
            filter.Add(builder.Eq("Dimensions", ""));
            var sot = Builders<SEPayload>.Sort.Descending("End");
            var options = new FindOptions<SEPayload, SEPayload> { Sort = sot, Limit = 1 };
            using (var cursor = await collectionSEPayload.FindAsync(builder.And(filter), options))
            {
                await cursor.MoveNextAsync();
        SEPayload endSEPayload = (SEPayload)cursor.Current.FirstOrDefault();
            }
            return null;
        }
        
        /// <summary>
        /// return the size of a singal or a SEPayload,
        /// if used on a signal, it returns the combine size of all its SEPayload,size is in byte
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<long> GetSizeAsync(Guid id)
        {
            //get the size of SEPayload
            var filter = Builders<SEPayload>.Filter.Eq("Id", id);
            var cursor = await collectionSEPayload.FindAsync(filter);
            await cursor.MoveNextAsync();
            var tm = cursor.Current.FirstOrDefault();
            return tm.ToBson().LongLength;
        }

        /// <summary>
        /// Copy sourceSignal data to targetSignal
        /// </summary>
        /// <param name="sourceSignalId"></param>
        /// <param name="targetSignalId"></param>
        /// <returns></returns>
        public async Task CopyDataAsync(Guid sourceSignalId, Guid targetSignalId)
        {
            var filter = Builders<SEPayload>.Filter.Eq("ParentId", sourceSignalId);
            using (var cursor = await collectionSEPayload.FindAsync(filter))
            {
                var batch = await cursor.ToListAsync();
                for (int i = 0; i < batch.Count(); i++)
                {
                    var item = batch[i];
                    item.Id = Guid.NewGuid();
                    item.ParentId = targetSignalId;
                    await collectionSEPayload.InsertOneAsync(item);
                }
            }
        }

        public async Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, long start, long end, bool createNewSEPayload = false)
        {
            throw new NotImplementedException();
            
        }

        public async Task<IWriter<T>> GetWriter<T>(Guid signalId)
        {
            throw new NotImplementedException();
        }

        public IWriter<T> GetWriter<T>(JDBCEntity signal)
        {
            throw new NotImplementedException();
        }

       
    }
}
