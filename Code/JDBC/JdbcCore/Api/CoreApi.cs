using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Interfaces;

namespace Jtext103.JDBC.Core.Api
{

    //review 这一段代码怎么看意义都不大，看有没有办法处理一下
    /// <summary>
    /// Jdbc服务层对应用层提供的API
    /// </summary>
    public partial class CoreApi
    {
        /// <summary>
        /// 单例模式
        /// </summary>
        static private CoreApi instance;

        private CoreService myCoreService;

        public CoreService CoreService
        {
            get { return myCoreService; } 
        }
        
        static public CoreApi GetInstance()
        {
            if(instance==null)
            {
                instance = new CoreApi();
            }
            return instance;
        }
        /// <summary>
        /// 初始化CoreApi
        /// </summary>
        protected CoreApi()
        {
            myCoreService = CoreService.GetInstance();
            myPermission = new JDBCEntityPermission(myCoreService);
            QueryPlugins = new Dictionary<string, IQueryPlugIn>();
            DataTypePlugins = new Dictionary<string, IDataTypePlugin>();
        }

        #region 查找遍历Node类API
        /// <summary>
        /// 根据Id查找JDBCEntity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> FindNodeByIdAsync(Guid id)
        {
            return await CoreService.GetOneByIdAsync(id);
        }
        /// <summary>
        /// 根据ChildId查找Parent
        /// </summary>
        /// <param name="childId"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> FindParentByChildIdAsync(Guid childId)
        {
            return await CoreService.GetParentAsync(childId);
        }
        /// <summary>
        /// 根据路径查找Node
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> FindOneByPathAsync(string path)
        {
            return await CoreService.GetOneByPathAsync(path);
        }
        /// <summary>
        /// 根据ParentId查找所有的Node
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<JDBCEntity>> FindAllChildrenByParentIdAsync(Guid parentId)
        {
            return await CoreService.GetAllChildrenAsync(parentId);
        }
        /// <summary>
        /// 根据ParentId查找所有的NodeId
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Guid>> FindAllChildrenIdsByParentIdAsync(Guid parentId)
        {
            return await CoreService.GetAllChildrenIdAsync(parentId);
        }
        /// <summary>
        /// 根据ParentId和ChildName查找Node
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> FindChildByParentIdAndChildNameAsync(Guid parentId, string childName)
        {
            return await CoreService.GetChildByNameAsync(parentId,childName);
        }

        #endregion

        #region 增删改Node类API
        /// <summary>
        /// 写测试用，以后删掉
        /// </summary>
        public async Task clearDb(bool clearSamples = false)
        {
            await CoreService.ClearDb();
            if (clearSamples)
            {
               await CoreService.StorageEngine.ClearDb();
            }
        }
        /// <summary>
        /// 更新Node实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> UpdateNodeAsync(Guid id, Dictionary<string, UpdateEntity> list)
        {
            return await CoreService.UpDateAsync(id,list);
        }
        /// <summary>
        /// 修改Node名称（相应修改Path和子节点Path）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> ReNameNodeAsync(Guid id, string name)
        {
            return await CoreService.ReNameAsync(id,name);
        }
        /// <summary>
        /// 添加Node实例到Experiment
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> AddOneToExperimentAsync(Guid parentId, JDBCEntity entity)
        {
            return await CoreService.AddJdbcEntityToAsync(parentId,entity);
        }
        /// <summary>
        /// 移动节点
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> MoveJdbcEntityAsync(Guid parentId, Guid entityId)
        {
            var entity = await CoreService.GetOneByIdAsync(entityId);
            if(entity == null)
            {
                throw new Exception(ErrorMessages.EntityNotFoundError);
            }
            return await CoreService.AddJdbcEntityToAsync(parentId, entity);
        }
        /// <summary>
        /// 删除entity
        /// </summary>
        /// <param name="jdbcId"></param>
        /// <param name="isRecuresive"></param>
        /// <returns></returns>
        public async Task DeletAsync(Guid jdbcId, bool isRecuresive = false)
        {
            await CoreService.DeleteAsync(jdbcId,isRecuresive);
        }

        /// <summary>
        /// 复制节点
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="parentId"></param>
        /// <param name="newName"></param>
        /// <param name="isRecuresive"></param>
        /// <param name="duplicatePayload"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> DuplicateAsync(Guid sourceId, Guid parentId, string newName, bool isRecuresive = false, bool duplicatePayload = false)
        {
            return await CoreService.DuplicateAsync(sourceId,parentId,newName,isRecuresive,duplicatePayload);
        }
        #endregion
    }
}
