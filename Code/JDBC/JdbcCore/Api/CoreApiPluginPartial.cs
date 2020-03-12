 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using MongoDB.Bson.Serialization.Conventions;
using Jtext103.JDBC.Core.Models;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Jtext103.JDBC.Core.Services;

namespace Jtext103.JDBC.Core.Api
{
    public partial  class CoreApi
    {
        public Dictionary<string,IQueryPlugIn> QueryPlugins { get; set; }

        public Dictionary<string,IDataTypePlugin> DataTypePlugins { get; set; }

        #region QueryPlugins
        /// <summary>
        /// 通过query查找Node实例
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JDBCEntity>> FindNodeByUriAsync(Uri uri)
        {
            IQueryPlugIn queryPlugIn = GetBestQueryPlugin(uri);
            //通过queryPathPlugIn查找JDBCEntity实例
            return await queryPlugIn.FindJdbcEntityAsync(uri.PathAndQuery);
        }
        #endregion

        #region DataTypePlugins

        /// <summary>
        /// 创建一个Signal并返回,if dataType="Expression",initString="/exp/sig", dataType="FixedWave",initString="StartTime=-2&SampleInterval=0.5"
        /// </summary>
        /// <param name="dataType">"Expression"/"FixedWave"</param>
        /// <param name="name">the signal name</param>
        /// <param name="initString"></param>
        /// <returns>a new signal object</returns>
        public Signal CreateSignal(string dataType, string name, string initString="")
        {
            return GetBestDataTypePlugin(dataType).CreateSignal(dataType, name, initString);
        }
        /// <summary>
        /// 查询得到数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public async Task<object> GetDataByUriAsync(Uri uri, string format)
        {
            IQueryPlugIn queryPathPlugIn = GetBestQueryPlugin(uri);
            ITypedSignal signal = (await queryPathPlugIn.FindJdbcEntityAsync(uri.PathAndQuery)).FirstOrDefault() as ITypedSignal;
            if (signal != null)
            {
                return await signal.GetDataAsync(uri.Fragment, format);
            }
            else
            {
                throw new Exception(ErrorMessages.ExperimentOrSignalNotFoundError);
            }
        }
        #endregion

        #region Plugin管理
        /// <summary>
        /// 添加一个DataTypePlugin
        /// </summary>
        /// <param name="plugIn"></param>
        public void AddDataTypePlugin(IDataTypePlugin plugIn)
        {
            if (!instance.DataTypePlugins.Keys.Contains(plugIn.Name))
            {
                instance.DataTypePlugins.Add(plugIn.Name, plugIn);
            }
        }

        /// <summary>
        /// 添加一个QueryPlugin
        /// </summary>
        /// <param name="plugIn"></param>
        public void AddQueryPlugin(IQueryPlugIn plugIn)
        {
            if (!instance.QueryPlugins.Keys.Contains(plugIn.Name))
            {
                instance.QueryPlugins.Add(plugIn.Name, plugIn);
            }
        }
        /// <summary>
        /// 从动态库文件加载QueryPlugin，review 这个应该还没测试吧，写个todo家测试的
        /// </summary>
        /// <param name="assemblyFile"></param>
        public void LoadQueryPlugin(string assemblyFile)
        {
            string ext = assemblyFile.Substring(assemblyFile.LastIndexOf("."));
            Assembly dll;
            if (ext != ".dll")
            {
                throw new Exception(ErrorMessages.NotValidFileError);
            }
            try
            {
                 dll = Assembly.LoadFile(assemblyFile);
            }
            catch (Exception)
            {
                throw new Exception(ErrorMessages.FileNotFoundError);
            }
            Type[] types = dll.GetTypes();
            foreach (Type type in types)
            {
                if (typeof(IQueryPlugIn).IsAssignableFrom(type))
                {
                    IQueryPlugIn queryPlugIn = (IQueryPlugIn)Activator.CreateInstance(type, myCoreService);
                    AddQueryPlugin(queryPlugIn);
                }
            }
        }
        /// <summary>
        /// 从动态库文件加载DataTypePlugin
        /// </summary>
        /// <param name="assemblyFile"></param>
        public void LoadDataTypePlugin(string assemblyFile)
        {
            string ext = assemblyFile.Substring(assemblyFile.LastIndexOf("."));
            Assembly dll;
            if (ext != ".dll")
            {
                throw new Exception(ErrorMessages.NotValidFileError);
            }
            try
            {
                dll = Assembly.LoadFile(assemblyFile);
            }
            catch (Exception)
            {
                throw new Exception(ErrorMessages.FileNotFoundError);
            }
            Type[] types = dll.GetTypes();
            foreach (Type type in types)
            {
                if (typeof(IDataProcesser).IsAssignableFrom(type))
                {
                    IDataTypePlugin dataTypePlugIn = (IDataTypePlugin)Activator.CreateInstance(type, myCoreService);
                    AddDataTypePlugin(dataTypePlugIn);
                }
            }
        }
        /// <summary>
        /// 获取最适合处理数据类型dataType的DataTypePlugin
        /// review 这个应该还没测试吧，写个todo家测试的
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public IDataTypePlugin GetBestDataTypePlugin(string dataType)
        {
            var bestDataTypePlugin = instance.DataTypePlugins.OrderByDescending(p => p.Value.ScoreDatatype(dataType)).FirstOrDefault().Value;
            if (bestDataTypePlugin != null && bestDataTypePlugin.ScoreDatatype(dataType) > 0)
            {
                return bestDataTypePlugin;
            }
            else
            {
                throw new Exception(ErrorMessages.NotValidDataTypeError);
            }
        }

        /// <summary>
        /// 获取最适合处理路径uri的QueryPlugin
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IQueryPlugIn GetBestQueryPlugin(Uri query)
        {
            var bestQueryPlugin = instance.QueryPlugins.OrderByDescending(p => p.Value.ScoreQuery(query.PathAndQuery)).FirstOrDefault().Value;
            if (bestQueryPlugin.ScoreQuery(query.PathAndQuery) > 0)
            {
                return bestQueryPlugin;
            }
            else
            {
                throw new Exception(ErrorMessages.NotValidURIError);
            }
        }
        #endregion

        /// <summary>
        /// 将uri反转义为url的中间部分
        /// todo:巨神说以后要移走
        /// review 功能没写不知道具体作用，应该给个例子，这汇总设计具体格式的最好都给个例子
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string ParseToUrlMiddle(Uri uri)
        {
            var urlString = uri.AbsolutePath;
            urlString = urlString + uri.Query.Replace("?","?_").Replace("&","&_");
            urlString = urlString + uri.Fragment.Replace("#", "#__").Replace("&", "&__");
            return urlString;
        }
    }
}
