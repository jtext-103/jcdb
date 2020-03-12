using System;
using System.Threading.Tasks;
using System.Threading;
using BasicPlugins.TypedSignal;
using BasicPlugins;
using Jtext103.JDBC.Core.Api;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.JdbcCassandraIndexEngine;
using System.IO;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        private static CoreApi myCoreApi;
        static double[] value = rand(500000);
        static JDBCEntity exp1 = new Experiment("exp1");
        public void initial()
        {
            var myStorageEngine = new CassandraIndexEngine();//CassandraIndexEngine();//CassandraEngine();
            myStorageEngine.Init("host =192.168.137.153 & database = jdbc_unittest"); //127.0.0.1
            myCoreApi = CoreApi.GetInstance();
            myCoreApi.CoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment", (IStorageEngine)myStorageEngine);
            myCoreApi.CoreService.RegisterClassMap<FixedIntervalWaveSignal>();
            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.CoreService);
            myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);
        }
        public async Task clear()
        {
          await myCoreApi.clearDb(true);
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
        public static async void putData()
        {
            string name = "ws" + Thread.CurrentThread.Name;
            Debug.WriteLine(name);
            var wavesig = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", name, @"StartTime=0&SampleInterval=0.00001");
            wavesig.NumberOfSamples = 500000;
            await myCoreApi.AddOneToExperimentAsync(exp1.Id, wavesig);
            for (int i = 0; i < 10; i++)
            {
                await wavesig.PutDataAsync("", value);
            }
            await wavesig.DisposeAsync();
        }
        public async Task Threadtest()
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            writer.WriteLine(DateTime.Now + " " + DateTime.Now.Millisecond + ":start " + sw.ElapsedMilliseconds.ToString());
            for (int i = 0; i < threadnum; i++)
                threads[i].Start();
            for (int i = 0; i < threadnum; i++)
                threads[i].Join();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            writer.WriteLine(DateTime.Now + ":" + threadnum + "个线程" + " :" + ts.TotalMilliseconds.ToString() + "   10*50K");
            writer.Close();
            fs.Close();
        }
        public async Task Tasktest()
        {
            string filepath = "e:\\Record.txt";
            FileStream fs = new FileStream(filepath, FileMode.Append);
            StreamWriter writer = new StreamWriter(fs);
            await myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1);
        
            int threadnum = 10;
            var tasks = new Task[threadnum];
            Stopwatch sw = Stopwatch.StartNew();
           
            for (int i = 0; i < threadnum; i++)
            {
                tasks[i] = Task.Factory.StartNew(async statement =>
                {
                    string name = "ws" + statement;
                //    Debug.WriteLine(name);
                    var wavesig = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", name, @"StartTime=0&SampleInterval=0.00001");
                    wavesig.NumberOfSamples = 500000;
                    await myCoreApi.AddOneToExperimentAsync(exp1.Id, wavesig);
                    for (int j = 0; j < 10; j++)
                    {
                        if (j==0)
                        {
                            Debug.WriteLine(wavesig.Name + " started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
                        }
                        await wavesig.PutDataAsync("", value);
                    }
                    await wavesig.DisposeAsync();
                }, i);
            }
            Task.WaitAll(tasks);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            writer.WriteLine(DateTime.Now + ":" + threadnum + "个Task" + " :" + ts.TotalMilliseconds.ToString()+"   10*500K");
            writer.Close();
            fs.Close();
        }
        static void Main(string[] args)
        {
            Program pro = new Program();
            pro.initial();
            //  pro.Threadtest();
          pro.Tasktest().Wait();
            //pro.clear();
            Console.WriteLine("Finish");
            Console.Read();
        }
    }
}
