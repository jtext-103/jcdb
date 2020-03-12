using BasicPlugins;
using BasicPlugins.TypedSignal;
using Jtext103.JDBC.Core.Api;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.MongoStorageEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.JdbcCassandraIndexEngine;

namespace CoreApiIntegrationTest
{
    /// <summary>
    /// CoreAPI 单元测试
    /// </summary>
    [TestClass]
    public class ApiEntityUnitTest
    {
        //test basic parent child navigation
        //test create and delet node
        //test duplicate nod
        private static CoreApi myCoreApi;

        JDBCEntity exp1 = new Experiment("exp1");
        JDBCEntity exp2 = new Experiment("exp2");
        JDBCEntity exp2c = new Experiment("exp2");
        JDBCEntity exp11 = new Experiment("exp1-1");
        JDBCEntity exp12 = new Experiment("exp1-2");
        JDBCEntity exp12c = new Experiment("exp1-2");
        JDBCEntity sig121 = new Signal("sig1-2-1");
        JDBCEntity sig121c = new Signal("sig1-2-1");

        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            //实例化StorageEngine，初始化
            var myStorageEngine = new CassandraIndexEngine();//CassandraIndexEngine();//CassandraEngine();
            myStorageEngine.Init("host =127.0.0.1 & database = jdbc_unittest");
            //实例化CoreService和CoreApi
            myCoreApi = CoreApi.GetInstance();
            myCoreApi.CoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment", (IStorageEngine)myStorageEngine);
            //实例化DataTypePlugin，注入StorageEngine、CoreService，将其添加到CoreApi
            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.CoreService);
            myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);
        }

        /// <summary>
        /// */exp1/exp1-1
        /// *     /exp1-2/sig1-2-1
        /// */exp2
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            myCoreApi.clearDb();
            //add root
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2).Wait();
            //add child
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp12).Wait();
            myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121).Wait();
            var wavesig1 = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            myCoreApi.AddOneToExperimentAsync(exp12.Id, wavesig1).Wait();
            var wavesig2 = myCoreApi.CreateSignal("FixedWave-int", "ws2", @"StartTime=-2&SampleInterval=0.5");
            myCoreApi.AddOneToExperimentAsync(exp12.Id, wavesig2).Wait();
        }

        [TestCleanup]
        public void Cleanup()
        {
            myCoreApi.clearDb();
        }

        [TestMethod]
        public void AddEntityTest()
        {
            //add entity with same name to root
            string errorSameNameOnRoot = "";
            try
            {
                myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2c).Wait();
            }
            catch (Exception e)
            {
                errorSameNameOnRoot = e.InnerException.Message;
            }

            //add signal to root, expecting error, signal's parent can only be experiment
            string errorSigOnRoot = "";
            try
            {
                myCoreApi.AddOneToExperimentAsync(Guid.Empty, sig121c).Wait();
            }
            catch (Exception e)
            {
                errorSigOnRoot = e.InnerException.Message;
            }

            //add signal to another signal, expecting error, signal's parent can only be experiment
            string errorAddToSignal = "";
            try
            {
                myCoreApi.AddOneToExperimentAsync(sig121.Id, sig121c).Wait();
            }
            catch (Exception e)
            {
                errorAddToSignal = e.InnerException.Message;
            }

            //add the sig with the same name to the parent errexpected
            string errorSameName = "";
            try
            {
                myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121c).Wait();
            }
            catch (Exception e)
            {
                errorSameName = e.InnerException.Message;
            }

            //the parent is  non exist, error expected
            string errorParentNonExistWhenAdd = "";
            try
            {
                myCoreApi.AddOneToExperimentAsync(Guid.NewGuid(), sig121c).Wait();
            }
            catch (Exception e)
            {
                errorParentNonExistWhenAdd = e.InnerException.Message;
            }

            //get child for later asserter
            var roots = myCoreApi.FindAllChildrenByParentIdAsync(Guid.Empty).Result;
            var rootsById = myCoreApi.FindAllChildrenByParentIdAsync(Guid.Empty).Result;
            var secLevId = myCoreApi.FindAllChildrenIdsByParentIdAsync(exp1.Id).Result;
            var secLevIdById = myCoreApi.FindAllChildrenIdsByParentIdAsync(exp1.Id).Result;
            var trdLevIdById = myCoreApi.FindAllChildrenIdsByParentIdAsync(exp12.Id).Result;
            var shouldBeEmpty = myCoreApi.FindAllChildrenIdsByParentIdAsync(exp11.Id).Result;
            //the parent is  non exist, error expected
            string errorParentNonExist = "";
            try
            {
                var tNothing = myCoreApi.FindAllChildrenIdsByParentIdAsync(Guid.NewGuid()).Result;
            }
            catch (Exception e)
            {
                errorParentNonExist = e.InnerException.Message;
            }
            
            //assert
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameNameOnRoot);
            Assert.AreEqual(ErrorMessages.NotValidParentError, errorSigOnRoot);
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameName);
            Assert.AreEqual(ErrorMessages.NotValidParentError, errorAddToSignal);
            Assert.AreEqual(ErrorMessages.ParentNotExistError, errorParentNonExistWhenAdd);
            Assert.AreEqual(ErrorMessages.ParentNotExistError, errorParentNonExist);
            CollectionAssert.AreEqual(new List<Guid> { exp1.Id, exp2.Id }, roots.Select(r => r.Id).ToList());
            CollectionAssert.AreEqual(new List<Guid> { exp1.Id, exp2.Id }, rootsById.Select(r => r.Id).ToList());
            CollectionAssert.AreEquivalent(new List<Guid> { exp11.Id, exp12.Id }, secLevId.ToList());
            CollectionAssert.AreEquivalent(new List<Guid> { exp11.Id, exp12.Id }, secLevIdById.ToList());
            Assert.AreEqual(3, trdLevIdById.Count());
            Assert.AreEqual(0,shouldBeEmpty.Count());
            //check type
            Assert.AreEqual(exp11.EntityType, JDBCEntityType.Experiment);
            Assert.AreEqual(sig121.EntityType, JDBCEntityType.Signal);
        }

        [TestMethod]
        public void DeleteEntityTest()
        {
            //delete mistakenly
            string errorDeleteNotExistedEntity = "";
            try
            {
                myCoreApi.DeletAsync(Guid.NewGuid()).Wait();
            }
            catch (Exception e)
            {
                errorDeleteNotExistedEntity = e.InnerException.Message;
            }
            string errorDeleteExperimentWithChildren = "";
            try
            {
                myCoreApi.DeletAsync(exp12.Id).Wait();
            }
            catch (Exception e)
            {
                errorDeleteExperimentWithChildren = e.InnerException.Message;
            }

            //delete properly
            JDBCEntity wavesig = myCoreApi.FindOneByPathAsync("/exp1/exp1-2/ws1").Result;
            //myCoreApi.DeletAsync(wavesig.Id).Wait();
            myCoreApi.DeletAsync(exp11.Id).Wait();
            myCoreApi.DeletAsync(exp12.Id, true).Wait();

            //get child for later asserter
            var wavesigResult = myCoreApi.FindNodeByIdAsync(wavesig.Id).Result;
            var exp11Result = myCoreApi.FindNodeByIdAsync(exp11.Id).Result;
            var exp12Result = myCoreApi.FindNodeByIdAsync(exp12.Id).Result;
            var sig121Result = myCoreApi.FindNodeByIdAsync(sig121.Id).Result;

            //assert
            Assert.IsNull(wavesigResult);
            Assert.IsNull(exp11Result);
            Assert.IsNull(exp12Result);
            Assert.IsNull(sig121Result);
        }

        [TestMethod]
        public void DuplicateEntityTest()
        {
            var errorSameName = "";
            try
            {
                myCoreApi.DuplicateAsync(exp12.Id, exp12.ParentId, exp12.Name).Wait();
            }
            catch (Exception e)
            {
                errorSameName = e.InnerException.Message;
            }
            myCoreApi.DuplicateAsync(exp12.Id, exp12.ParentId, "exp1-2c").Wait();
            myCoreApi.DuplicateAsync(exp11.Id, exp2.Id, "exp1-1c").Wait();
            ///now
            //*/exp1/exp1-1
            //*     /exp1-2/sig1-2-1
            //*     /exp1-2c/sig1-2-1
            //*/exp2/exp1-1c

            var exp1_2c = myCoreApi.FindOneByPathAsync("/exp1/exp1-2");
            var sig1_21c = myCoreApi.FindOneByPathAsync("/exp1/exp1-2/sig1-2-1");
            var exp1_1c = myCoreApi.FindOneByPathAsync("/exp2/exp1-1c");

            //assert 
            Assert.AreEqual(ErrorMessages.NameDuplicateError, errorSameName);
            Assert.IsNotNull(exp1_2c);
            Assert.IsNotNull(exp1_1c);
            Assert.AreEqual(sig121.Name,sig121c.Name);
        }
        [TestMethod]
        public void AuthorizationaTest()
        {
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            //myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2).Wait();
            ////add child
            //myCoreApi.AddOneToExperimentAsync(exp1.Id, exp11).Wait();
            //myCoreApi.AddOneToExperimentAsync(exp1.Id, exp12).Wait();
            //myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121).Wait();
          //  var wavesig1 = (FixedIntervalWaveSignal)myCoreApi.CreateSingal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            //myCoreApi.AddOneToExperimentAsync(exp12.Id, wavesig1).Wait();
            //var wavesig2 = (FixedIntervalWaveSignal)myCoreApi.CreateSingal("FixedWave-int", "ws2", @"StartTime=-2&SampleInterval=0.5");
            //myCoreApi.AddOneToExperimentAsync(exp12.Id, wavesig2).Wait();

            //myCoreApi.AssignPermission(exp1.Id,"root","liu",new KeyValuePair<string,string>("user","1")).Wait();
            //string error="";
            //try
            //{
            //    myCoreApi.AssignPermission(exp1.Id, "liu", "Tom", new KeyValuePair<string, string>("user", "1")).Wait();
            //}
            //catch (Exception e)
            //{
            //    error=e.InnerException.Message;
            //}
            //Assert.AreEqual(error,ErrorMessages.WrongUserError);

            //myCoreApi.AssignPermission(exp1.Id, "liu", "Tom", new KeyValuePair<string, string>("group", "1")).Wait();

            //bool exp2Result = myCoreApi.Authorization(exp1.Id, "Tom", "1");
            //Assert.AreEqual(exp2Result, true);

            //bool exp3Result = myCoreApi.Authorization(exp1.Id, "Tom", "4");
            //Assert.AreEqual(exp3Result, false);

            //bool exp1Result=myCoreApi.Authorization(exp1.Id,"ss","4");
            //Assert.AreEqual(exp1Result,true);

           
        }
    }
}
