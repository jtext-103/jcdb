using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Jtext103.JDBC.CassandraStorageEngine;
using Jtext103.JDBC.MongoStorageEngine;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Api;
using Jtext103.JDBC.Core.Interfaces;
using BasicPlugins.TypedSignal;
using System.Diagnostics;
using BasicPlugins;

namespace CassandraMongoDBTest
{
   public class Account
    {
        public static CoreApi MyCoreApi;
        private  static CoreService myCoreService;
        private  static IMatrixStorageEngineInterface storageEngine ;
        
        List<double> value;
        int datanum;
        int threadnum;
        int appendnum;
        int signalnum;
        int signalcount;
        string[] check = { "192.168.137.101:30000", "192.168.137.102:30000","192.168.137.103:30000","192.168.137.104:30000" };

        //internal Account(int number, int signalnum, int threadnum,int appendnum,bool cassandra=true)
        //{
        //    Random ran = new Random();
        //   value = new List<double>(number);
        //    for (int i = 0; i < number; i++)
        //    {
        //        value.Add( ran.NextDouble());
        //    }
            
        //    datanum = number;
        //    this.threadnum = threadnum;
        //    this.appendnum = appendnum;
        //    this.signalnum = signalnum;
        //    this.signalcount = signalnum / threadnum;

        //    myCoreService = CoreService.GetInstance();
        //    myCoreService.Init("JDBC-test");
        //    if (cassandra)
        //    {
        //        storageEngine = new CassandraEngine();
        //        storageEngine.Init("host =115.156.252.12 & database = demo");
        //    //    storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"
        //    }
        //    else
        //    {
        //        storageEngine = new MongoEngine();
        //        storageEngine.Init("host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload");
        //    }
        //    myCoreService.myStorageEngine = storageEngine;
        //}
        internal Account(int number, int signalnum, int threadnum, int appendnum, bool cassandra = true)
        {
            Random ran = new Random();
            value = new List<double>(number);
            for (int i = 0; i < number; i++)
            {
                value.Add(ran.NextDouble());
            }

            datanum = number;
            this.threadnum = threadnum;
            this.appendnum = appendnum;
            this.signalnum = signalnum;
            this.signalcount = signalnum / threadnum;

        //    myCoreService = CoreService.GetInstance();
            
            if (cassandra)
            {
                storageEngine = new CassandraEngine();
                storageEngine.Init("host =115.156.252.12 & database = demo1");
                //    storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"
            }
            else
            {
                storageEngine = new MongoEngine();
                storageEngine.Init("host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload");
            }
            myCoreService.Init("mongodb://127.0.0.1:27017","JDBC-test","Experiment",storageEngine);
        }

        internal  void Setup()
        {
           var node= myCoreService.GetChildByNameAsync(Guid.Empty,"exp1").Result;
           Debug.Write("");
           if (node==null)
           {
                var exp1 = new Experiment("exp1");
                myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
                for (int i = 0; i < 2; i++) //signalnum
                {
                    var sig11 = new Signal(i.ToString());//"sig1-1"
                    myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
                }
            }
           
        }
        public void clearDb()
        {
            myCoreService.ClearDb();
            storageEngine.ClearDb();
        }
        internal void QueryTransactions()
        {
            int threadid = Convert.ToInt16(Thread.CurrentThread.Name);
            //for (int i = 0; i < appendnum; i++)
            //{
            //    for (int j = 0; j < signalcount; j++)
            //    {
            //var index = threadid * signalcount + j;
            //string path = "/exp1/" + index.ToString();
            //var signal = myCoreService.getOneByPathAsync(path).Result;
            //storageEngine.GetDimentionsAsync(signal.Id);
            //    }
            //}
            //for (int i = 0; i < threadnum; i++)
            //{
                string path = "/exp1/" + "1";
                var signal = myCoreService.GetOneByPathAsync(path).Result;
                storageEngine.GetDimentionsAsync(signal.Id);
        //}
    }
        internal void QueryTransactions1()
        {
            int threadid = Convert.ToInt16(Thread.CurrentThread.Name);
            //var index = threadid * signalcount + 1;
            //string path = "/exp1/" + index.ToString();
            //var signal = myCoreService.getOneByPathAsync(path).Result;
            //storageEngine.GetSizeAsync(signal.Id);
            //for (int i = 0; i < threadnum; i++)
            //{
                string path = "/exp1/" + "1";
                 var signal = myCoreService.GetOneByPathAsync(path).Result;
                 storageEngine.GetSizeAsync(signal.Id);
            //}
        }
        internal void QueryTransactions2()
        {
            int threadid = Convert.ToInt16(Thread.CurrentThread.Name);
                string path = "/exp1/" + "1";
                var signal = myCoreService.GetOneByPathAsync(path).Result;
                storageEngine.GetSizeAsync(signal.Id);
        }
        internal void WebTransactions()
        {
            int threadid = Convert.ToInt16(Thread.CurrentThread.Name);
            string path = "/exp1/1" ;
            var signal = myCoreService.GetOneByPathAsync(path).Result;
            //JDBCEntity exp0 = new Experiment("exp0");
            //MyCoreApi.AddOneToExperimentAsync(Guid.Empty, exp0).Wait();
            ////normal data 
            //var waveSignal1 = (IntFixedIntervalWaveSignal)MyCoreApi.CreateSingal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            //MyCoreApi.AddOneToExperimentAsync(exp0.Id, waveSignal1).Wait();
            //int[] data1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //waveSignal1.PutDataAsync("", data1).Wait();
            //waveSignal1.PutDataAsync("", data1).Wait();
            //waveSignal1.PutDataAsync("", data1).Wait();
            ////big data
            //var waveSignal2 = (DoubleFixedIntervalWaveSignal)MyCoreApi.CreateSingal("FixedWave-double", "ws2", @"StartTime=-2&SampleInterval=0.00001");
            //MyCoreApi.AddOneToExperimentAsync(exp0.Id, waveSignal2).Wait();
            List<double> data2 = new List<double>();
            var rand = new Random();
            for (int i = 0; i < 20000; i++)
            {
                data2.Add(Math.Sin(i) + rand.NextDouble());
            }
            for (int i = 0; i < 200; i++)
            {
                storageEngine.AppendSampleAsync<double>(signal.Id, new List<long> { }, data2, true).Wait();
              //  waveSignal2.PutDataAsync("", data2).Wait();
            }
        }
        internal void MongoDBTransactions()
        {
            int threadid = Convert.ToInt16(Thread.CurrentThread.Name);
            
            for (int i = 0; i < appendnum; i++)
            {
                for (int j = 0; j < signalcount; j++)
                {
                    int start = DateTime.Now.Millisecond;
                    var index = threadid * signalcount + j;
                    string path = "/exp1/" + index.ToString();
                    var signal = myCoreService.GetOneByPathAsync(path).Result;
            
                    storageEngine.AppendSampleAsync(signal.Id, new List<long> { }, value,start,start*2,true).Wait();
                }
            }
        }
        internal void CassandraTransactions()
        {
            int threadid = Convert.ToInt16(Thread.CurrentThread.Name);
          //  Debug.WriteLine("thread:" + threadid);
            for (int i = 0; i < appendnum; i++)
            {
                for (int j = 0; j < signalcount; j++)
                {
                    int start = DateTime.Now.Millisecond;
                    var index = threadid * signalcount + j;
                    string path = "/exp1/" + index.ToString();
                    var signal = myCoreService.GetOneByPathAsync(path).Result;
                    storageEngine.AppendSampleAsync(signal.Id, new List<long>{}, value,start,start*2, true).Wait();
                }
            }
        }

        public static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
