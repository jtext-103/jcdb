using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Jtext103.JDBC.Core.StorageEngineInterface;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using ProtoBuf;
using System.IO;
using Jtext103.JDBC.CassandraStorageEngine.Models;


namespace Jtext103.JDBC.CassandraStorageEngine
{
    public class CassandraEngine : IMatrixStorageEngineInterface
    {
        private int pagesize = 1000;
        private Cluster cluster;
        private ISession database;
        private ISession session;
        private IMapper mapper;
        private string databaseName = "";
        public CassandraEngine() { }
        public void Init(string configString)
        {
            string[] split = configString.Split('&');
            string hostName = "";
            foreach (var item in split)
            {
                int startIndex = item.IndexOf("=");
                string head = item.Substring(0, startIndex).Trim();
                string name = item.Substring(startIndex + 1).Trim();
                switch (head)
                {
                    case "host": hostName = name; break;
                    case "database": databaseName = name; break;
                    default: throw new Exception(ErrorMessages.NotValidConfigDatabaseError);
                }
            }
            cluster = Cluster.Builder().AddContactPoint(hostName).Build();
            database = cluster.Connect();
            Dictionary<string, string> replication = new Dictionary<string, string>() { { "class", "SimpleStrategy" }, { "replication_factor", "1" } };
            database.CreateKeyspaceIfNotExists(databaseName, replication, true);
            // database.ChangeKeyspace("SEPayload");
            session = cluster.Connect(databaseName);
            session.Execute("CREATE TABLE IF NOT EXISTS " + databaseName + ".SEPayload (" +
             "parentid uuid," +
             "dimensions  text," +
             "start bigint," +
             "end bigint," +
             "samples blob," +
               "PRIMARY KEY (parentId,dimensions,start,end)" +
             ") WITH CLUSTERING ORDER BY (dimensions ASC,start ASC);");
          
            mapper = new Mapper(session);
        }
        public void Connect(string configString)
        {
            string[] split = configString.Split('&');
            string hostName = "";
            foreach (var item in split)
            {
                int startIndex = item.IndexOf("=");
                string head = item.Substring(0, startIndex).Trim();
                string name = item.Substring(startIndex + 1).Trim();
                switch (head)
                {
                    case "host": hostName = name; break;
                    case "database": databaseName = name; break;
                    default: throw new Exception(ErrorMessages.NotValidConfigDatabaseError);
                }
            }
            cluster = Cluster.Builder().AddContactPoint(hostName).Build();
            database = cluster.Connect();
            session = cluster.Connect(databaseName);
            mapper = new Mapper(session);
        }
        public void Close()
        {
            cluster.Shutdown();
        }


        public async Task ClearDb()
        {
            //string cql = string.Format("DROP table " + databaseName + ".sepayload;");
            //session.Execute(cql);
            Statement statement = new SimpleStatement("DROP table " + databaseName + ".sepayload;");
            await session.ExecuteAsync(statement);
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

        public async Task<ICursor<T>> GetCursorAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            List<IPage<SEPayload>> listIPage = new List<IPage<SEPayload>>();
            long queryStart= start.LastOrDefault();
            long queryEnd;
            if (decimationFactor == null)
            {
                decimationFactor = new List<long>();
                for (int i = 0; i < start.Count; i++)
                {
                    decimationFactor.Add(1);
                }
                queryEnd = queryStart+ count.LastOrDefault() ;
            }
            else
            {
                queryEnd = queryStart + decimationFactor.LastOrDefault() * (count.LastOrDefault() - 1);
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
            List<long> dimPage = new List<long>();   
            
            for (long line = 0; line < lineCount; line++)
            {
                #region
                Byte[] pagingState = null;
                string dimension = DimensionsToText(lineCursor);
            Cql cql = Cql.New("SELECT * FROM sepayload where parentid=? and dimensions=? and start<=? ", signalId, dimension, queryEnd);
           //     Cql cql = Cql.New("SELECT * FROM sepayload where parentid=? and dimensions=? ", signalId, dimension);
                IPage<SEPayload> ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql.WithOptions(opt => opt.SetPageSize(pagesize).SetPagingState(pagingState))));
                listIPage.Add(ipagePartialList);
                while (ipagePartialList.PagingState != null)
                {
                    pagingState = ipagePartialList.PagingState;
                    ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql.WithOptions(opt => opt.SetPageSize(pagesize).SetPagingState(pagingState))));
                    if (ipagePartialList.Count() > 0)
                    {
                        listIPage.Add(ipagePartialList);
                    }
                }
                long dimPageCount = listIPage.Count() - dimPage.Sum();
                dimPage.Add(dimPageCount);

                /***************************************/
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
            Cursor<T> jdbccursor = new Cursor<T>(listIPage, start.LastOrDefault(), count, dimPage, decimationFactor.LastOrDefault());
            return jdbccursor;
        }

        public Task<IEnumerable<T>> GetSamplesAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// append data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signalId"></param>
        /// <param name="dim"></param>
        /// <param name="samples"></param>
        /// <param name="createNewSEPayload"></param>
        /// <returns></returns>
        public async Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, bool createNewSEPayload = false)
         {
            //review cassandra的se是不是不管怎样都create new payload？     是的
            //存在Signal,找到子节点满足  父节点Id=signal，dimension=dim
            string dimension = DimensionsToText(dim);
            var ms = new MemoryStream();
            Serializer.Serialize(ms, samples);
            byte[] result = ms.ToArray();

            SEPayload lastPayload = (await mapper.FetchAsync<SEPayload>("SELECT * FROM sepayload where parentid=? and dimensions=?", signalId, dimension)).LastOrDefault();
            long start = 0, end = -1;
            //review todo 这里还是有读取
            if (lastPayload != null)
            {
                start = lastPayload.start;
                end = lastPayload.end;
            }
            SEPayload newPayload = new SEPayload();
            newPayload.parentid = signalId;
            newPayload.start = end + 1;
            newPayload.end = end + samples.Count();
            newPayload.samples = result;
            newPayload.dimensions = dimension;
            await mapper.InsertAsync<SEPayload>(newPayload);
        }

        public Task<IEnumerable<Guid>> GetPayloadIdsByParentIdAsync(Guid parentId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// test 
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        public async Task<List<long>> GetDimentionsAsync(Guid signalId)
        {
            var cursor = GetCursorAsync<double>(signalId, new List<long> { 70 }, new List<long> { 20 }).Result;
         //   List<double> result1 = cursor.Read(10).Result.ToList();
            return null;
            //List<long> dimentions = new List<long>();
            //List<IPage<SEPayload>> listIPage = new List<IPage<SEPayload>>();
            //Byte[] pagingState = null;
            //Cql cql = Cql.New("SELECT dimensions,start,end FROM sepayload where parentid=? ", signalId);
            //IPage<SEPayload> ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql.WithOptions(opt => opt.SetPageSize(pagesize).SetPagingState(pagingState))));
            //listIPage.Add(ipagePartialList);
            //while (ipagePartialList.PagingState != null)
            //{
            //    pagingState = ipagePartialList.PagingState;
            //    ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql.WithOptions(opt => opt.SetPageSize(pagesize).SetPagingState(pagingState))));
            //    if (ipagePartialList.Count() > 0)
            //    {
            //        listIPage.Add(ipagePartialList);
            //    }
            //}
            //Dictionary<string, long> dic = new Dictionary<string, long>();
            //for (int i = 0; i < listIPage.Count(); i++)
            //{
            //    List<SEPayload> partialList = new List<SEPayload>();
            //    IPage<SEPayload> iPage = listIPage[i];
            //    partialList = iPage.ToList();
            //    for (int j = 0; j < partialList.Count(); j++)
            //    {
            //        var payload = partialList[j];
            //        if (dic.ContainsKey(payload.dimensions))
            //        {
            //            dic[payload.dimensions] = dic[payload.dimensions] > payload.end ? dic[payload.dimensions] : payload.end;
            //        }
            //        else
            //        {
            //            dic.Add(payload.dimensions, payload.end);
            //        }
            //    }
            //}
            //for (int j = 0; j < dic.Count(); j++)
            //{
            //    dimentions.Add(dic.ElementAt(j).Value);
            //}
            //return dimentions;
        }

        public Task<long> GetSizeAsync(Guid id)
        {
            var cursor = GetCursorAsync<double>(id, new List<long> {19950 }, new List<long> { 90 },new List<long> { 40000}).Result;
            List<double> result1 = cursor.Read(80).Result.ToList();
            return null;
          //  throw new NotImplementedException();
        }

        /// <summary>
        /// Delete all payload of signal by signalid
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        public async Task DeleteDataAsync(Guid signalId)
        {
            Cql cql = Cql.New("where parentid=? ", signalId);
            await mapper.DeleteAsync<SEPayload>(cql);
        }

        /// <summary>
        /// Copy payload from sourceSignal
        /// </summary>
        /// <param name="sourceSignalId"></param>
        /// <param name="targetSignalId"></param>
        /// <returns></returns>
        public async Task CopyDataAsync(Guid sourceSignalId, Guid targetSignalId)
        {
            Byte[] pagingState = null;
            Cql cql = Cql.New("SELECT * FROM sepayload where parentid=? ", sourceSignalId);
            IPage<SEPayload> ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql));
            //IPage<SEPayload> ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql.WithOptions(opt => opt.SetPageSize(10).SetPagingState(pagingState))));
            pagingState = ipagePartialList.PagingState;
            List<SEPayload> list = ipagePartialList.ToList();
            for (int i = 0; i < list.Count(); i++)
            {
                var item = list[i];
                item.parentid = targetSignalId;
                mapper.Insert<SEPayload>(item);
            }
        }

        /// <summary>
        /// 自定义Start和End写入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signalId"></param>
        /// <param name="dim"></param>
        /// <param name="samples"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="createNewSEPayload"></param>
        /// <returns></returns>
        public async Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, long start, long end, bool createNewSEPayload = false)
        {
            //存在Signal,找到子节点满足  父节点Id=signal，dimension=dim
            string dimension = DimensionsToText(dim);

            var ms = new MemoryStream();
            List<Byte> by = new List<byte>();
            Serializer.Serialize(ms, samples);
            byte[] result = ms.ToArray();

            SEPayload newPayload = new SEPayload();
            newPayload.parentid = signalId;
            newPayload.start = start;
            newPayload.end = end;
            newPayload.samples = result;
            newPayload.dimensions = dimension;
            await mapper.InsertAsync<SEPayload>(newPayload);
        }

        public IWriter<T> GetWriter<T>(JDBCEntity signal)
        {
            throw new NotImplementedException();
        }
    }


}
