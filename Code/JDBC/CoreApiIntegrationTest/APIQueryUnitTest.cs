using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicPlugins;
using Jtext103.JDBC.Core.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.CassandraStorageEngine;
using Jtext103.JDBC.Core.StorageEngineInterface;

namespace CoreApiIntegrationTest
{
    [TestClass]
    public class APIQueryUnitTest
    {
        //class init should:
        //create core service, create

        private static CoreApi myCoreApi;
        private static CoreService myCoreService;
        private static IMatrixStorageEngineInterface storageEngine = new CassandraEngine();

        JDBCEntity exp1 = new Experiment("exp1");
        JDBCEntity sig11 = new Signal("sig1");
        JDBCEntity exp11 = new Experiment("exp1-1");
        JDBCEntity exp12 = new Experiment("exp1-2");
        JDBCEntity sig121 = new Signal("sig1");
        JDBCEntity exp2 = new Experiment("exp2");
        JDBCEntity exp21 = new Experiment("exp1-2");
        JDBCEntity sig21 = new Signal("sig2-1");

        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            //todo wankuanhong inject service here
            myCoreService = CoreService.GetInstance();
            myCoreService.Init("JDBC-test");
            storageEngine.Init("host =127.0.0.1 & database = jdbc_unittest");
            myCoreService.MyStorageEngine = storageEngine;

            myCoreApi = CoreApi.GetInstance();
            myCoreApi.MyCoreService.Init("JDBC-test", (IStorageEngine)storageEngine);
        }

        /// <summary>
        /// exp1
        /// ===sig1
        /// ===exp1-1
        /// ===exp1-2
        /// ======sig1
        /// exp2
        /// ===exp1-2
        /// ===sig2-1
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            myCoreApi.clearDb();

            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp12).Wait();
            myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121).Wait();

            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, exp21).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, sig21).Wait();
        }

        [TestCleanup]
        public void Cleanup()
        {
            myCoreApi.clearDb();
        }
        
        [TestMethod]
        public void IdQueryTest()
        {
            IDQueryPlugIn idQueryPlugIn = new IDQueryPlugIn(myCoreApi.MyCoreService);
            myCoreApi.AddQueryPlugin(idQueryPlugIn);
            IQueryPlugIn queryPlugin = null;
            IEnumerable<JDBCEntity> result = null;
            Uri uri = null;
            //0
            uri = new Uri("jdbc:///idx");
            try
            {
                queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorMessages.NotValidURI, e.Message);
            }
            //1
            uri = new Uri("jdbc:///id/");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(0, result.Count());
            //2
            uri = new Uri("jdbc:///id/?name=*");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            //3
            uri = new Uri("jdbc:///id/?name=exp1");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(exp1.Id, result.FirstOrDefault().Id);
            //4
            uri = new Uri("jdbc:///id/?name=*&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(8, result.Count());
            //5
            uri = new Uri("jdbc:///id/?name=exp2-1&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(0, result.Count());
            //6
            uri = new Uri("jdbc:///id/?name=exp1-2&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            //7
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=*");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(3, result.Count());
            //8
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=*&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(4, result.Count());
            //9
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=sig1");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(sig11.Id, result.FirstOrDefault().Id);
            //10
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=sig1&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            //11
            uri = new Uri("jdbc:///id/" + exp1.Id);
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(exp1.Id, result.FirstOrDefault().Id);
        }

        [TestMethod]
        public void PathQueryTest()
        {
            PathQueryPlugIn pathQueryPlugIn = new PathQueryPlugIn(myCoreApi.MyCoreService);
            myCoreApi.AddQueryPlugin(pathQueryPlugIn);
            IQueryPlugIn queryPlugin = null;
            IEnumerable<JDBCEntity> result = null;
            Uri uri = null;
            //0
            uri = new Uri("jdbc:///pathx");
            try
            {
                queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            }
            catch(Exception e)
            {
                Assert.AreEqual(ErrorMessages.NotValidURI, e.Message);
            }
            //1
            uri = new Uri("jdbc:///path/");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(0,result.Count());
            //2
            uri = new Uri("jdbc:///path/?name=*");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            //3
            uri = new Uri("jdbc:///path/?name=exp1");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(exp1.Id, result.FirstOrDefault().Id);
            //4
            uri = new Uri("jdbc:///path/?name=*&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(8, result.Count());
            //5
            uri = new Uri("jdbc:///path/?name=exp2-1&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(0, result.Count());
            //6
            uri = new Uri("jdbc:///path/?name=exp1-2&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            //7
            uri = new Uri("jdbc:///path/exp1?name=*");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(3, result.Count());
            //8
            uri = new Uri("jdbc:///path/exp1?name=*&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(4, result.Count());
            //9
            uri = new Uri("jdbc:///path/exp1?name=sig1");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(sig11.Id, result.FirstOrDefault().Id);
            //10
            uri = new Uri("jdbc:///path/exp1?name=sig1&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
        }
    }
}
