using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.MongoStorageEngine;
using Jtext103.JDBC.MongoStorageEngine.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Jtext103.JDBC.Core.StorageEngineInterface;

namespace StorageEngineTest
{
    [TestClass]
    public class SampleRWTest
    {
        /*
        private static CoreService myCoreService;
        private static IMatrixStorageEngineInterface mongoEngine = new MongoEngine();
        #region test helpers

        private static void clearDb()
        {
            myCoreService.ClearDb();
            mongoEngine.ClearDb();
        }

        private static double[, ,] generateDummyData()
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


        #endregion

        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            //todo Liu Qiang, add proper ctor
            myCoreService = CoreService.GetInstance();
            myCoreService.Init("JDBC-test");
            mongoEngine.Init("host =mongodb://127.0.0.1:27017 & database = JDBC-test & collection = Payload");
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            clearDb();
        }
        //tofo get append size dimension

        [TestMethod]
        public void AppendAndGetSampleTest()
        {
            //arrange
            //myCoreService = new CoreService("JDBC");
            //myCoreService.ClearDb();
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
                    //     myCoreService.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }

            //act
            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;
            //   var payloadIds = myCoreService.GetPayloadIdsByParentIdAsync(sig11.Id).Result;
            //   var randomPayload = (Payload<double>)myCoreService.GetPayloadByIdAsync<double>(payloadIds.FirstOrDefault()).Result;
            var randomPayload = (SEPayload<double>)mongoEngine.GetPayloadByIdAsync<double>(payloadIds.FirstOrDefault()).Result;
            //     var randomPayload = (Payload<double>)myCoreService.GetPayloadByIdAsync<double>(payloadIds.SingleOrDefault()).Result;
            //assert append
            Assert.AreEqual(64, payloadIds.Count());
            Assert.AreEqual(8, randomPayload.Samples.Count);
            Assert.AreEqual(0, randomPayload.Start);
            Assert.AreEqual(7, randomPayload.End);
            Assert.AreEqual(2, randomPayload.Dimensions.Count);
            //get samples
            //expected data:
            //[213,214,215
            //223,224,225]
            //-----------
            //[313,314,315
            //323,324,325]
            //flatted!!
            var samples = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 1, 3 },
                new List<long> { 2, 2, 3 }).Result.ToList();
            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[2, 2, 3]
            { 
                { { 213,214,215 }, { 223,224,225 } }, 
                { { 313,314,315 }, { 323,324,325 } } 
            };
            var flat = expectedSample.Cast<double>().ToList();
            CollectionAssert.AreEqual(flat, samples);


        }


        [TestMethod]
        public void AppendSampleTwiceTest()
        {
            //this will append to 8*8*16
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
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }
            //act
            //append more makes it 8*8*16
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


            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;
            var randomPayload = mongoEngine.GetPayloadByIdAsync<double>(payloadIds.FirstOrDefault()).Result;
            //get samples

            var samples = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 1, 6 },
                new List<long> { 2, 2, 4 }).Result.ToList();

            //assert append
            Assert.AreEqual(64, payloadIds.Count());
            Assert.AreEqual(16, randomPayload.Samples.Count);
            Assert.AreEqual(0, randomPayload.Start);
            Assert.AreEqual(15, randomPayload.End);
            Assert.AreEqual(2, randomPayload.Dimensions.Count);
            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[2, 2, 4]
            { 
                { { 216,217,210,211 }, { 226,227,220,221 } }, 
                { { 316,317,310,311 }, { 326,327,320,321 } } 
            };
            var flat = expectedSample.Cast<double>().ToList();
            CollectionAssert.AreEqual(flat, samples);
        }


        [TestMethod]
        public void AppendSampleExtendTest()
        {
            //this will create a 8*8*8
            //then add a line to the 3,4 surfave, make it 8*9*8,but only the 3,4 surface have the 9th line
            //add the 2 line on 5 surface to 16 element make it 8*9*16,but only the 2 line on 2 surface have the 9th line
            //read non-extist reange will throw error
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
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }
            //act
            //use your imagation to get the test data
            var oneDim2 = new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 };
            //one line to 3 surface
            var line9sur3 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { 2, 8 }, oneDim2).Result;
            //one line to 4 surface
            var line9sur4 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { 3, 8 }, oneDim2).Result;
            //make line 2 in surface 5 16 element
            var line2sur5 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { 4, 1 }, oneDim2).Result;

            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;

            //get samples
            //error
            var errorNotData = "";
            try
            {
                var samples1 = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 7, 3 },
                     new List<long> { 3, 2, 4 }).Result.ToList();
            }
            catch (Exception e)
            {
                errorNotData = e.InnerException.Message;
            }
            var samples2 = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 7, 3 },
                     new List<long> { 2, 2, 3 }).Result.ToList();
            var samples3 = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 4, 1, 6 },
                    new List<long> { 1, 1, 4 }).Result.ToList();

            //assert append
            Assert.AreEqual(64 + 2, payloadIds.Count());

            //Assert  get
            Assert.AreEqual(ErrorMessages.SampleNotExistsError, errorNotData);

            //make a flatter array for assert
            var expectedSample2 = new double[2, 2, 3]
            { 
                { { 273,274,275 }, { 3,4,5 } }, 
                { { 373,374,375 }, { 3,4,5 } } 
            };
            var flat2 = expectedSample2.Cast<double>().ToList();
            CollectionAssert.AreEqual(flat2, samples2);

            //sample3
            var flat3 = new List<double> { 416, 417, 0, 1 };
            CollectionAssert.AreEqual(flat3, samples3);

            //check the payload details
            Assert.AreEqual(1, line9sur3.Count);
            Assert.AreEqual(8, line9sur3.Single().SampleCount);
            Assert.AreEqual(8, line9sur4.Single().SampleCount); //modified by liuqiang :rejected
            var pL9sur3 = mongoEngine.GetPayloadByIdAsync<double>(line9sur3.Single().PayloadId).Result;
            Assert.AreEqual(8, pL9sur3.Samples.Count);
            Assert.AreEqual(0, pL9sur3.Start);
            Assert.AreEqual(7, pL9sur3.End);
            //    CollectionAssert.AreEqual(new List<long>{3,8}, pL9sur3.Dimensions); modified by liuqiang :approved
            CollectionAssert.AreEqual(new List<long> { 2, 8 }, pL9sur3.Dimensions);
            //check the long line payload
            Assert.AreEqual(1, line9sur3.Count); //modified by liuqiang :rejected
            Assert.AreEqual(1, line9sur4.Count);
            Assert.AreEqual(8, line9sur4.Single().SampleCount);
            var pL2sur5 = mongoEngine.GetPayloadByIdAsync<double>(line2sur5.Single().PayloadId).Result;
            Assert.AreEqual(16, pL2sur5.Samples.Count);
            Assert.AreEqual(0, pL2sur5.Start);
            Assert.AreEqual(15, pL2sur5.End);
            CollectionAssert.AreEqual(new List<long> { 4, 1 }, pL2sur5.Dimensions);
            //    CollectionAssert.AreEqual(new List<long> { 5, 2 }, pL2sur5.Dimensions);  modified by liuqiang :approved
        }


        [TestMethod]
        public void AppendNewPayloadTest()
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
                    mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { i, j }, oneDim).Wait();
                }
            }

            //act
            //append require new payload to be created
            var oneDim2 = new List<double> { 8, 9, 10 };
            var oneDim3 = new List<double> { 0, 1, 2 };
            //append line2 of 2 surface
            var appendResult2 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { 2, 2 }, oneDim2).Result;
            //create new payload
            var appendResult3 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long> { 2, 2 }, oneDim3, true).Result;
            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;
            var pl221 = mongoEngine.GetPayloadByIdAsync<double>(appendResult2.SingleOrDefault().PayloadId).Result;
            var pl222 = mongoEngine.GetPayloadByIdAsync<double>(appendResult3.SingleOrDefault().PayloadId).Result;
            //get samples

            var samples = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 2, 2, 7 },
                new List<long> { 1, 1, 6 }).Result.ToList();

            //assert append
            Assert.AreEqual(65, payloadIds.Count());
            Assert.AreEqual(11, pl221.Samples.Count);
            Assert.AreEqual(0, pl221.Start);
            Assert.AreEqual(10, pl221.End);
            //the new payload
            Assert.AreEqual(3, pl222.Samples.Count);
            Assert.AreEqual(11, pl222.Start);
            Assert.AreEqual(13, pl222.End);

            //asser append result
            Assert.AreEqual(3, appendResult2.Single().SampleCount);
            Assert.AreEqual(3, appendResult3.Single().SampleCount);
            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[]
            { 
                227,8,9,10,0,1
            };
            CollectionAssert.AreEqual(expectedSample, samples);
        }

        [TestMethod]
        public void AppendOneDimentionTest()
        {
            //arrange
            var exp1 = new Experiment("exp1");
            var sig11 = new Signal("sig1-1");
            myCoreService.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            myCoreService.AddJdbcEntityToAsync(exp1.Id, sig11).Wait();

            //act
            var oneDim = new List<double> { 0, 1, 2 };
            //append line2 of 2 surface
            //the dim is given null so one dim
            var appendResult = mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim, true).Result;
            var appendResult2 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim, true).Result;
            mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim, true).Wait();
            var appendResult3 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim).Result;
            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;
            var pl1 = mongoEngine.GetPayloadByIdAsync<double>(appendResult.Single().PayloadId).Result;
            var pl2 = mongoEngine.GetPayloadByIdAsync<double>(appendResult2.Single().PayloadId).Result;

            //get samples
            var samples = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 1 },
                new List<long> { 4 }).Result.ToList();
            var samplesInLastPayload = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 8 },
                new List<long> { 3 }).Result.ToList();
            var samplesCrossLastPayload = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 5 },
                new List<long> { 3 }).Result.ToList();

            //assert append
            Assert.AreEqual(3, payloadIds.Count());
            //   Assert.AreEqual(2, pl1.Samples.Count);  modified by liuqiang :approved
            Assert.AreEqual(3, pl1.Samples.Count);
            Assert.AreEqual(0, pl1.Start);
            Assert.AreEqual(2, pl1.End);
            Assert.AreEqual(0, pl1.Dimensions.Count);
            Assert.AreEqual(3, pl2.Start);
            Assert.AreEqual(5, pl2.End);
            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[]
            { 
                1,2,0,1
            };
            var expectedSample2 = new double[]
            { 
                2,0,1
            };
            CollectionAssert.AreEqual(expectedSample, samples);
            CollectionAssert.AreEqual(expectedSample2, samplesInLastPayload);
            CollectionAssert.AreEqual(expectedSample2, samplesCrossLastPayload);
            //assert result test
            var idFromResult = new List<Guid>();
            idFromResult.AddRange(appendResult.Select(r => r.PayloadId).ToList());
            idFromResult.AddRange(appendResult2.Select(r => r.PayloadId).ToList());
            idFromResult.AddRange(appendResult3.Select(r => r.PayloadId).ToList());
            CollectionAssert.AreEquivalent(payloadIds.ToList(), idFromResult);


        }


        [TestMethod]
        public void DecimationTest()
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
            var randomPayload = (SEPayload<double>)mongoEngine.GetPayloadByIdAsync<double>(payloadIds.FirstOrDefault()).Result;
            //get samples
            var samples = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 5, 6 },
                new List<long> { 4, 4 }, new List<long> { 5, 2 }).Result.ToList();

            //assert append
            Assert.AreEqual(100, payloadIds.Count());
            Assert.AreEqual(100, randomPayload.Samples.Count);
            Assert.AreEqual(0, randomPayload.Start);
            Assert.AreEqual(99, randomPayload.End);
            Assert.AreEqual(1, randomPayload.Dimensions.Count);
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
            List<SESampleAppendResult> result1 = null, result2 = null;
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
                    result1 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim).Result;
                }
                else
                {
                    result2 = mongoEngine.AppendSampleAsync(sig11.Id, new List<long>(), oneDim).Result;
                }

            }


            //get payload numbers for later assert
            var payloadIds = mongoEngine.GetPayloadIdsByParentIdAsync(sig11.Id).Result;

            //get samples
            var samples = mongoEngine.GetSamplesAsync<double>(sig11.Id, new List<long> { 2099997 },
                new List<long> { 3 }).Result.ToList();

            //assert append,split into 2 payloads
            Assert.AreEqual(2, payloadIds.Count());

            //Assert  get
            //make a flatter array for assert
            var expectedSample = new double[]
            { 
                2099997,2099998,2099999
            };
            CollectionAssert.AreEqual(expectedSample, samples);
            //assert result test
            var idFromResult = new List<Guid>();
            idFromResult.AddRange(result1.Select(r => r.PayloadId).ToList());
            idFromResult.AddRange(result2.Select(r => r.PayloadId).ToList());
            CollectionAssert.AreEquivalent(payloadIds.ToList(), idFromResult);//modified by liuqiang


        }*/
    }
}
