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


namespace StorageEngineTest
{
    /// <summary>
    /// StorageEngine的读写测试
    /// </summary>
    [TestClass]
    public class StorageEngineUnitTest
    {
        private static CoreService myCoreService;
        private static IMatrixStorageEngineInterface storageEngine = new CassandraEngine();//MongoEngine(); //CassandraEngine();
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
            storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest");

            myCoreService.StorageEngine = storageEngine;

            //   storageEngine.Init("host =115.156.252.12 & database = jdbc_unittest");
            //storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"
            //myCoreService.myStorageEngine = storageEngine;

            //  myCoreService.Init("JDBC-test", (IStorageEngine)myStorageEngine);
            //  Debug.Write(1);
        }
        [TestCleanup]
        public void MyTestCleanup()
        {
            clearDb();
        }
        private static double[, ,] generateDummyData1()
        {
            var result = new double[10,10, 10];
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

        /// <summary>
        /// 读取二维数据并测试
        /// </summary>
        [TestMethod]
        public void ReadData2DTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            var data = generateDummyData();
            int n = 0, m = 0;
            //append signal
            for (int i = 0; i < 20; i++)
            {
                m = n + 5;
                var oneDim = new List<double>(5);
                for (; n < m; n++)
                {
                    oneDim.Add(n);
                }
                storageEngine.AppendSampleAsync(sig11.Id, new List<long> { 0 }, oneDim).Wait();
            }
            n = m = 0;
            for (int i = 0; i < 20; i++)
            {
                m = n + 5;
                var oneDim = new List<double>(5);
                for (; n < m; n++)
                {
                    oneDim.Add(n);
                }
                storageEngine.AppendSampleAsync(sig11.Id, new List<long> { 1 }, oneDim).Wait();
            }

            double[] assertresult1 = new double[50];
            double[] assertresult21 = new double[60];
            double[] assertresult22 = new double[40];
            for (int i = 46, j = 0; j < 50; )
            {
                assertresult1[j] = i;
                assertresult21[j++] = i++;
            }
            for (int i = 46, j = 50; j < 60;)
            {
                assertresult21[j++] = i++;
            }
            for (int i = 56, j = 0; j < 40;)
            {
                assertresult22[j++] = i++;
            }
            // 1  读取测试
            var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0,46 }, new List<long> { 2,60 }).Result;
            List<double> result1 = cursor1.Read(50).Result.ToList();
            CollectionAssert.AreEqual(assertresult1, result1);

            // 2  跨维度测试
            var cursor2 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0, 46 }, new List<long> { 2, 50 }).Result;
            List<double> result21= cursor2.Read(60).Result.ToList();
            CollectionAssert.AreEqual(assertresult21, result21);

            List<double> result122 = cursor2.Read(50).Result.ToList();
            CollectionAssert.AreEqual(assertresult22, result122);

            // 3  边界测试
            double[] assertresult31 = new double[60];
            for (int i = 40, j = 0; j < 60;)
            {
                assertresult31[j++] = i++;
            }
            var cursor3 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0, 40 }, new List<long> { 2, 60 }).Result;
            List<double> result31 = cursor3.Read(60).Result.ToList();
            CollectionAssert.AreEqual(assertresult31, result31);

            List<double> result32 = cursor3.Read(60).Result.ToList();
            CollectionAssert.AreEqual(assertresult31, result32);

            // 4  读取测试，带factor
            double[] assertresult41 = new double[5] { 5,10,15,20,25};
            var cursor4 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0, 5 }, new List<long> { 2, 10 },new List<long> { 1,5}).Result;
            List<double> result41 = cursor4.Read(5).Result.ToList();
            CollectionAssert.AreEqual(assertresult41, result41);

        }
        /// <summary>
        /// 测试一维数据
        /// </summary>
        [TestMethod]
        public void ReadData1DTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
          //  var oneDim = new List<double>(8);
            int n=0,m=0;
            //append signal
            for (int i = 0; i < 20; i++)
            {
                m = n+5;
                var oneDim = new List<double>(5);
                for (; n <m ; n++)
                {
                    oneDim.Add(n);
                }
                storageEngine.AppendSampleAsync(sig11.Id, new List<long> { }, oneDim).Wait();
            }

            // 1  读取测试
            double[] assertresult1 = new double[10];
            for (int i = 46, j = 0; j < 10;)
            {
                assertresult1[j++] = i++;
            }
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0 }, new List<long> { 1000 }).Result;
           // var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 46 }, new List<long> { 60 }).Result;
            List<double> result1 = cursor.Read(1000).Result.ToList();
            CollectionAssert.AreEqual(assertresult1, result1);

            for (int i = 56, j = 0; j < 10; )
            {
                assertresult1[j++] = i++;
            }
            CollectionAssert.AreEqual(assertresult1, result1);

            result1 = cursor.Read(30).Result.ToList();

            // 2  超出边界测试
            var errorMessage = "";
            try
            {
                List<double> result2 = cursor.Read(10).Result.ToList();
            }
            catch (Exception e)
            {
                errorMessage = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.OutOfRangeError,errorMessage);

            // 3 读取测试，跨维度，factor
            double[] assertresult11 = new double[5] { 46,56,66,76,86};
            var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 46 }, new List<long> { 5 },new List <long> { 10 }).Result;
            List<double> result11 = cursor1.Read(10).Result.ToList();
            CollectionAssert.AreEqual(assertresult11, result11);
        }

        /// <summary>
        /// 读出全部数据
        /// </summary>
        [TestMethod]
        public void ReadData3DTest2()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            var data = generateDummyData();

            for (int n = 0; n < 20; n++)
            {
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
                        storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
                    }
                }  
            }
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result; 

            List<double> result = cursor.Read(3).Result.ToList();
            double[] assertresult = { 213, 214, 215 };
            CollectionAssert.AreEqual(assertresult, result);
            List<double> result3 = cursor.Read(4).Result.ToList();
            double[] assertresult3 = { 223, 224, 225, 313 };
            CollectionAssert.AreEqual(assertresult3, result3);

            List<double> result4 = cursor.Read(4).Result.ToList();
            double[] assertresult4 = { 314, 315, 323, 324 };
            CollectionAssert.AreEqual(assertresult4, result4);

            List<double> result5 = cursor.Read(5).Result.ToList();
            double[] assertresult5 = { 325 };
            CollectionAssert.AreEqual(assertresult5, result5);

            var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result1 = cursor1.Read(2).Result.ToList();
            double[] assertresult1 = { 213, 214 };
            CollectionAssert.AreEqual(assertresult1, result1);

            List<double> result2 = cursor1.Read(2).Result.ToList();
            double[] assertresult2 = { 215, 223 };
            CollectionAssert.AreEqual(assertresult2, result2);

            var cursor2 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 5 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result22 = cursor2.Read(3).Result.ToList();
            double[] assertresult22 = { 215, 216, 217 };
            CollectionAssert.AreEqual(assertresult22, result22);
            List<double> result33 = cursor2.Read(4).Result.ToList();
            double[] assertresult33 = { 225, 226, 227, 315 };
            CollectionAssert.AreEqual(assertresult33, result33);
        }
      

        /// <summary>
        /// 读取三维数据测试
        /// </summary>
        [TestMethod]
        //Use Cursor.Read()
        public void ReadData3DTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            var data = generateDummyData1();
            
            for (int n = 0; n < 10; n++)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        var oneDim = new List<double>(10);
                        for (int k = 0; k < 10; k++)
                        {
                            oneDim.Add(data[i, j, k]);
                        }
                        //notice that this has only 2 interations for 3 dimention arry
                        storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
                    }
                }
            }
            // 1 测试读取跨维度的数据，并使用factor
            var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }, new List<long> { 2, 2, 2 }).Result;
            List<double> result11 = cursor1.Read(3).Result.ToList();
            double[] assertresult11 = { 213, 215, 217 };
            CollectionAssert.AreEqual(assertresult11, result11);
            List<double> result12 = cursor1.Read(4).Result.ToList();
            double[] assertresult12 = { 233, 235, 237, 413 };
            CollectionAssert.AreEqual(assertresult12, result12);

            var cursor2 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result21 = cursor2.Read(2).Result.ToList();
            double[] assertresult21 = { 213, 216 };
            CollectionAssert.AreEqual(assertresult21, result21);
            List<double> result22 = cursor2.Read(3).Result.ToList();
            double[] assertresult22 = { 219, 233, 236};
            CollectionAssert.AreEqual(assertresult22, result22);

            var cursor3 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 10 }, new List<long> { 2, 2, 6 }).Result;
            List<double> result31 = cursor3.Read(3).Result.ToList();
            double[] assertresult31 = { 213, 219, 215 };
            CollectionAssert.AreEqual(assertresult31, result31);
            List<double> result32 = cursor3.Read(4).Result.ToList();
            double[] assertresult32 = { 211, 217, 213, 219 };
            CollectionAssert.AreEqual(assertresult32, result32);
            List<double> result33 = cursor3.Read(4).Result.ToList();
            double[] assertresult33 = { 215, 211, 217, 233 };
            CollectionAssert.AreEqual(assertresult33, result33);

            // 2 边界测试，并使用factor
            var cursor4 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 2 }, new List<long> { 2, 2, 6 }, new List<long> { 2, 2, 2 }).Result;
            List<double> result41 = cursor4.Read(5).Result.ToList();
            double[] assertresult41 = { 212, 214, 216,218,210 };
            CollectionAssert.AreEqual(assertresult41, result41);
            List<double> result42 = cursor4.Read(5).Result.ToList();
            double[] assertresult42 = { 212, 232, 234, 236,238 };
            CollectionAssert.AreEqual(assertresult42, result42);

            // 3 测试读取跨维度的数据，不使用factor
            var cursor5 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 8}, new List<long> { 2, 2, 5 }).Result;
            List<double> result51 = cursor5.Read(3).Result.ToList();
            double[] assertresult51 = { 218, 219, 210 };
            CollectionAssert.AreEqual(assertresult51, result51);
            List<double> result52 = cursor5.Read(4).Result.ToList();
            double[] assertresult52 = { 211, 212, 228, 229 };
            CollectionAssert.AreEqual(assertresult52, result52);
        }

        /// <summary>
        /// 一维数据
        /// </summary>
        [TestMethod]
        public void AppendData1DTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            int number = 10;//210000;
            for (int i = 0; i < number; i++)
            {
                var oneDim = new List<double>(number);
                for (double k = 0; k < number; k++)
                {
                    oneDim.Add(i * number + k);
                }
                //todo optimize, use array as input
                storageEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim).Wait();
            }
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 15 }, new List<long> { 10 }).Result;
            var samples = cursor.Read(30).Result.ToList();
            var expectedSample = new double[] {15,16,17,18,19,20,21,22,23,24 };
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
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            //append signal 100*100
            for (int i = 0; i < 100; i++)
            {
                var oneDim = new List<double>(100);
                for (double k = 0; k < 100; k++)
                {
                    oneDim.Add(i + k / 100);
                }
                //notice that this has only 2 interations for 3 dimention arry
                storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i }, oneDim).Wait();
            }

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
                    storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }
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

        /// <summary>
        /// 暂时不测
        /// </summary>
        [TestMethod]
        public void AppendBigDataTest()
        {
            //this test take very long do not run this if not necessary
            //arrange
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();

            //act
            //append signal one dimentions very long many times
            //10*210000*8bytes, bigger than a document can be
            for (int i = 0; i < 10; i++)
            {
                var oneDim = new List<double>(210000);
                for (double k = 0; k < 210000; k++)
                {
                    oneDim.Add(i * 210000 + k);
                }
                long start = 210000 * i;
                long end = (i + 1) * 210000 - 1;
                storageEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim, false).Wait();
            }
            //get samples
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 209997 },new List<long> { 3 }).Result;
            var samples = cursor.Read(5).Result.ToList();
            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[]
            {
                209997,209998,209999
            };
            CollectionAssert.AreEqual(expectedSample, samples);
        }

        /// <summary>
        /// 测试删除数据
        /// </summary>
        [TestMethod]
        public void DeleteTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var exp11 = new Experiment("exp1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig111 = new Signal("sig1-1-1");
    
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();

            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, sig111).Wait();
            var data = generateDummyData();
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
                    storageEngine.AppendSampleAsync(sig111.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }
            myCoreService.DuplicateAsync(exp11.Id, exp1.Id, "exp1-1-c", true,true).Wait();
            //old tree
            /*
             /exp1/exp11/sig111/payload1-1-1-1
                  /exp12/sig121  
                  /exp11c/sig111/payload-1-1-1
             */
            //act

            //var cursor = storageEngine.GetCursorAsync<double>(sig111.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 2 }).Result;
            //List<double> result = cursor.Read(3).Result.ToList();
            //double[] assertresult = { 213, 214, 223 };
            //CollectionAssert.AreEqual(assertresult, result);

            myCoreService.DeleteAsync(exp11.Id,true).Wait();

            var exp11Result=myCoreService.GetChildByNameAsync(exp1.Id,"exp1-1").Result;
            Assert.AreEqual(null, exp11Result);

            var sig111Duplicate = myCoreService.GetOneByPathAsync("/exp1/exp1-1-c/sig1-1-1").Result;
            var cursor = storageEngine.GetCursorAsync<double>(sig111Duplicate.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 2 }).Result;
            List<double> result = cursor.Read(3).Result.ToList();
            double[] assertresult = { 213, 214, 223 };
            CollectionAssert.AreEqual(assertresult, result);

            storageEngine.DeleteDataAsync(sig111Duplicate.Id);
            try
            {
                var cursor1 = storageEngine.GetCursorAsync<double>(sig111Duplicate.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
                List<double> result1 = cursor1.Read(3).Result.ToList();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.InnerException.Message,ErrorMessages.OutOfRangeError);
            }
        }

        //复制信号节点测试
        [TestMethod]
        public void DuplicateDataTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp111 = new TestExp("exp1-1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig111 = new TestSignal("sig1-1-1");

            exp111.TestString = "test";
            exp111.TestList = new List<string> { "1", "1", "1" };
            sig111.TestString = "test";
            sig111.TestList = new List<string> { "1", "1", "1" };

            //add 2 root
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp2).Wait();

            //add child
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp11).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, exp12).Wait();
            myCoreService.AddJdbcEntityToAsync(exp12.Id, sig121).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, sig111).Wait();
            myCoreService.AddJdbcEntityToAsync(exp11.Id, exp111).Wait();

            var data = generateDummyData();
            //append signal
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
                    storageEngine.AppendSampleAsync(sig111.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }
            // 1 新的节点不能和原来的子节点同名
            var errorSameName = "";
            try
            {
                myCoreService.DuplicateAsync(exp11.Id, exp1.Id, exp11.Name).Wait();
            }
            catch (Exception e)
            {
                errorSameName = e.InnerException.Message;
            }
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameName);
            //old tree                              new tree
            /*
             /exp1/exp11/sig111/payload64       /exp1/exp11/sig111/payload64
                        /exp111                            /exp111
                  /exp12/sig121                      /exp11c/sig111
             /exp2                                          /exp111
                                                     /exp12/sig121  
                                                /exp2/exp12
             */

            //2 get for assert /exp1/exp11c，递归拷贝节点测试.
            myCoreService.DuplicateAsync(exp11.Id, exp1.Id, "exp1-1-c", true).Wait();
            myCoreService.DuplicateAsync(exp12.Id, exp2.Id, exp12.Name).Wait();
           
            var childrenNameOfExp1 = myCoreService.GetAllChildrenAsync(exp1.Id).Result.Select(e => e.Name);
            CollectionAssert.AreEquivalent(new List<string> { "exp1-1-c", "exp1-1", "exp1-2" }, childrenNameOfExp1.ToList());

            var exp11c = myCoreService.GetChildByNameAsync(exp1.Id, "exp1-1-c").Result;
            var exp111c = myCoreService.GetChildByNameAsync(exp11c.Id, "exp1-1-1").Result as TestExp;
            Assert.AreEqual(exp111.TestString, exp111c.TestString);
            Assert.AreNotEqual(exp111.Id, exp111c.Id);               //   exp1/exp11/exp111   exp1/exp11c/exp111
            Assert.AreNotEqual(exp111.ParentId, exp111c.ParentId);    //  /exp1/exp11/
            CollectionAssert.AreEqual(exp111.TestList, exp111c.TestList);
          

            var sig111c = myCoreService.GetChildByNameAsync(exp11c.Id, "sig1-1-1").Result as TestSignal;
            Assert.AreEqual(sig111.TestString, sig111c.TestString);
            CollectionAssert.AreEqual(sig111.TestList, sig111c.TestList);

            var childOfExp2 = myCoreService.GetAllChildrenAsync(exp2.Id).Result.Single();
            Assert.AreEqual("/exp2/exp1-2",childOfExp2.Path);
            /*
            /exp1/exp11/sig111/payload64       /exp1/exp11/sig111/payload64
                       /exp111                            /exp111
                 /exp12/sig121                      /exp11c/sig111
            /exp2                                          /exp111
                                                    /exp12/sig121  
                                               /exp2/exp12
                                                    /exp11/sig111/payload64
                                                          /exp111
            */
            //3 get for assert /exp2/exp11，递归拷贝节点和数据测试.
            myCoreService.DuplicateAsync(exp11.Id, exp2.Id, exp11.Name, true, true).Wait();
            var childrenOfExp2 = myCoreService.GetAllChildrenAsync(exp2.Id).Result;
            Assert.AreEqual(2,childrenOfExp2.Count());

           var sig111New= myCoreService.GetOneByPathAsync("/exp2/exp1-1/sig1-1-1").Result;
           var cursor = storageEngine.GetCursorAsync<double>(sig111New.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result = cursor.Read(3).Result.ToList();
            double[] assertresult = { 213, 214, 215 };
            CollectionAssert.AreEqual(assertresult, result);     
        }


        //  [TestMethod]
        //public void AppendSampleExtendTest()
        //{
        //    this will create a 8 * 8 * 8
        //    then add a line to the 3,4 surfave, make it 8 * 9 * 8,but only the 3,4 surface have the 9th line
        //    add the 2 line on 5 surface to 16 element make it 8 * 9 * 16,but only the 2 line on 2 surface have the 9th line
        //    read non-extist reange will throw error
        //    arrange
        //    var exp1 = new Experiment("exp1");
        //    var sig11 = new Signal("sig1-1");
        //    myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
        //    myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
        //    var data = generateDummyData();
        //    //append signal
        //    for (int i = 0; i < 8; i++)
        //    {
        //        for (int j = 0; j < 8; j++)
        //        {
        //            var oneDim = new List<double>(8);
        //            for (int k = 0; k < 8; k++)
        //            {
        //                oneDim.Add(data[i, j, k]);
        //            }

        //            //notice that this has only 2 interations for 3 dimention arry
        //            myCoreService.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
        //        }
        //    }
        //    //act
        //    //use your imagation to get the test data
        //    var oneDim2 = new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 };
        //    //one line to 3 surface
        //    var line9sur3 = myCoreService.AppendSampleAsync(sig11.Id, new List<long> { 2, 8 }, oneDim2).Result;
        //    //one line to 4 surface
        //    var line9sur4 = myCoreService.AppendSampleAsync(sig11.Id, new List<long> { 3, 8 }, oneDim2).Result;
        //    //make line 2 in surface 5 16 element
        //    var line2sur5 = myCoreService.AppendSampleAsync(sig11.Id, new List<long> { 4, 1 }, oneDim2).Result;

        //    //get payload numbers for later assert
        //    var payloadIds = myCoreService.GetPayloadIdsByParentIdAsync(sig11.Id).Result;

        //    //get samples
        //    //error
        //    var errorNotData = "";
        //    try
        //    {
        //        var samples1 = myCoreService.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 7, 3 },
        //             new List<long> { 3, 2, 4 }).Result.ToList();
        //    }
        //    catch (Exception e)
        //    {
        //        errorNotData = e.InnerException.Message;
        //    }
        //    var samples2 = myCoreService.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 7, 3 },
        //             new List<long> { 2, 2, 3 }).Result.ToList();
        //    var samples3 = myCoreService.GetSamplesAsync<double>(sig11.Id, new List<long> { 4, 1, 6 },
        //            new List<long> { 1, 1, 4 }).Result.ToList();

        //    //assert append
        //    Assert.AreEqual(64 + 2, payloadIds.Count());

        //    //Assert  get
        //    Assert.AreEqual(ErrorMessages.SampleNotExistsError, errorNotData);

        //    //make a flatter array for assert
        //    var expectedSample2 = new double[2, 2, 3]
        //    {
        //                { { 273,274,275 }, { 3,4,5 } },
        //                { { 373,374,375 }, { 3,4,5 } }
        //    };
        //    var flat2 = expectedSample2.Cast<double>().ToList();
        //    CollectionAssert.AreEqual(flat2, samples2);

        //    //sample3
        //    var flat3 = new List<double> { 416, 417, 0, 1 };
        //    CollectionAssert.AreEqual(flat3, samples3);

        //    //check the payload details
        //    Assert.AreEqual(1, line9sur3.Count);
        //    Assert.AreEqual(8, line9sur3.Single().SampleCount);
        //    Assert.AreEqual(8, line9sur4.Single().SampleCount); //modified by liuqiang :rejected
        //    var pL9sur3 = myCoreService.GetPayloadByIdAsync<double>(line9sur3.Single().PayloadId).Result;
        //    Assert.AreEqual(8, pL9sur3.Samples.Count);
        //    Assert.AreEqual(0, pL9sur3.Start);
        //    Assert.AreEqual(7, pL9sur3.End);
        //    //    CollectionAssert.AreEqual(new List<long>{3,8}, pL9sur3.Dimensions); modified by liuqiang :approved
        //    CollectionAssert.AreEqual(new List<long> { 2, 8 }, pL9sur3.Dimensions);
        //    //check the long line payload
        //    Assert.AreEqual(1, line9sur3.Count); //modified by liuqiang :rejected
        //    Assert.AreEqual(1, line9sur4.Count);
        //    Assert.AreEqual(8, line9sur4.Single().SampleCount);
        //    var pL2sur5 = myCoreService.GetPayloadByIdAsync<double>(line2sur5.Single().PayloadId).Result;
        //    Assert.AreEqual(16, pL2sur5.Samples.Count);
        //    Assert.AreEqual(0, pL2sur5.Start);
        //    Assert.AreEqual(15, pL2sur5.End);
        //    CollectionAssert.AreEqual(new List<long> { 4, 1 }, pL2sur5.Dimensions);
        //    CollectionAssert.AreEqual(new List<long> { 5, 2 }, pL2sur5.Dimensions); modified by liuqiang: approved
        //}

        //[TestMethod]
        //public void JDBCDimensionTest()
        //{
        //    var exp1 = new Experiment("exp1");
        //    var sig11 = new Signal("sig1-1");
        //    myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
        //    myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
        //    var data = generateDummyData1();

        //    for (int n = 0; n < 2; n++)
        //    {
        //        for (int i = 9; i > 0; i--)
        //        {
        //            for (int j = 0; j < 10; j++)
        //            {
        //                var oneDim = new List<double>(8);
        //                for (int k = 0; k < 10; k++)
        //                {
        //                    oneDim.Add(data[i, j, k]);
        //                }
        //                //notice that this has only 2 interations for 3 dimention arry
        //                storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
        //            }
        //        }
        //    }
        //    var dim = storageEngine.GetDimentionsAsync(sig11.Id).Result;

        //}
    }


}
