using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Models;
using MongoDB.Driver;
using Jtext103.JDBC.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Jtext103.JDBC.Core.Services
{
   public static class CoreServiceExtend
    {
        public static async Task<IEnumerable<JDBCEntity>> FindJdbcEntityAsync(this CoreService myCoreService, JDBCEntity parent, string name, bool recursive)
        {
            List<JDBCEntity> children = new List<JDBCEntity>();
            Guid parentId = Guid.Empty;
            if (parent != null)//父节点非根节点
            {
                parentId = parent.Id;
                if (parent.EntityType != JDBCEntityType.Experiment)//类型非Experiment
                {
                    return new List<JDBCEntity>();
                }
            }
            var childs = await myCoreService.GetAllChildrenAsync(parentId);
            if (name.Equals("*"))//获取所有子节点
            {
                children.AddRange(childs);
            }
            else//按名称获取某一个子节点
            {
                var node = await myCoreService.GetChildByNameAsync(parentId, name);
                if (node != null)
                {
                    children.Add(node);
                }
            }
            foreach (var child in childs)
            {
                if (recursive == true && child.EntityType == JDBCEntityType.Experiment)//递归
                {
                    children.AddRange(await myCoreService.FindJdbcEntityAsync(child, name, true));
                }
            }
            return children;
        }
    }
}

