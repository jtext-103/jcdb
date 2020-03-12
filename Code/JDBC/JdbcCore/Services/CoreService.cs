using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Models;
using MongoDB.Driver;
using Jtext103.JDBC.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

//review 对于所有的public的东西都要写summary呀，尽量吧xml注释里面改天的都天了，private的最好也注视一下
//review 关键的实现一定要有注视

namespace Jtext103.JDBC.Core.Services
{
    /// <summary>
    /// the core service used by various plugin and the upper level api.
    /// this is for the developers of the JDCB, not the users
    /// </summary>
    public partial class CoreService
    {
        /// <summary>
        /// 单例模式
        /// </summary>
        static private CoreService instance;

       /// <summary>
       /// 获得实例
       /// </summary>
       /// <returns></returns>
        static public CoreService GetInstance()
        {
            if (instance == null)
            {
                instance = new CoreService();
            }
            return instance;
        }

        public CoreService(){}

        /// <summary>
        /// 数据库相关
        /// </summary>
        private MongoClient client;
        private IMongoCollection<JDBCEntity> collection;
        private IMongoDatabase database;
        private IStorageEngine myStorageEngine;  //数据库引擎

        public IStorageEngine StorageEngine
        {
            get { return myStorageEngine; }
            set { myStorageEngine = value; }
        }

 
        /// review url 这个参数名称不太合适吧，
        /// <summary>
        /// todo the database should be injected here
        /// </summary>
        /// <param name="databaseName">The name of entity's database</param>
        /// <param name="storageEngine">Payload's storageEngine</param>
        /// <param name="databaseUrl">Url of entity's database</param>
        public void Init(string mongoHost, string mongoDatabase, string mongoCollection, IStorageEngine storageEngine=null)
        {//"mongodb://127.0.0.1:27017"
            myStorageEngine = storageEngine;
            client = new MongoClient(mongoHost);
            database = client.GetDatabase(mongoDatabase);
            collection = database.GetCollection<JDBCEntity>(mongoCollection);
          
            //注册基本类
            RegisterClassMap<Experiment>();
            RegisterClassMap<Signal>();
        }

        public void RegisterClassMap<T>()
        {
            BsonClassMap.RegisterClassMap<T>();
        }

        public async Task ClearDb()
        {
          await database.DropCollectionAsync("Experiment");
        }

        //todo:PBI INF,INF1,2is duplicated, it can be done using get children,give it a null or empty guid get root elementy
        //todo for those get methods that returns a collection, add paging functions, so you dont get awful big alot collection WHEN YOU HAVE large data
        //Note: you can split the file into partial class file it the implementation gets too long

        /// <summary>
        /// 查找entity，如果找不到entity==null，找到了返回实例
        /// review 找不到返回null可以在下面的return里面在写一下
        /// </summary>
        /// <param name="id"></param>
        /// <returns>如果找不到返回null，找到了返回实例</returns>
        public async Task<JDBCEntity> GetOneByIdAsync(Guid id)
        {
            IFindFluent<JDBCEntity, JDBCEntity> findFluent = collection.Find(x => x.Id == id);
            JDBCEntity entity = await findFluent.FirstOrDefaultAsync();
            return entity;
        }

        /// <summary>
        /// return all the child entities,如果父节点为空，查找ParentId为空的节点，返回root节点
        /// </summary>
        /// <param name="parentId">the parent Id,give it empty guid get nod directly below root</param>
        /// <returns>return all the child entities,如果父节点为空，查找ParentId为空的节点，返回root节点</returns>
        public async Task<IEnumerable<JDBCEntity>> GetAllChildrenAsync(Guid parentId)
        {
            try
            {
                var filter = Builders<JDBCEntity>.Filter.Eq("ParentId", parentId);
                using (var cursor = await collection.FindAsync(filter))
                {
                    return await cursor.ToListAsync<JDBCEntity>();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
          
        }
        /// <summary>
        /// return all the child entities,give it null get nod directly below root
        /// </summary>
        /// <param name="parent">the parent entity</param>
        /// <returns>return all the child entities,give it null get nod directly below root</returns>
        public async Task<IEnumerable<JDBCEntity>> GetAllChildrenAsync(JDBCEntity parent = null)
        {
            //如果父节点为空，查找ParentId为空的节点，返回root节点
            //如果父节点不为空，查找ParentId和此节点Id相同的节点，为他的所有子节点
            Guid id = parent == null ? Guid.Empty : parent.Id;
            var filter = Builders<JDBCEntity>.Filter.Eq("ParentId", id);
            using (var cursor = await collection.FindAsync(filter))
            {
                return await cursor.ToListAsync<JDBCEntity>();
            }
        }

        /// <summary>
        /// return all the child entity ids, give it null get nod directly below root
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Guid>> GetAllChildrenIdAsync(JDBCEntity parent = null)
        {
            //如果父节点为空，查找ParentId为空的节点，返回root节点
            //如果父节点不为空，查找ParentId和此节点Id相同的节点，为他的所有子节点
            Guid id = parent == null ? Guid.Empty : parent.Id;
            var filter = Builders<JDBCEntity>.Filter.Eq("ParentId", id);
            var projection = Builders<JDBCEntity>.Projection.Include("Id");
            var options = new FindOptions<JDBCEntity, BsonDocument> { Projection = projection };
            using (var cursor = await collection.FindAsync(filter,options))
            {
                var list = cursor.ToListAsync().Result;
                List<Guid> listGuid = new List<Guid>();
                foreach (var item in list)
                {
                    listGuid.Add(item["_id"].AsGuid);
                }
                //如果没找到，返回长度为0的空list
                return listGuid;
            }
        }

        /// <summary>
        /// return all the child entity ids,give it empty guid get nod directly below root
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Guid>> GetAllChildrenIdAsync(Guid parentId)
        {
            var parent = await GetOneByIdAsync(parentId);
            if (!parentId.Equals(Guid.Empty) && parent == null)
            {
               throw new Exception(ErrorMessages.ParentNotExistError);
            }
            var projection = Builders<JDBCEntity>.Projection.Include(m => m.Id);
            var filter = Builders<JDBCEntity>.Filter.Eq("ParentId", parentId);
            var options = new FindOptions<JDBCEntity, BsonDocument> { Projection = projection };
            //如果要排序，限制数量 这里加
            using (var cursor = await collection.FindAsync(filter, options))
            {
                var list = cursor.ToListAsync().Result;
                List<Guid> listGuid = new List<Guid>();
                foreach (var item in list)
                {
                    listGuid.Add(item["_id"].AsGuid);
                }
                //如果没找到，返回长度为0的空list
                return listGuid;
            }
        }

        /// <summary>
        /// get the only child id by specify the name of it
        /// </summary>
        /// <param name="parentId">the parent id, empty to be the root</param>
        /// <param name="name">child name</param>
        /// <returns></returns>
        public async Task<Guid> GetChildIdByNameAsync(Guid parentId, string name)
        {
            var parent = await GetOneByIdAsync(parentId);
            if (!parentId.Equals(Guid.Empty) && parent == null)
            {
                throw new Exception(ErrorMessages.ParentNotExistError);
            }
            var filterBuilder = Builders<JDBCEntity>.Filter;
            var filter = filterBuilder.Eq("ParentId", parentId) & filterBuilder.Eq("Name", name);
            var projection = Builders<JDBCEntity>.Projection.Include("Id");
            var options = new FindOptions<JDBCEntity, BsonDocument> { Projection = projection };
            using (var cursor = await collection.FindAsync(filter,options))
            {
                var list= cursor.ToListAsync().Result.FirstOrDefault();
                Guid entity = Guid.Empty;
                if (list!= null)
                {
                    entity = list["_id"].AsGuid;
                }
                return entity;
            }
        }

        /// <summary>
        /// get the only child entity by specify the name of it
        /// </summary>
        /// <param name="parentId">the parent id, empty to be the root</param>
        /// <param name="name">child name</param>
        /// <returns></returns>
        public async Task<JDBCEntity> GetChildByNameAsync(Guid parentId, string name)
        {
            var parent = await GetOneByIdAsync(parentId);
            if (!parentId.Equals(Guid.Empty) && parent == null)
            {
                 throw new Exception(ErrorMessages.ParentNotExistError);
            }
            var filterBuilder = Builders<JDBCEntity>.Filter;
            var filter = filterBuilder.Eq("ParentId", parentId) & filterBuilder.Eq("Name", name);
            using (var cursor = await collection.FindAsync(filter))
            {
                await cursor.MoveNextAsync();
                if (cursor.Current.Count() != 0)
                {
                    return cursor.Current.FirstOrDefault();
                }
                return null;
            }
        }

        /// <summary>
        /// Get the parent id
        /// </summary>
        /// <param name="id">the child id</param>
        /// <returns></returns>
        public async Task<Guid> GetParentIdAsync(Guid id)
        {
            JDBCEntity child = await GetOneByIdAsync(id);
            return child == null ? Guid.Empty : child.ParentId;
        }

        /// <summary>
        /// get the parent entity
        /// </summary>
        /// <param name="id">the child id</param>
        /// <returns></returns>
        public async Task<JDBCEntity> GetParentAsync(Guid id)
        {
            JDBCEntity child = await GetOneByIdAsync(id);
            if (child == null)
            {
                return null;
            }
            JDBCEntity parent = await GetOneByIdAsync(child.ParentId);
            return parent;
        }

        /// <summary>
        /// delet a experiment, sig, the payload in the sig is also deleted
        /// </summary>
        /// <param name="jdbcId">the id</param>
        /// <param name="isRecuresive">do i delte the children, if set to false and the entity has child, it will throw exeption</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid jdbcId, bool isRecuresive = false)
        {
            JDBCEntity entity = await GetOneByIdAsync(jdbcId);
            if (entity == null)
            {
                throw new Exception(ErrorMessages.EntityNotFoundError);
            }

            List<JDBCEntity> list = (await GetAllChildrenAsync(jdbcId)).ToList();
            //如果有孩子节点，又没指定递归，则不许删除
            if (list.Count() > 0 && isRecuresive == false)
            {
                throw new Exception(ErrorMessages.DeleteEntityWithChildError);
            }
            //如果无孩子，则删除
            if (list.Count() == 0)
            {
                var delfilter = Builders<JDBCEntity>.Filter.Eq(m => m.Id, jdbcId);
                if (entity.EntityType.Equals(JDBCEntityType.Signal))
                {
                    await myStorageEngine.DeleteDataAsync(jdbcId);
                }
                await collection.DeleteOneAsync(delfilter);
            }
            await recursiveDeleteAsync(entity);
            var delfilter1 = Builders<JDBCEntity>.Filter.Eq(m => m.Id, jdbcId);
            await collection.DeleteOneAsync(delfilter1);
        }

        /// <summary>
        /// 递归删除节点
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task recursiveDeleteAsync(JDBCEntity entity)
        {
            List<JDBCEntity> list = (await GetAllChildrenAsync(entity)).ToList();
            for (int i = 0; i < list.Count(); i++)
            {
                var item = list[i];
                if (item.Equals(JDBCEntityType.Signal))
                {
                    await myStorageEngine.DeleteDataAsync(item.Id);  //删除signal下的payload
                }
                //review 
                var delfilter = Builders<JDBCEntity>.Filter.Eq(m => m.Id, item.Id);
                await collection.DeleteOneAsync(delfilter);
                await recursiveDeleteAsync(item);
            }
        }

        /// <summary>
        /// duplicate a entity
        /// </summary>
        /// <param name="sourceId">the id of the source entity</param>
        /// <param name="parentId">the parent entity id of the new entity, empty id for root</param>
        /// <param name="newName">give it a new name</param>
        /// <param name="isRecuresive">do i duplcate the children</param>
        /// <param name="duplicatePayload">du i dupplcate the pyload in the signal, apply to all child eneity</param>
        /// <returns>return new entity</returns>
        public async Task<JDBCEntity> DuplicateAsync(Guid sourceId, Guid parentId, string newName, bool isRecuresive = false, bool duplicatePayload = false)
        {
            //检查是否有源节点
            JDBCEntity source = await GetOneByIdAsync(sourceId);
            if (source == null)
            {
                throw new Exception(ErrorMessages.ExperimentOrSignalNotFoundError);
            }
            var entityWithSameName = await GetChildByNameAsync(parentId, newName);
            if (entityWithSameName != null)
            {
                //如果父节点下有同名节点，报错
                throw new Exception(ErrorMessages.NameDuplicateError);
            }
            JDBCEntity parent = await GetOneByIdAsync(parentId);
            if (source.EntityType.Equals(JDBCEntityType.Signal))
            {
                if (parentId.Equals(Guid.Empty)|| parent == null)
                {
                    //如果parentId==Guid.Empty，源节点又是Signal，则报错
                    //检查是否有指定父节点
                    throw new Exception(ErrorMessages.ParentNotExistError);
                }
                if (!parent.EntityType.Equals(JDBCEntityType.Experiment))
                {
                    throw new Exception(ErrorMessages.NotValidParentError);
                }
                source.Id = Guid.NewGuid();
                source.Path = parent.Path.Trim() + "/" + newName;
                source.Name = newName;
                source.ParentId = parentId;
                await collection.InsertOneAsync(source);
                if (duplicatePayload == true)
                {
                    await myStorageEngine.CopyDataAsync(sourceId, source.Id);
                }
                return source;
            }
            string path = "";
            if (parentId.Equals(Guid.Empty))
            {
                path = "/" + newName;
            }
            else
            {
                if (parent!=null)
                {
                    path = parent.Path.Trim() + "/" + newName;
                }
                else
                {
                    throw new Exception(ErrorMessages.ExperimentOrSignalNotFoundError);
                }
            }
            source.Id = Guid.NewGuid();
            source.Path = path;
            source.Name = newName;
            source.ParentId = parentId;
            await collection.InsertOneAsync(source);
            if (isRecuresive == false && duplicatePayload == true)
            {
                throw new Exception(ErrorMessages.NotValidDuplicateOperatorError);
            }
            if (isRecuresive == true)
            {
                if (duplicatePayload == true)
                {
                    await recursiveCopyPayloadAsync(sourceId, source.Path, source.Id);
                }
                else
                {
                    await recursiveCopyEntityAsync(sourceId, source.Path, source.Id);
                }
            }
            return source;
        }
        /// <summary>
        /// 递归拷贝Experiment和Signal
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="path"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task recursiveCopyEntityAsync(Guid entity, string path,Guid parentId)
        {
            List<JDBCEntity> list = (await GetAllChildrenAsync(entity)).ToList();
            for (int i = 0; i < list.Count(); i++)
            {
                var item = list[i];
                Guid sourceId = item.Id;
                item.Id = Guid.NewGuid();
                item.Path = path.Trim() + "/" + item.Name;
                item.ParentId = parentId;
                await collection.InsertOneAsync(item);
                await recursiveCopyEntityAsync(sourceId, item.Path, item.Id);
            }
        }
        /// <summary>
        /// 递归拷贝Payload
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="path"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task recursiveCopyPayloadAsync(Guid entity, string path, Guid parentId)
        {
            List<JDBCEntity> list = (await GetAllChildrenAsync(entity)).ToList();
            for (int i = 0; i < list.Count(); i++)
            {
                var item = list[i];
                Guid sourceId = item.Id;
                item.Id = Guid.NewGuid();
                item.Path = path.Trim() + "/" + item.Name;
                item.ParentId = parentId;
                await collection.InsertOneAsync(item);
                if (item.EntityType.Equals(JDBCEntityType.Signal))
                {
                    await myStorageEngine.CopyDataAsync(sourceId, item.Id);
                }
                await recursiveCopyEntityAsync(sourceId, item.Path, item.Id);
            }
        }

        /// <summary>
        /// add an entity to the db, if the entity is already saved in the db, this is used to change its parent
        /// </summary>
        /// <param name="parentId">empty for root, it must be a experiment, or exception will be thrown</param>
        /// <param name="entity">the entity to insert it must be a signal or experiment</param>
        /// <returns></returns>
        public async Task<JDBCEntity> AddJdbcEntityToAsync(Guid parentId, JDBCEntity entity)
        {
            //you should check if the new parent is a child of this entity!!!
            //use recursion to do this will only require  one pass, i hava already find a way, hope you find easily
            //!!if the entity has many signal as its children, and the signales have many payload, re-calc the path would course a big overhead
            //查询到同名JdbcEntity抛出异常

            //entity.Name  判断是不是为空
            var entityWithSameName = await GetChildByNameAsync(parentId, entity.Name);
            if (entityWithSameName != null)
            {
                //如果父节点下有同名节点，报错
                throw new Exception(ErrorMessages.NameDuplicateError);
            }
         
            if (parentId.Equals(Guid.Empty))
            {
                var child = await GetOneByIdAsync(entity.Id);
                if (child == null)
                {
                    if (entity.EntityType.Equals(JDBCEntityType.Experiment))
                    {
                        entity.Path = "/" + entity.Name;
                        await collection.InsertOneAsync(entity);
                    }
                    else
                    {
                        throw new Exception(ErrorMessages.NotValidParentError);
                    }
                    return entity;
                } 
                else 
                {
                    string path = "/" + child.Name;
                    child.Path = path;
                    var filter = Builders<JDBCEntity>.Filter.Eq(m => m.Id, child.Id);
                    var update = Builders<JDBCEntity>.Update.Set("Path", path).Set("ParentId", Guid.Empty);
                    await collection.UpdateOneAsync(filter, update);
                    await RecursiveChangePathAsync(child);
                    return child;
                }
            }
            else
            {
                JDBCEntity parent = await GetOneByIdAsync(parentId);
                if (parent == null)
                {
                    throw new Exception(ErrorMessages.ParentNotExistError);
                }
                if (!parent.EntityType.Equals(JDBCEntityType.Experiment))
                {
                    throw new Exception(ErrorMessages.NotValidParentError);
                }
                //如果是自己的子节点
                //遍历函数 判断父节点路径下是否有子节点
                //判断这个entity是新实例，还是原来在数据库的
                var child = await GetOneByIdAsync(entity.Id);
                if (child == null)
                {
                    entity.ParentId = parentId;
                    string path = parent.Path;
                    entity.Path = path.Trim() + "/" + entity.Name;
                    await collection.InsertOneAsync(entity);//await
                    return entity;
                }
                else
                {   //  child:/a/b/c/d/e   父节点是c，子节点是e
                    //  children: /a/b/c/d/e/f/g  包含 parent:  /a/b/c   
                    //    更改后得到      不用处理：/a/b/c/d   需要处理的：/a/b/c/e/f/g  
                    //    子节点        需要更新path  和 ParentId（改变了父节点）
                    //     子节点的孩子 需要更新path  /a/b/c/e/f/g
                    var parentPath = parent.Path;
                    var childPath = child.Path;
                    if (parentPath.Count()>=childPath.Count())
                    {
                        var compare = parentPath.Substring(0, childPath.Count());
                        if (compare.Equals(childPath))
                        {
                            throw new Exception(ErrorMessages.ParentLoopError); 
                        }
                    }
                    string path = parent.Path.Trim() + "/" + child.Name;
                    child.Path = path;   //递归时有用
                    child.ParentId = parentId;
                    var filter = Builders<JDBCEntity>.Filter.Eq(m => m.Id, child.Id);
                    var update = Builders<JDBCEntity>.Update.Set("Path", path).Set("ParentId", parentId);
                    await collection.UpdateOneAsync(filter, update);
                    await RecursiveChangePathAsync(child);
                    return child;
                }
            }
        }
        private async Task RecursiveChangePathAsync(JDBCEntity entity)
        {
            //  child:/a/b/c/d/e   父节点是c，子节点是e
            //  children: /a/b/c/d/e/f/g  包含 parent:  /a/b/c   
            //    更改后得到      不用处理：/a/b/c/d   需要处理的：/a/b/c/e/f/g  
            //    子节点        需要更新path  和 ParentId（改变了父节点）
            //     子节点的孩子 需要更新path  /a/b/c/e/f/g
            List<JDBCEntity> list = (await GetAllChildrenAsync(entity)).ToList();
            foreach (var item in list)
            {
                //更改子节点的路径
                string path = entity.Path.Trim() + "/" + item.Name;
                //updata
                var filter = Builders<JDBCEntity>.Filter.Eq(m => m.Id, item.Id);
                var update = Builders<JDBCEntity>.Update.Set(m => m.Path, path);
                await collection.UpdateOneAsync(filter, update);
                await RecursiveChangePathAsync(item);
            }
        }
        /// <summary>
        /// get one by path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>return a JDBCEntity</returns>
        public async Task<JDBCEntity> GetOneByPathAsync(string path)
        {
            if (path.LastOrDefault().Equals('/'))
            {
                path = path.Substring(0,path.Length-1);
            }
            var filter = Builders<JDBCEntity>.Filter.Eq("Path", path);
            using (var cursor = await collection.FindAsync(filter))
            {
                await cursor.MoveNextAsync();
                if (cursor.Current.Count()!=0)
                {
                    return cursor.Current.FirstOrDefault();
                }
                return null;
            }
        }
        /// <summary>
        /// Save experiment or signal
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task SaveAsync(JDBCEntity entity)
        {
            var filter = Builders<JDBCEntity>.Filter.Eq("Id", entity.Id);
            await collection.ReplaceOneAsync(filter, entity);
        }

        /// <summary>
        /// Update experiment or signal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public async Task<JDBCEntity> UpDateAsync(Guid id, Dictionary<string,UpdateEntity> updateDic)
        {
            var filter = Builders<JDBCEntity>.Filter.Eq("Id", id);
            var Builder = Builders<JDBCEntity>.Update;
            var update = new List<UpdateDefinition<JDBCEntity>>();
            //数组如何转换,字典如何update，filed逻辑判断
            string[] fieldType = { "Id", "Name", "GroupPermission" , "OthersPermission" };

            foreach (var item in updateDic)
            {
                if (fieldType.Contains(item.Key))
                {
                    throw new Exception(ErrorMessages.NotValidUpdateFieldError);
                }

                OperatorType control = item.Value.Operator;
                switch (control)
                {
                    case OperatorType.Set: //对属性进行重新赋值（包括Array属性）
                        update.Add(Builder.Set(item.Key, item.Value.Value)); 
                        break;
                    case OperatorType.Push: //对Array属性添加取值
                        if (item.Value.Value is IEnumerable<object>)//对Array属性添加一组取值
                        {
                            var value = item.Value.Value as IEnumerable<object>;
                            update.Add(Builder.PushEach<object>(item.Key,value));
                        }
                        else//对Array属性添加一个取值
                        {
                            update.Add(Builder.Push(item.Key, item.Value.Value));
                        }
                        break;
                    default:throw new Exception(ErrorMessages.NotValidUpdateOperatorError);
                }
            }
            await collection.UpdateOneAsync(filter, Builder.Combine(update));
            return await GetOneByIdAsync(id);
        }
        /// <summary>
        /// Rename experiment or signal
        /// </summary>
        /// <param name="id">The id of JDBCEntity</param>
        /// <param name="name">Rename</param>
        /// <returns></returns>
        public async Task<JDBCEntity> ReNameAsync(Guid id, string name)
        {
            var item = await GetOneByIdAsync(id);
            var child = await GetChildByNameAsync(item.ParentId, name);
            if (child == null)
            {
                string oldpath = item.Path.Trim();
                string newPath = oldpath.Substring(0, oldpath.LastIndexOf("/")) + "/" + name;
                var filter = Builders<JDBCEntity>.Filter.Eq(m => m.Id, id);
                var update = Builders<JDBCEntity>.Update.Set("Name", name).Set("Path", newPath);
                await collection.UpdateOneAsync(filter, update);
                item = await GetOneByIdAsync(id);
                await RecursiveChangePathAsync(item);
            }
            else
            {
                throw new Exception(ErrorMessages.NameDuplicateError);
            }
            return item;
        }


    }
}
