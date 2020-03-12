using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Models;
using Jtext103.Identity.Models;
namespace Jtext103.JDBC.Core.Services
{
   /// <summary>
   /// 权限分为数据的读权限4，写权限2，此节点的操作权限1(包括节点的删除，更改，metadata等)
   /// </summary>
   public class JDBCEntityPermission
    {
        private CoreService myCoreService;
        public JDBCEntityPermission()
        {
            myCoreService = CoreService.GetInstance();
        }
        public JDBCEntityPermission(CoreService coreService)
        {
            myCoreService = coreService;
        }

        /// <summary>
        /// 授权,给newuser一个指定权限
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="user">发起授权用户</param>
        /// <param name="newuser">被授权用户</param>
        /// <param name="permisssion">键值对：("group", "1")</param>
        /// <returns></returns>
        public async Task AssignPermission(Guid id, LoginClaim user, string newuser, KeyValuePair<string, string> permisssion)
        {
            JDBCEntity entity = await myCoreService.GetOneByIdAsync(id);
            if(entity == null) 
            {
                throw new Exception(ErrorMessages.EntityNotFoundError);
            }
            else if (!user.Equals(entity.User) && !IsRootRole(user))
            {
                throw new Exception(ErrorMessages.WrongUserError);
            } 
            else //管理员和所有者可进行权限设置
            {
                switch (permisssion.Key.ToLower()) {
                    case "user"://指定所有者
                        entity.SetUser(newuser);
                        break;
                    case "group"://设置其他用户权限
                        if (entity.GroupPermission.ContainsKey(newuser)) {
                            entity.GroupPermission[newuser] = permisssion.Value;
                        } else {
                            entity.GroupPermission.Add(newuser, permisssion.Value);
                        }
                        break;
                    case "others"://设置默认用户权限
                        entity.OthersPermission = permisssion.Value;
                        break;
                    default:
                        throw new Exception(ErrorMessages.NotValidUpdateOperatorError);
                }
                await myCoreService.SaveAsync(entity);
            }
        }
        public bool IsRootRole(LoginClaim user) {
            return user.Roles.Contains("Admin") || user.Roles.Contains("Root");
        }
        /// <summary>
        /// 认证,此用户是否对JDBCEntity有某一操作权限
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="user">发起操作用户</param>
        /// <param name="operation">操作类型："421"（4代表读权限,2代表写权限,1代表节点管理权限）</param>
        /// <returns>Accreditation</returns>
        public async Task<bool> Authorization(Guid id, LoginClaim user, string operation)
        {
            JDBCEntity entity = await myCoreService.GetOneByIdAsync(id);
            //管理员或所有者直接拥有所有权限,包括根节点
            if (IsRootRole(user) || (entity != null && entity.User.Equals(user.UserName)))
            {
                return true;
            }
            //entity不存在直接返回false
            else if (entity == null)
            {
                return false;
            }
            //验证某个用户是否拥有对当前节点的某操作权限
            else if (entity.GroupPermission.ContainsKey(user.UserName) && entity.GroupPermission[user.UserName].Contains(operation))
            {
                return true;
            }
            //验证某个操作是否属于开放权限
            else if(entity.OthersPermission.Contains(operation))
            {
                return true;
            }
            //当权限可继承时，验证某个用户是否拥有对（祖）父节点的某操作权限
            else if(entity.QueryToParentPermission)
            {
                return await Authorization(entity.ParentId, user, operation);
            }
            else
            {
                return false;
            }
        }
    }
}
