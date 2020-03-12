using AutoMapper;
using BasicPlugins;
using Jtext103.JDBC.Core.Api;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using System;
using WebAPI.Models;
using BasicPlugins.TypedSignal;
using Jtext103.JDBC.CassandraStorageEngine;
using Jtext103.JDBC.JdbcCassandraIndexEngine;
using System.Configuration;
using Jtext103.Auth;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI
{
    /// <summary>
    /// 初始化逻辑代码
    /// 初始化数据库、创建映射、写入测试数据、热修复等
    /// </summary>
    public class BusinessConfig
    {
        /// <summary>
        /// CoreApi实例
        /// </summary>
        public static CoreApi MyCoreApi;
        /// <summary>
        /// mongodb服务器地址
        /// </summary>
        public static string mongoHost;
        /// <summary>
        /// mongodb数据库名称
        /// </summary>
        public static string mongoDatabase;
        /// <summary>
        /// mongodb数据集合名称
        /// </summary>
        public static string mongoCollection;
        /// <summary>
        /// cassandra初始化字符串
        /// </summary>
        public static string cassandraInit;
        /// <summary>
        /// 表达式信号根节点
        /// </summary>
        public static JDBCEntity ExpressionRoot { get; set; }
        /// <summary>
        /// config the business logic
        /// </summary>
        public static void ConfigBusiness()
        {
            //从config文件加载数据库连接字符串
            mongoHost = ConfigurationManager.AppSettings["MongoHost"];
            mongoDatabase = ConfigurationManager.AppSettings["MongoDatabase"];
            mongoCollection = ConfigurationManager.AppSettings["MongoCollection"];
            cassandraInit = ConfigurationManager.ConnectionStrings["CassandraDB"].ConnectionString;
            //初始化StorageEngine
            var cassandraStorageEngine = new CassandraIndexEngine();//CassandraEngine
            cassandraStorageEngine.Init(cassandraInit);
            //初始化CoreApi
            MyCoreApi = CoreApi.GetInstance();
            MyCoreApi.CoreService.Init(mongoHost, mongoDatabase, mongoCollection, (IStorageEngine)cassandraStorageEngine);
            //添加QueryPlugIn
            var pathQueryPlugIn = new PathQueryPlugIn(MyCoreApi.CoreService);
            MyCoreApi.AddQueryPlugin(pathQueryPlugIn);
            var idQueryPlugin = new IDQueryPlugIn(MyCoreApi.CoreService);
            MyCoreApi.AddQueryPlugin(idQueryPlugin);

            //添加DataTypePlugin
            var fixedWaveDataTypePlugin = new FixedWaveDataTypePlugin(MyCoreApi.CoreService);
            MyCoreApi.AddDataTypePlugin(fixedWaveDataTypePlugin);

            //添加ExpressionPlugin
            var expressionPlugin = new ExpressionPlugin(MyCoreApi.CoreService);
            MyCoreApi.AddDataTypePlugin(expressionPlugin);

            //将继承类在MongoDB中进行注册
            MyCoreApi.CoreService.RegisterClassMap<FixedIntervalWaveSignal>();
            //MyCoreApi.MyCoreService.RegisterClassMap<IntFixedIntervalWaveSignal>();
            //MyCoreApi.MyCoreService.RegisterClassMap<DoubleFixedIntervalWaveSignal>();
            //创建DTO映射
            ConfigDtoMapping();

            SetExpressionRoot();
        }

        /// <summary>
        /// 创建模型和数据传输对象的映射
        /// </summary>
        public static void ConfigDtoMapping()
        {
            //JDBCEntity
            Mapper.CreateMap<JDBCEntity, NodeDto>();
            Mapper.CreateMap<Experiment, ExperimentDto>();
            Mapper.CreateMap<FixedIntervalWaveSignal, FixedIntervalWaveSignalDto>();
        }
        /// <summary>
        /// 重置ExpressionRoot
        /// </summary>
        public static async void SetExpressionRoot() {
            var uri = new Uri("jdbc:///path/expression");
            ExpressionRoot = (await MyCoreApi.FindNodeByUriAsync(uri)).FirstOrDefault();
            if (ExpressionRoot != null)
            {
                await MyCoreApi.DeletAsync(ExpressionRoot.Id, true);
            }
            ExpressionRoot = await MyCoreApi.AddOneToExperimentAsync(Guid.Empty, new Experiment("expression"));
        }
    }
}