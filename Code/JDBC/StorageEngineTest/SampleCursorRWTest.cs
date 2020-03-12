using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using Jtext103.JDBC.MongoStorageEngine;
using Jtext103.JDBC.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace StorageEngineTest
{
    [TestClass]
    public class SampleCursorRWTest
    {
        private static CoreService myCoreService;
        private static IMatrixStorageEngineInterface mongoEngine = new MongoEngine();
        private static void clearDb()
        {
            myCoreService.ClearDb();
            mongoEngine.ClearDb();
        }
        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            Debug.Write(1);
            //myCoreService = new CoreService("JDBC-test");
            //mongoEngine.Init("host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload");
            //myCoreService.istorageEngine = mongoEngine;
        }
        [TestInitialize]
        public void Setup()
        {
            myCoreService = CoreService.GetInstance();
            mongoEngine.Init("host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload");
            myCoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment", mongoEngine);
            //mongoEngine.ClearDb();
        }
        [TestCleanup]
        public void MyTestCleanup()
        {
            clearDb();
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
        public void JDBCCursorSingleDimReadTest()
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
                mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { }, oneDim).Wait();
            }
            var cursor = mongoEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 46 }, new List<long> { 60 }).Result;

            List<double> result1 = cursor.Read(10).Result.ToList();
            //   double[] assertresult1 = { 4, 5, 6,7,8 };
            double[] assertresult1 = new double[10];
            for (int i = 46, j = 0; j < 10; )
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
            List<double> result2 = cursor.Read(10).Result.ToList();
            double[] assertresult2 = { 96, 97, 98, 99,0,0,0,0,0,0 };
            //double[] assertresult2 = {9, 10, 11, 12,13, 14, 15, 16, 17, 18 };
            CollectionAssert.AreEqual(assertresult2, result2);
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
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }
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
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim, true).Wait();
                }
            }
            var cursor = mongoEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
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


            var cursor1 = mongoEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
            List<double> result1 = cursor1.Read(2).Result.ToList();
            double[] assertresult1 = { 213, 214 };
            CollectionAssert.AreEqual(assertresult1, result1);

            List<double> result2 = cursor1.Read(2).Result.ToList();
            double[] assertresult2 = { 215, 223 };
            CollectionAssert.AreEqual(assertresult2, result2);
        }

        [TestMethod]
        public void CursorAppendAndGetSampleTest()
        {
            //arrange
            myCoreService = CoreService.GetInstance();
            myCoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment");
            myCoreService.ClearDb();
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
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
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
            var cursor = mongoEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2, 1, 3 },
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
                mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i }, oneDim).Wait();
            }

            //act
            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;
       //     var randomPayload = (SEPayload<double>)mongoEngine.GetPayloadByIdAsync<double>(payloadIds.FirstOrDefault()).Result;
            //get samples
            var cursor = mongoEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 5, 6 },
                new List<long> { 4, 4 }, new List<long> { 5, 2 }).Result;
            var samples = cursor.Read(100).Result.ToList();

            //assert append
            Assert.AreEqual(100, payloadIds.Count());
            //Assert.AreEqual(100, randomPayload.Samples.Count);
            //Assert.AreEqual(0, randomPayload.Start);
            //Assert.AreEqual(99, randomPayload.End);
            //Assert.AreEqual(1, randomPayload.Dimensions.Count);
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
                if (i == 0)
                {
                    //todo optimize, use array as input
                     mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim);
                }
                else
                {
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim);
                }

            }
            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;

            //get samples
            var cursor = mongoEngine.GetCursorAsync<double>(sig11.Id, new List<long> { 2099997 },
                new List<long> { 3 }).Result;
            var samples = cursor.Read(5).Result.ToList();

            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[]
            {
                2099997,2099998,2099999
            };
            CollectionAssert.AreEqual(expectedSample, samples);
            //assert result test
            var idFromResult = new List<Guid>();
            //idFromResult.AddRange(result1.Select(r => r.PayloadId).ToList());
            //idFromResult.AddRange(result2.Select(r => r.PayloadId).ToList());
            CollectionAssert.AreEquivalent(payloadIds.ToList(), idFromResult);//modified by liuqiang
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
                    mongoEngine.AppendSampleAsync(sig111.Id, new List<long> { i, j }, oneDim).Wait();
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
            Assert.AreEqual(ErrorMessages.EntityNotFoundError, errorDeleteNonExist);
            CollectionAssert.AreEquivalent(new List<Guid> { exp1.Id }, root.Select(r => r.Id).ToList());
            Assert.IsNull(nSig121);
            Assert.AreEqual(Guid.Empty, nExp12);

            var payloads=mongoEngine.GetPayloadIdsByParentIdAsync(sig111.Id).Result;
            Assert.AreEqual(64,payloads.Count());
            myCoreService.DeleteAsync(sig111.Id,true).Wait();
            var payloadZero = mongoEngine.GetPayloadIdsByParentIdAsync(sig111.Id).Result;
            Assert.AreEqual(0, payloadZero.Count());
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
                    mongoEngine.AppendSampleAsync(sig111.Id, new List<long> { i, j }, oneDim).Wait();
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
           var cursor = mongoEngine.GetCursorAsync<double>(sig111New.Id, new List<long> { 2, 1, 3 }, new List<long> { 2, 2, 3 }).Result;
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
    public class TestExp : Experiment
    {
        public TestExp(string name) : base(name)
        {
            TestList = new List<string>();
        }

        public string TestString { get; set; }
        public List<string> TestList { get; set; }
    }
    public class TestSignal : Signal
    {
        public string TestString { get; set; }
        public List<string> TestList { get; set; }

        public TestSignal(string name) : base(name)
        {
            TestList = new List<string>();
        }
    }
}
