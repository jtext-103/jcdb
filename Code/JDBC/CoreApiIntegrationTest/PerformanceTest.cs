using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtext103.JDBC.Core.Api;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using BasicPlugins;
using BasicPlugins.TypedSignal;
using Jtext103.JDBC.JdbcCassandraIndexEngine;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace CoreApiIntegrationTest
{
    [TestClass]
    public class PerformanceTest
    {
        private static CoreApi myCoreApi;

        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            var myStorageEngine = new CassandraIndexEngine();//CassandraIndexEngine();//CassandraEngine();
            myStorageEngine.Init("host =localhost & database = jdbc_unittest"); //127.0.0.1
            myCoreApi = CoreApi.GetInstance();     //JDBC-test jdbc_liuweb
            myCoreApi.CoreService.Init("mongodb://127.0.0.1:27017", "jdbc_liuweb", "Experiment", (IStorageEngine)myStorageEngine);
            myCoreApi.CoreService.RegisterClassMap<FixedIntervalWaveSignal>();
        }

        [TestInitialize]
        public void Setup()
        {
            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.CoreService);
            myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);
           
        }

        [TestCleanup]
        public  async void Cleanup()
        {
       //    await  myCoreApi.clearDb(true);
        }
        [TestMethod]
        public void  Clean()
        {
             myCoreApi.clearDb(true);
        }
        public static double[] rand(long size)
        {
            double[] value = new double[size];
            Random ran = new Random();
            for (int i = 0; i < value.LongLength; i++)
            {
                value[i] = ran.NextDouble();
            }
            return value;
        }

        static double[] value = rand(500000);
        static JDBCEntity exp1 = new Experiment("exp1");
        [TestMethod]
        public  async Task MulThreadPerformance()
        {
            string filepath = "e:\\Record.txt";
            FileStream fs = new FileStream(filepath, FileMode.Append);
            StreamWriter writer = new StreamWriter(fs);

            await myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1);
            int j = 0;
            int threadnum = 10;
            int count = j + threadnum;
            Thread[] threads = new Thread[threadnum];
            for (; j < count; j++)
            {
                Thread t = new Thread(new ThreadStart(putData));
                t.Name = j.ToString();
                threads[j] = t;
            }
            for (int i = 0; i < threadnum; i++)
                threads[i].Start();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < threadnum; i++)
                threads[i].Join();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            writer.WriteLine(DateTime.Now + ":" + threadnum + "个线程" + " :" + ts.TotalMilliseconds.ToString());
            writer.Close();
            fs.Close();
        }
      
        public async void putData()
        {
            string name = "ws" + Thread.CurrentThread.Name;
          //  string name = "ws" + j;
            Debug.WriteLine(name);
            var wavesig = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", name, @"StartTime=0&SampleInterval=0.00001");
            wavesig.SampleInterval = 500000;
            await myCoreApi.AddOneToExperimentAsync(exp1.Id, wavesig);
            for (int i = 0; i < 10; i++)
            {
                await wavesig.PutDataAsync("", value);
            }
            await wavesig.DisposeAsync();
        }
        [TestMethod]
        public async Task CoreAPIPerformance()

        {
            string filepath = "e:\\Record.txt";
            FileStream fs = new FileStream(filepath, FileMode.Append);
            StreamWriter writer = new StreamWriter(fs);
            JDBCEntity exp1 = new Experiment("exp1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            var a= myCoreApi.FindOneByPathAsync("/exp1/ws1").Result;
            double[] value = rand(500000);
            int num = 1;
            int size = 10;
            Stopwatch sw = Stopwatch.StartNew();
            //Parallel.For(0, 10, async i =>
            // {
            //     string name = "ws" + i;
            //     var wavesig = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", name, @"StartTime=0&SampleInterval=0.00001");
            //     wavesig.NumberOfSamples = 500000;
            //     await myCoreApi.AddOneToExperimentAsync(exp1.Id, wavesig);
            //     for (int j = 0; j < size; j++)
            //     {
            //         await wavesig.PutDataAsync("", value);
            //     }
            //     await wavesig.DisposeAsync();
            // });
            var tasks = new Task[num];
            for (int i = 0; i < num; i++)
            {
                tasks[i] = Task.Factory.StartNew(async statement =>
                {
                    string name = "ws" + statement;
                    Debug.WriteLine(name);
                    var wavesig = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", name, @"StartTime=0&SampleInterval=0.00001");
                    wavesig.NumberOfSamples = 500000;
                    await myCoreApi.AddOneToExperimentAsync(exp1.Id, wavesig);
                    for (int j = 0; j < size; j++)
                    {
                        await wavesig.PutDataAsync("", value);
                    }
                    await wavesig.DisposeAsync();
                }, i);
            }
            Task.WaitAll(tasks);
          
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            writer.WriteLine(DateTime.Now + ":" + num+"个通道" + " :" + ts.TotalMilliseconds.ToString()+"  "+size+"*500K");
            writer.Close();
            fs.Close();
            Thread.Sleep(5000);
        }
        [TestMethod]
        public void WebAPIPerformance()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            GetData("1003065");
            GetData("1003066");
            Task.Run(() =>
            { 
                //for (int i = 1003065; i < 1003080; i++)
                //{
                //    GetData(i.ToString());
                //}
            });
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string filepath = "e:\\Record.txt";
            FileStream fs = new FileStream(filepath, FileMode.Append);
            StreamWriter writer = new StreamWriter(fs);

            writer.WriteLine(DateTime.Now + ":" + "WEB 2个通道" + " :" + ts.TotalMilliseconds.ToString());
            writer.Close();
            fs.Close();
        }
        public void GetData(string signal)
        {
            string urlHead = "http://localhost:12441/data/";
            string query = "path/jtext/1045961/CH" + signal + "?format=complex&__count=300"; //1003065
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlHead + query);
            request.Method = "GET";
            request.ContentType = "text/json;charset=UTF-8";
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("http://localhost:12441/"), new Cookie("user", "root"));
            request.CookieContainer = cookieContainer;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
                Debug.WriteLine(result);
            }
        }
        [TestMethod]
        public void AddEntity()
        {
           // IEnumerable<JDBCEntity> parent1 =myCoreApi.FindAllChildrenByParentIdAsync(Guid.Empty).Result;
            JDBCEntity parent= myCoreApi.FindOneByPathAsync("/jtext").Result;
            for (int i = 0; i < 500; i++)
            {
                JDBCEntity exp = new Experiment("exp"+i);
                myCoreApi.AddOneToExperimentAsync(parent.Id, exp).Wait();
            }
           
        }
      
    }
   
}
