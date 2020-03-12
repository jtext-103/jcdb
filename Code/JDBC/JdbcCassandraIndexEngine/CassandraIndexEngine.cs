using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;
using Jtext103.JDBC.Core.StorageEngineInterface;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.JdbcCassandraIndexEngine.Models;
using Jtext103.JDBC.Core.Services;
using ProtoBuf;
using System.IO;
using System.Diagnostics;

namespace Jtext103.JDBC.JdbcCassandraIndexEngine
{
    public class CassandraIndexEngine : IMatrixStorageEngineInterface
    {
        private int pagesize = 1000;
        private Cluster cluster;
        private ISession database;
        private ISession session;
        private IMapper mapper;
        private string databaseName = "";
        private CoreService myCoreService;

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
             "indexes bigint," +
             "samples blob," +
               "PRIMARY KEY (parentId,dimensions,indexes)" +
             ") WITH CLUSTERING ORDER BY (dimensions ASC,indexes ASC);");
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

            private void dispose()
        {
            cluster.Shutdown();
        }
        ~CassandraIndexEngine()
        {
          //  cluster.Shutdown();
          //  System.Diagnostics.Debug.WriteLine("MyClass.Dispose() was not called");
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
        public IWriter<T> GetWriter<T>(JDBCEntity signal)
        {
            Writer<T> jdbcWriter = new Writer<T>(signal, mapper);
            return jdbcWriter;
        }
    
        public Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, bool createNewSEPayload = false)
        {
            throw new NotImplementedException();
        }

        public Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, long start, long end, bool createNewSEPayload = false)
        {
            throw new NotImplementedException();
        }

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

        public async Task DeleteDataAsync(Guid signalId)
        {
            Cql cql = Cql.New("where parentid=? ", signalId);
            await mapper.DeleteAsync<SEPayload>(cql);
        }

        public async Task<ICursor<T>> GetCursorAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            myCoreService = CoreService.GetInstance();
            var signal =await myCoreService.GetOneByIdAsync(signalId);
            int sampleCount = (int)signal.NumberOfSamples;
            int indexStart = (int)start.LastOrDefault();
            int getNumber = (int)count.LastOrDefault();

            List<IPage<SEPayload>> listIPage = new List<IPage<SEPayload>>();
         
            if (decimationFactor == null)
            {
                decimationFactor = new List<long>();
                for (int i = 0; i < start.Count; i++)
                {
                    decimationFactor.Add(1);
                }
            }

            int factor = (int)decimationFactor[decimationFactor.Count() - 1];
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

            List<long> indexes = new List<long>();
            for (int line = 0; line < lineCount; line++)
            {
                #region
                Byte[] pagingState = null;
                string dimension = DimensionsToText(lineCursor);
                //计算index
                if (factor < sampleCount)
                {
                    var lastPoint = (factor * getNumber + indexStart) - factor;
                    var lastIndex = lastPoint / sampleCount+1;
                    var firstIndex = indexStart/sampleCount;
                    for (int i = firstIndex; i < lastIndex; i++)
                    {
                        indexes.Add(i);
                    }
                }
                else
                {
                    int coordinate = 0;
                    int variable = 0;
                    //每个点取一次比较
                    for (int i = 0; i < getNumber; i++)
                    {
                        coordinate = indexStart + i * factor;
                        variable = coordinate / sampleCount;
                        indexes.Add(variable);
                    }
                }
                //
               
                Cql cql = Cql.New("SELECT * FROM sepayload where parentid=? and dimensions=? and indexes in ? ", signalId, dimension,indexes);
                IPage<SEPayload> ipagePartialList = (await mapper.FetchPageAsync<SEPayload>(cql.WithOptions(opt => opt.SetPageSize(pagesize).SetPagingState(pagingState))));
                //  Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")+ "   getPAge");
                if (ipagePartialList.Count==0)
                {
                    throw new Exception(ErrorMessages.DataNotFoundError);
                }
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
            Cursor<T> jdbccursor= new Cursor<T>(listIPage, start.LastOrDefault(), count, dimPage, sampleCount, decimationFactor.LastOrDefault());
            return jdbccursor;
        }
        public async Task<ICursor<T>> GetCursorAsync<T>(Guid signalId)
        {
            myCoreService = CoreService.GetInstance();
            var signal = await myCoreService.GetOneByIdAsync(signalId);
            int sampleCount = (int)signal.NumberOfSamples;

            List<IPage<SEPayload>> listIPage = new List<IPage<SEPayload>>();
            List<long> dimPage = new List<long>();

            Byte[] pagingState = null;
            Cql cql = Cql.New("SELECT * FROM sepayload where parentid=?", signalId);
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

            Cursor<T> jdbccursor = new Cursor<T>(listIPage);
            return jdbccursor;
        }
        public Task<List<long>> GetDimentionsAsync(Guid signalId)
        {
            //cassandra统计查询特别慢，需要改进，如果使用这个函数
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Guid>> GetPayloadIdsByParentIdAsync(Guid parentId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetSamplesAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetSizeAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
