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
    [TestClass]
    public class StorageEngineRWTest
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
            storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"

            myCoreService = CoreService.GetInstance();
            myCoreService.Init("JDBC-test", storageEngine);

            //storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //"host =127.0.0.1 & database = jdbc_unittest "//"host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload"
            //myCoreService.istorageEngine = storageEngine;
          
        }
        [TestInitialize]
        public void Setup()
        {
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
        [TestMethod]
        //Use Cursor.Read()
        public void JDBCCursorTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            var data = generateDummyData();
            //  var oneDim = new List<double>(8);
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
            for (int i = 46, j = 0; j < 50; )
            {
                assertresult1[j++] = i++;
            }

            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0,46 }, new List<long> { 2,60 }).Result;
            List<double> result1 = cursor.Read(50).Result.ToList();
            CollectionAssert.AreEqual(assertresult1, result1);
            //var errorGetData = "";
            //try
            //{
            //    List<double> result1 = cursor.Read(5).Result.ToList();
            //}
            //catch (Exception e)
            //{
            //    errorGetData = e.InnerException.Message;
            //}
            //Assert.AreEqual(ErrorMessages.OutOfRangeError,errorGetData);

            var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 0, 46 }, new List<long> { 2, 50 }).Result;
            List<double> result2 = cursor1.Read(50).Result.ToList();
            CollectionAssert.AreEqual(assertresult1, result2);

            List<double> result3 = cursor1.Read(50).Result.ToList();
            CollectionAssert.AreEqual(assertresult1, result3);
        }
        [TestMethod]
        //Use Cursor.Read()
        public void JDBCCursorSingleDimReadTest()
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
           var cursor= storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 46}, new List<long> { 60 }).Result;

           List<double> result1= cursor.Read(10).Result.ToList();
            double[] assertresult1=new double[10];
            for (int i = 46,j=0; j < 10; )
            {
                assertresult1[j++] = i++;
            }
            CollectionAssert.AreEqual(assertresult1, result1);
            result1 = cursor.Read(10).Result.ToList();
            for (int i = 56, j = 0; j < 10; )
            {
                assertresult1[j++] = i++;
            }
            CollectionAssert.AreEqual(assertresult1, result1);
            result1 = cursor.Read(30).Result.ToList();
            //  result1 = cursor.Read(10).Result.ToList();
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
            
            //double[] assertresult2 = { 96, 97, 98, 99, 0, 0, 0, 0, 0, 0 };
           // CollectionAssert.AreEqual(assertresult2, result2);

        }
        [TestMethod]
        //Use Cursor.Read()
        public void JDBCCursorMulDimReadTest()
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

            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result; //, new List<long> { 2,2,2}
            //   cursor.readCursor();

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
        [TestMethod]
        public void JDBCDimensionTest()
        {
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();
            var data = generateDummyData1();

            //for (int n = 0; n < 10; n++)
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        for (int j = 0; j < 10; j++)
            //        {
            //            var oneDim = new List<double>(8);
            //            for (int k = 0; k < 10; k++)
            //            {
            //                oneDim.Add(data[i, j, k]);
            //            }
            //            //notice that this has only 2 interations for 3 dimention arry
            //            storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
            //        }
            //    }
            //}
            for (int n = 0; n < 2; n++)
            {
            for (int i = 9; i >0; i--)
            {
                for (int j = 0; j < 10; j++)
                {
                    var oneDim = new List<double>(8);
                    for (int k = 0; k < 10; k++)
                    {
                        oneDim.Add(data[i, j, k]);
                    }
                    //notice that this has only 2 interations for 3 dimention arry
                    storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
                }
            }
            }
           var dim=  storageEngine.GetDimentionsAsync(sig11.Id).Result;
      //     CollectionAssert.AreEqual(dim, new List<long> { 2, 2, 3 });
            //var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }, new List<long> { 2, 2, 2 }).Result;
            //List<double> result = cursor.Read(3).Result.ToList();
            ////double[] assertresult = { 215, 216, 217 };
            //double[] assertresult = { 213, 215, 217 };
            //CollectionAssert.AreEqual(assertresult, result);
            //List<double> result3 = cursor.Read(4).Result.ToList();
            //double[] assertresult3 = { 233, 235, 237, 413 };
            //CollectionAssert.AreEqual(assertresult3, result3);

            //var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }, new List<long> { 2, 2, 3 }).Result;
            //List<double> result1 = cursor1.Read(2).Result.ToList();
            //double[] assertresult1 = { 213, 216 };
            //CollectionAssert.AreEqual(assertresult1, result1);
            //List<double> result33 = cursor1.Read(3).Result.ToList();
            //double[] assertresult33 = { 219, 233, 236 };
            //CollectionAssert.AreEqual(assertresult33, result33);
        }
        [TestMethod]
        //Use Cursor.Read()
        public void JDBCCursorFactorReadTest()
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
                        var oneDim = new List<double>(8);
                        for (int k = 0; k < 10; k++)
                        {
                            oneDim.Add(data[i, j, k]);
                        }
                        //notice that this has only 2 interations for 3 dimention arry
                        storageEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
                    }
                }
            }
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }, new List<long> { 2, 2, 2 }).Result;
            List<double> result = cursor.Read(3).Result.ToList();
            //double[] assertresult = { 215, 216, 217 };
            double[] assertresult = { 213, 215, 217 };
            CollectionAssert.AreEqual(assertresult, result);
            List<double> result3 = cursor.Read(4).Result.ToList();
            double[] assertresult3 = { 233, 235, 237, 413 };
            CollectionAssert.AreEqual(assertresult3, result3);

            var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result1 = cursor1.Read(2).Result.ToList();
            double[] assertresult1 = { 213, 216 };
            CollectionAssert.AreEqual(assertresult1, result1);
            List<double> result33 = cursor1.Read(3).Result.ToList();
            double[] assertresult33 = { 219, 233, 236};
            CollectionAssert.AreEqual(assertresult33, result33);

            var cursor2 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 10 }, new List<long> { 2, 2, 6 }).Result;
            List<double> result2 = cursor2.Read(3).Result.ToList();
            double[] assertresult2 = { 213, 219, 215 };
            CollectionAssert.AreEqual(assertresult2, result2);
            List<double> result23 = cursor2.Read(4).Result.ToList();
            double[] assertresult23 = { 211, 217, 213, 219 };
            CollectionAssert.AreEqual(assertresult23, result23);
            List<double> result4 = cursor2.Read(4).Result.ToList();
            double[] assertresult4 = { 215, 211, 217, 233 };
            CollectionAssert.AreEqual(assertresult4, result4);


            //List<double> result5 = cursor.Read(5).Result.ToList();
            //double[] assertresult5 = { 325 };
            //CollectionAssert.AreEqual(assertresult5, result5);


            //var cursor1 = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
            //List<double> result1 = cursor1.Read(2).Result.ToList();
            //double[] assertresult1 = { 213, 214 };
            //CollectionAssert.AreEqual(assertresult1, result1);

            //List<double> result2 = cursor1.Read(2).Result.ToList();
            //double[] assertresult2 = { 215, 223 };
            //CollectionAssert.AreEqual(assertresult2, result2);
        }
        [TestMethod]
        public void CursorAppendAndGetSampleTest()
        {
            //arrange
            //myCoreService = CoreService.GetInstance();
            //myCoreService.Init("JDBC");

         //   myCoreService.ClearDb();
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
        [TestMethod]
        public void CursorDecimationTest()
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
        [TestMethod]
        public void QueryDataTest()
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
           storageEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim);
            }
           // storageEngine.ge
            var cursor = storageEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 15 }, new List<long> { 10 }).Result;

        }
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


        [TestMethod]
        public void DeleteTest()
        {
            //arrange
            //arrange
            var exp1 = new Experiment("exp1");
            var exp2 = new Experiment("exp2");
            var exp11 = new Experiment("exp1-1");
            var exp111 = new Experiment("exp1-1-1");
            var exp12 = new Experiment("exp1-2");
            var sig121 = new Signal("sig1-2-1");
            var sig111 = new Signal("sig1-1-1");
    
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
            //old tree
            /*
             /exp1/exp11/sig111/payload1-1-1-1
                        /exp111
                  /exp12/sig121  
             /exp2
             
             */

            //act
            //try delet exp12 error expetced
            var errorDelete = "";
            try
            {
                myCoreService.DeleteAsync(exp12.Id).Wait();
            }
            catch (Exception e)
            {
                errorDelete = e.InnerException.Message;
            }
            myCoreService.DeleteAsync(exp12.Id, true).Wait();
            myCoreService.DeleteAsync(exp2.Id).Wait();
            //the deleted entity is  non exist, error expected
            string errorDeleteNonExist = "";
            try
            {
                myCoreService.DeleteAsync(exp2.Id).Wait();
            }
            catch (Exception e)
            {
                //this is async call so exception is wrapped
                errorDeleteNonExist = e.InnerException.Message;
            }

            //get for assert
            var root = myCoreService.GetAllChildrenAsync().Result;
            var nSig121 = myCoreService.GetOneByIdAsync(sig121.Id).Result;
            var nExp12 = myCoreService.GetChildIdByNameAsync(exp1.Id, exp12.Name).Result;
            //assert 
            Assert.AreEqual(ErrorMessages.DeleteEntityWithChildError, errorDelete);
            Assert.AreEqual(ErrorMessages.DeleteEntityNotExitsError, errorDeleteNonExist);
            CollectionAssert.AreEquivalent(new List<Guid> { exp1.Id }, root.Select(r => r.Id).ToList());
            Assert.IsNull(nSig121);
            Assert.AreEqual(Guid.Empty, nExp12);

        //    var payloads=storageEngine.GetPayloadIdsByParentIdAsync(sig111.Id).Result;
         //   Assert.AreEqual(64,payloads.Count());
            myCoreService.DeleteAsync(sig111.Id,true).Wait();
       //     var payloadZero = storageEngine.GetPayloadIdsByParentIdAsync(sig111.Id).Result;
       //     Assert.AreEqual(0, payloadZero.Count());
        }

        //todo duplacate test
        [TestMethod]
        public void DuplicateTest()
        {
            //arrange
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
            var errorSameName = "";
            try
            {
                myCoreService.DuplicateAsync(exp11.Id, exp1.Id, exp11.Name).Wait();
            }
            catch (Exception e)
            {
                errorSameName = e.InnerException.Message;
            }
            //old tree                              new tree
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

            myCoreService.DuplicateAsync(exp11.Id, exp1.Id, "exp1-1-c", true).Wait();
            myCoreService.DuplicateAsync(exp12.Id, exp2.Id, exp12.Name).Wait();
           

            //get for assert
            var childrenNameOfExp1 = myCoreService.GetAllChildrenAsync(exp1.Id).Result.Select(e => e.Name);
            var exp11c = myCoreService.GetChildByNameAsync(exp1.Id, "exp1-1-c").Result;
            var exp111c = myCoreService.GetChildByNameAsync(exp11c.Id, "exp1-1-1").Result as TestExp;
            var sig111c = myCoreService.GetChildByNameAsync(exp11c.Id, "sig1-1-1").Result as TestSignal;
            var childOfExp2 = myCoreService.GetAllChildrenAsync(exp2.Id).Result.Single();

            myCoreService.DuplicateAsync(exp11.Id, exp2.Id, exp11.Name, true, true).Wait();

            var childrenOfExp2 = myCoreService.GetAllChildrenAsync(exp2.Id).Result;
            Assert.AreEqual(2,childrenOfExp2.Count());

           
           var sig111New= myCoreService.GetOneByPathAsync("/exp2/exp1-1/sig1-1-1").Result;
           var cursor = storageEngine.GetCursorAsync<double>(sig111New.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result = cursor.Read(3).Result.ToList();
            double[] assertresult = { 213, 214, 215 };
            CollectionAssert.AreEqual(assertresult, result);
            //assert 
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameName);

            CollectionAssert.AreEquivalent(new List<string> { "exp1-1-c", "exp1-1", "exp1-2" }, childrenNameOfExp1.ToList());
            Assert.AreEqual(exp111.TestString, exp111c.TestString);
            Assert.AreNotEqual(exp111.Id, exp111c.Id);
            Assert.AreNotEqual(exp111.ParentId, exp111c.ParentId);
            Assert.AreEqual(sig111.TestString, sig111c.TestString);
            CollectionAssert.AreEqual(exp111.TestList, exp111c.TestList);
            CollectionAssert.AreEqual(sig111.TestList, sig111c.TestList);
        //    Assert.AreEqual(0, childOfNewExp12.Count());
        }

        }
    
}
