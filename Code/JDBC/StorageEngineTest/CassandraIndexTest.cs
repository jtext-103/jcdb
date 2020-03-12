using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using Jtext103.JDBC.MongoStorageEngine;
using Jtext103.JDBC.CassandraStorageEngine;
using Jtext103.JDBC.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Jtext103.JDBC.JdbcCassandraIndexEngine;
using System.Threading.Tasks;

namespace StorageEngineTest
{
    [TestClass]
    public class CassandraIndexTest
    {
        private static CoreService myCoreService;
        private static CassandraIndexEngine storageEngine = new CassandraIndexEngine();//MongoEngine(); //CassandraEngine();
        private static void clearDb()
        {
            myCoreService.ClearDb();
            storageEngine.ClearDb();
        }
        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            //   storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"
            myCoreService = CoreService.GetInstance();
            myCoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment");
            //   myCoreService.Init("JDBC-test", storageEngine);
        }
        [TestInitialize]
        public void Setup()
        {
            // Debug.Write(1);
            storageEngine.Init("host =127.0.0.1 & database = jdbc_indextest");

            myCoreService.StorageEngine = storageEngine;

            //   storageEngine.Init("host =115.156.252.12 & database = jdbc_unittest");
            //storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"
            //myCoreService.myStorageEngine = storageEngine;

            //  myCoreService.Init("JDBC-test", (IStorageEngine)myStorageEngine);
            //  Debug.Write(1);
        }
        private static double[,,] generateDummyData1()
        {
            var result = new double[10, 10, 10];
            for (int i = 0; i < 10; i++)
            {
                Debug.WriteLine("-----------------------");
                for (int j = 0; j < 10; j++)
                {
                    Debug.WriteLine("->");
                    for (int k = 0; k < 10; k++)
                    {
                        var elem = double.Parse(((i).ToString() + (j).ToString() + (k).ToString()));
                        result[i, j, k] = elem;
                        Debug.Write(elem);
                        Debug.Write(",");
                    }
                }
            }
            return result;
        }
        private static double[,,] generateDummyData()
        {
            //dummy data 8*8*8
            //[000,001,002,...007
            //010,011,012,....017
            //.....
            //070,071,072,...077]

            //[100,101,103,...107
            //110,111,012,....017
            //.....
            //170,171,072,...177]

            //...........

            //[700,701,003,...707
            //710,711,712,....717
            //.....
            //770,771,772,...777]
            var result = new double[8, 8, 8];
            for (int i = 0; i < 8; i++)
            {
                Debug.WriteLine("-----------------------");
                for (int j = 0; j < 8; j++)
                {
                    Debug.WriteLine("->");
                    for (int k = 0; k < 8; k++)
                    {
                        var elem = double.Parse(((i).ToString() + (j).ToString() + (k).ToString()));
                        result[i, j, k] = elem;
                        Debug.Write(elem);
                        Debug.Write(",");
                    }
                }
            }
            return result;
        }
        [TestCleanup]
        public void MyTestCleanup()
        {
            clearDb();
        }

        [TestMethod]
        public void AppendData1DTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            int number = 10;

            var writer = storageEngine.GetWriter<double>(sig11);
            for (int i = 0; i < number; i++)
            {
                var oneDim = new List<double>(number);
                for (double k = 0; k < number; k++)
                {
                    oneDim.Add(i * number + k);
                }
                //todo optimize, use array as input
                writer.AppendSampleAsync(new List<long>(), oneDim).Wait();
            }
            writer.DisposeAsync();
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 15 }, new List<long> { 10 }).Result;
            var samples = cursor.Read(30).Result.ToList();
            var expectedSample = new double[] { 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            var flat = expectedSample.Cast<double>().ToList();
            CollectionAssert.AreEqual(samples, expectedSample);
        }

        /// <summary>
        /// 二维数据
        /// </summary>
        [TestMethod]
        public void AppendData2DTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            sig11.NumberOfSamples = 100;
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            
            var writer = storageEngine.GetWriter<double>(sig11);
            for (int kj = 0; kj < 20; kj++)
            {
                for (int j = 0; j < 10; j++)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var oneDim = new List<double>(100);
                        for (double k = 0; k < 100; k++)
                        {
                            oneDim.Add(i + k / 100);
                        }
                        //notice that this has only 2 interations for 3 dimention arry
                        writer.AppendSampleAsync(new List<long> { i }, oneDim).Wait();
                    }
                }
            }
            writer.DisposeAsync();
            //get samples
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 5, 6 },
                new List<long> { 4, 4 }, new List<long> { 5, 2 }).Result;
            var samples = cursor.Read(100).Result.ToList();

            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[4, 4]
            {
                { 5.06,5.08,5.10,5.12 },
                { 10.06,10.08,10.10,10.12 },
                { 15.06,15.08,15.10,15.12 },
                { 20.06,20.08,20.10,20.12 }
            };
            var flat = expectedSample.Cast<double>().ToList();
            CollectionAssert.AreEqual(flat, samples);
        }


        /// <summary>
        /// 三维数据
        /// </summary>
        [TestMethod]
        public void AppendData3DTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            var data = generateDummyData();

            //append signal

            var writer = storageEngine.GetWriter<double>(sig11);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var oneDim = new List<double>(8);
                    for (int k = 0; k < 8; k++)
                    {
                        oneDim.Add(data[i, j, k]);
                    }
                    //notice that this has only 2 interations for 3 dimention arry
                    writer.AppendSampleAsync(new List<long> { i, j }, oneDim).Wait();
                }
            }
            writer.DisposeAsync();
            //get samples
            //expected data:
            //[213,214,215
            //223,224,225]
            //-----------
            //[313,314,315
            //323,324,325]
            //flatted!!

            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 },
                new List<long> { 2, 2, 3 }).Result;
            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[2, 2, 3]
            {
                { { 213,214,215 }, { 223,224,225 } },
                { { 313,314,315 }, { 323,324,325 } }
            };
            var sample = cursor.Read(15).Result.ToList();
            var flat = expectedSample.Cast<double>().ToList();
            CollectionAssert.AreEqual(flat, sample);
        }

    }
}

