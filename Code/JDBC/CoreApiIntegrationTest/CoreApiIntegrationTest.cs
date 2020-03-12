using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jtext103.JDBC.Core.Api;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using BasicPlugins;
using System.Collections.Generic;
using System.Linq;
using BasicPlugins.TypedSignal;
using Jtext103.JDBC.JdbcCassandraIndexEngine;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Jtext103.JDBC.JDBCExpression;
using System.Diagnostics;

namespace CoreApiIntegrationTest
{
    /// <summary>
    /// 对CoreAPI的BasicPlugin进行测试
    /// </summary>
    [TestClass]
    public class APIIntegrationTest
    {
        private static CoreApi myCoreApi;

        [ClassInitialize]
        public static void BasicSetup(TestContext context)
        {
            var myStorageEngine = new CassandraIndexEngine();//CassandraIndexEngine();//CassandraEngine();
            myStorageEngine.Init("host =127.0.0.1 & database = jdbc_unittest"); //127.0.0.1
            myCoreApi = CoreApi.GetInstance();
            myCoreApi.CoreService.Init("mongodb://127.0.0.1:27017", "JDBC-test", "Experiment", (IStorageEngine)myStorageEngine);
        }

        [TestInitialize]
        public void Setup()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
            myCoreApi.clearDb(true);
        }

        /// <summary>
        /// 测试动态加载PlugIn
        /// </summary>
        [TestMethod]
        public void LoadPlugInTest()
        {
            JDBCEntity exp1 = new Experiment("exp1");
            JDBCEntity sig11 = new Signal("sig1-1");
            JDBCEntity exp11 = new Experiment("exp1-1");
            JDBCEntity exp12 = new Experiment("exp1-2");
            JDBCEntity sig121 = new Signal("sig1-2-1");
            JDBCEntity exp2 = new Experiment("exp2");
            JDBCEntity exp21 = new Experiment("exp1-2");
            JDBCEntity sig21 = new Signal("sig2-1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp12).Wait();
            myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121).Wait();

            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, exp21).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, sig21).Wait();

            // 1 测试加载IDQueryPlugIn
            string IdQueryPlugInPath = @"F:\JTEXTSVN\JtextDBCloud\Code\JDBC\IDQueryPlugInTestDll\bin\Debug\IDQueryPlugInTestDll.dll";
            myCoreApi.LoadQueryPlugin(IdQueryPlugInPath);

            IQueryPlugIn queryPlugin = null;
            IEnumerable<JDBCEntity> result = null;
            Uri uri = null;

            uri = new Uri("jdbc:///idx");
            try
            {
                queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorMessages.NotValidURIError, e.Message);
            }
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=*");
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(3, result.Count());

            try
            {
                uri = new Uri("jdbc:///path/?name=*");
                result = myCoreApi.FindNodeByUriAsync(uri).Result;
                Assert.AreEqual(2, result.Count());
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorMessages.NotValidURIError, e.InnerException.Message);
            }

            // 2 测试加载PathQueryPlugIn
            string PathQueryPlugInPath = @"F:\JTEXTSVN\JtextDBCloud\Code\JDBC\PathQueryPlugInTestDll\bin\Debug\PathQueryPlugInTestDll.dll";
            myCoreApi.LoadQueryPlugin(PathQueryPlugInPath);

            uri = new Uri("jdbc:///path/?name=*");
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=*");
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(3, result.Count());

            // 3 测试加载DataPlugIn
            string DataPlugInPath = @"F:\JTEXTSVN\JtextDBCloud\Code\JDBC\FixedWaveDataTypePluginTestDll\bin\Debug\FixedWaveDataTypePluginTestDll.dll";
            myCoreApi.LoadDataTypePlugin(DataPlugInPath);

            var waveSignal = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal).Wait();
            //act
            int[] data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            var readBack = ((List<int>)waveSignal.GetDataAsync("", "array").Result).ToArray();
            int[] expectedData = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            CollectionAssert.AreEqual(expectedData, readBack);
        }

        /// <summary>
        /// 测试IdQueryPlugIn插件
        /// </summary>
        [TestMethod]
        public void IdQueryTest()
        {
            JDBCEntity exp1 = new Experiment("exp1");
            JDBCEntity sig11 = new Signal("sig1");
            JDBCEntity exp11 = new Experiment("exp1-1");
            JDBCEntity exp12 = new Experiment("exp1-2");
            JDBCEntity sig121 = new Signal("sig1");
            JDBCEntity exp2 = new Experiment("exp2");
            JDBCEntity exp21 = new Experiment("exp1-2");
            JDBCEntity sig21 = new Signal("sig1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp12).Wait();
            myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121).Wait();

            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, exp21).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, sig21).Wait();

            // exp1
            // ===sig1
            // ===exp1-1
            // ===exp1-2
            // ======sig1
            // exp2
            // ===exp2-1
            // ===sig1

            IDQueryPlugIn idQueryPlugIn = new IDQueryPlugIn(myCoreApi.CoreService);
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
                Assert.AreEqual(ErrorMessages.NotValidURIError, e.Message);
            }
            //1
            uri = new Uri("jdbc:///id/");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(0, result.Count());
            //2
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=*");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(3, result.Count());
            //3
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=*&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(4, result.Count());
            //4
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=sig1");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(sig11.Id, result.FirstOrDefault().Id);
            //5
            uri = new Uri("jdbc:///id/" + exp1.Id + "?name=sig1&recursive=true");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(2, result.Count());
            //6
            uri = new Uri("jdbc:///id/" + exp1.Id);
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(idQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(exp1.Id, result.FirstOrDefault().Id);
        }

        /// <summary>
        /// 测试PathQueryPlugIn插件
        /// </summary>
        [TestMethod]
        public void PathQueryTest()
        {
            JDBCEntity exp1 = new Experiment("exp1");
            JDBCEntity sig11 = new Signal("sig1");
            JDBCEntity exp11 = new Experiment("exp1-1");
            JDBCEntity exp12 = new Experiment("exp1-2");
            JDBCEntity sig121 = new Signal("sig1");
            JDBCEntity exp2 = new Experiment("exp2");
            JDBCEntity exp21 = new Experiment("exp1-2");
            JDBCEntity sig21 = new Signal("sig2-1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp11).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, exp12).Wait();
            myCoreApi.AddOneToExperimentAsync(exp12.Id, sig121).Wait();

            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp2).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, exp21).Wait();
            myCoreApi.AddOneToExperimentAsync(exp2.Id, sig21).Wait();

            // exp1
            // ===sig1
            // ===exp1-1
            // ===exp1-2
            // ======sig1-2-1
            // exp2
            // ===exp2-1
            // ===sig2-1

            PathQueryPlugIn pathQueryPlugIn = new PathQueryPlugIn(myCoreApi.CoreService);
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
            catch (Exception e)
            {
                Assert.AreEqual(ErrorMessages.NotValidURIError, e.Message);
            }
            //1
            uri = new Uri("jdbc:///path/");
            queryPlugin = myCoreApi.GetBestQueryPlugin(uri);
            Assert.AreEqual(pathQueryPlugIn, queryPlugin);
            result = myCoreApi.FindNodeByUriAsync(uri).Result;
            Assert.AreEqual(0, result.Count());
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

        /// <summary>
        /// DataPlugIn添加数据测试
        /// </summary>
        [TestMethod]
        public void AppendDataTest()
        {
            JDBCEntity exp1 = new Experiment("exp1");
            JDBCEntity sig11 = new Signal("sig1-1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();

            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.CoreService);
            myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);

            //var waveSignal = (IntFixedIntervalWaveSignal)myCoreApi.CreateSingal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            var waveSignal = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal).Wait();
            //act
            int[] data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.DisposeAsync().Wait();
            var readBack = ((List<int>)waveSignal.GetDataAsync("", "array").Result).ToArray();

            //var readbackSig = (IntFixedIntervalWaveSignal)myCoreApi.FindNodeByIdAsync(waveSignal.Id).Result;
            var readbackSig = (FixedIntervalWaveSignal)myCoreApi.FindNodeByIdAsync(waveSignal.Id).Result;

            int[] expectedData = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            CollectionAssert.AreEqual(expectedData, readBack);

            Assert.AreEqual(0.5, readbackSig.SampleInterval);
            Assert.AreEqual(-2, readbackSig.StartTime);

            Assert.AreEqual(12.5, readbackSig.EndTime);
            Assert.AreEqual("s", readbackSig.Unit);

        
        }

        /// <summary>
        /// DataPlugIn读取测试
        /// </summary>
        [TestMethod]
        public void ReadDataTest()
        {
            //arange 
            JDBCEntity exp1 = new Experiment("exp1");
            JDBCEntity sig11 = new Signal("sig1-1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();

            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.CoreService);
            myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);

            //var waveSignal = (IntFixedIntervalWaveSignal)myCoreApi.CreateSingal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            var waveSignal = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal).Wait();

            int[] data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.DisposeAsync();

            //act&assert
            //plugin
            IQueryPlugIn pathQueryPlugIn = new PathQueryPlugIn(myCoreApi.CoreService);
            myCoreApi.AddQueryPlugin(pathQueryPlugIn);
            var readBackData = ((List<int>)myCoreApi.GetDataByUriAsync(new Uri("jdbc:///path/exp1?name=ws1#start=-0.6&end=3.7&decimation=3"), "array").Result).ToArray();
            int[] expectedData = { 3, 6, 9, 2 };
            CollectionAssert.AreEqual(expectedData, readBackData);

            //1
            var readBack1 = ((List<int>)waveSignal.GetDataAsync("", "array").Result).ToArray();
            int[] expectedData1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            CollectionAssert.AreEqual(expectedData1, readBack1);

            //2
            var readBack2 = ((List<int>)waveSignal.GetDataAsync(@"start=-0.5&end=3.5", "array").Result).ToArray();
            int[] expectedData2 = { 3, 4, 5, 6, 7, 8, 9, 0, 1 };
            CollectionAssert.AreEqual(expectedData2, readBack2);

            //3
            var readBack3 = ((List<int>)waveSignal.GetDataAsync(@"start=-0.6&end=3.7", "array").Result).ToArray();
            int[] expectedData3 = { 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 };
            CollectionAssert.AreEqual(expectedData3, readBack3);

            //4
            var readBack4 = ((List<int>)waveSignal.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            int[] expectedData4 = { 3, 6, 9, 2 };
            CollectionAssert.AreEqual(expectedData4, readBack4);

            //5
            var readBack5 = (FixedIntervalWaveComplex<int>)waveSignal.GetDataAsync(@"start=-0.6&end=4.6&decimation=3", "complex").Result;
            int[] expectedData5 = { 3, 6, 9, 2 };
            Assert.AreEqual(expectedData5.Length, readBack5.Count);
            CollectionAssert.AreEqual(expectedData5, readBack5.Data);
            Assert.AreEqual(-0.5, readBack5.Start);
            Assert.AreEqual(4, readBack5.End);
            Assert.AreEqual(0.5 * 3, readBack5.DecimatedSampleInterval);
            Assert.AreEqual(3, readBack5.StartIndex);

            //5
            var readBack6 = (FixedIntervalWaveComplex<int>)waveSignal.GetDataAsync(@"start=-0.6&end=3.7&count=3", "complex").Result;
            int[] expectedData6 = { 3, 6, 9, 2 };
            CollectionAssert.AreEqual(expectedData6, readBack6.Data);
            Assert.AreEqual(-0.5, readBack6.Start);
            Assert.AreEqual(4, readBack6.End);
            Assert.AreEqual(0.5 * 3, readBack6.DecimatedSampleInterval);
            Assert.AreEqual(4, readBack6.Count);

            //7
            var readBack7 = (FixedIntervalWaveComplex<int>)waveSignal.GetDataAsync(@"start=-0.6&end=3.7&count=4", "complex").Result;
            int[] dexpecteData2 = { 3, 5, 7, 9, 1 };
            CollectionAssert.AreEqual(dexpecteData2, readBack7.Data);
            Assert.AreEqual(-0.5, readBack7.Start);
            Assert.AreEqual(3.5, readBack7.End);
            Assert.AreEqual(1, readBack7.DecimatedSampleInterval);
            Assert.AreEqual(5, readBack7.Count);
        }
        [TestMethod]
        public async Task CursorReadTest()
        {
            JDBCEntity exp1 = new Experiment("exp1");
            JDBCEntity sig11 = new Signal("sig1-1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait();
            myCoreApi.AddOneToExperimentAsync(exp1.Id, sig11).Wait();

            //var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.MyCoreService);
            //myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);
            
            var waveSignal = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-int", "ws1", @"StartTime=-2&SampleInterval=0.5");
            myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal).Wait();
          
            int[] data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();
            waveSignal.PutDataAsync("", data).Wait();

            ICursor cursor = await waveSignal.GetCursorAsync("");
            while (cursor.LeftPoint > 0)
            {
                var readObject = cursor.ReadObject(50).Result;
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream rems = new MemoryStream();
                formatter.Serialize(rems, readObject);
                var buffer = rems.GetBuffer();
                //    await outputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        [TestMethod]
        public async Task ExpressionTest()
        {
            //arange 
            JDBCEntity exp1 = new Experiment("exp1");
            myCoreApi.AddOneToExperimentAsync(Guid.Empty, exp1).Wait(); 

            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(myCoreApi.CoreService);
            var expressionPlugin = new ExpressionPlugin(myCoreApi.CoreService);
            myCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);
            myCoreApi.AddDataTypePlugin(expressionPlugin);

            var waveSignal1 = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", "ws1", @"StartTime=-2&SampleInterval=0.5");
            var d = myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal1).Result;
            double[] data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            waveSignal1.PutDataAsync("", data).Wait();
            waveSignal1.PutDataAsync("", data).Wait();
            waveSignal1.PutDataAsync("", data).Wait();
            await waveSignal1.DisposeAsync();

            var waveSignal2 = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("FixedWave-double", "ws2", @"StartTime=-2&SampleInterval=0.5");
            var c = myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal2).Result;
            waveSignal2.PutDataAsync("", data).Wait();
            waveSignal2.PutDataAsync("", data).Wait();
            waveSignal2.PutDataAsync("", data).Wait();
            await waveSignal2.DisposeAsync();

            string code = " return  JDBC.Signal(\"/exp1/ws1\")+ JDBC.Signal(\"/exp1/ws2\");";
            var waveSignal3 = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("Expression", "ws3", "");
            waveSignal3.AddExtraInformation("expression", code);
            myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal3).Wait();

            var readBack1 = ((List<double>)waveSignal3.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            double[] expectedData1 = { 6, 12, 18, 4 };//{ 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 }; //int[] expectedData = { 3, 6, 9, 2 };
            CollectionAssert.AreEqual(expectedData1, readBack1);

            string code2 =@"  var a = JDBC.Signal(""/exp1/ws1"") + 2;
            var b = JDBC.Signal(""/exp1/ws2"") + 3;
            var c = a + b; 
            return c;";
            waveSignal3.ModifyExtraInformation("expression", code2);
            myCoreApi.CoreService.SaveAsync(waveSignal3).Wait();
            var readBack2 = ((List<double>)waveSignal3.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            double[] expectedData2 = { 11, 17, 23, 9 };
            CollectionAssert.AreEqual(expectedData2, readBack2);

            string code3 = @"  var a = JDBC.Signal(""/exp1/ws1"") * 2;
            var b = JDBC.Signal(""/exp1/ws2"") + 3;
            var c = a + b; 
            return c;"; 
            waveSignal3.ModifyExtraInformation("expression", code3);
            myCoreApi.CoreService.SaveAsync(waveSignal3).Wait();
            var readBack3 = ((List<double>)waveSignal3.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            double[] expectedData3= { 12, 21, 30, 9 };
            CollectionAssert.AreEqual(expectedData3, readBack3);

            string code4 = @"  var a = JDBC.Signal(""/exp1/ws1"") * 2;  //int[] expectedData = { 3, 6, 9, 2 };
            var b = JDBC.Signal(""/exp1/ws2"") + Math.Sin(Math.PI/2);
            var c = a + b;
            return c;";
            waveSignal3.ModifyExtraInformation("expression", code4);
            myCoreApi.CoreService.SaveAsync(waveSignal3).Wait();
            var readBack4 = ((List<double>)waveSignal3.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            double[] expectedData4 = { 10, 19, 28, 7 };
            CollectionAssert.AreEqual(expectedData4, readBack4);

            string code5 = @"  var a = JDBC.Signal(""/exp1/ws1"") * 2;  //int[] expectedData = { 3, 6, 9, 2 };
            double[] b={-3,-6,-9,-2};
            var c = a + b;
            return c;";
            waveSignal3.ModifyExtraInformation("expression", code5);
            myCoreApi.CoreService.SaveAsync(waveSignal3).Wait();
            var readBack5 = ((List<double>)waveSignal3.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            double[] expectedData5 = { 3, 6, 9, 2 };
            CollectionAssert.AreEqual(expectedData5, readBack5);

            string code6 = " return  JDBC.Signal(\"/exp1/ws1\")+ JDBC.Signal(\"/exp1/ws2\");";
            var waveSignal4 = (FixedIntervalWaveSignal)myCoreApi.CreateSignal("Expression", "ws4", "");///exp1/ws1
            waveSignal4.AddExtraInformation("expression", code6);
            myCoreApi.AddOneToExperimentAsync(exp1.Id, waveSignal4).Wait();

            var readBack6 = ((List<double>)waveSignal4.GetDataAsync(@"start=-0.6&end=3.7&decimation=3", "array").Result).ToArray();
            double[] expectedData6 = { 6, 12, 18, 4 };//{ 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 }; //int[] expectedData = { 3, 6, 9, 2 };
            CollectionAssert.AreEqual(expectedData6, readBack6);
        }
    }
}
