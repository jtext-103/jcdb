using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jtext103.Identity.Models;
using Jtext103.JDBC.Core.Services;

namespace Jtext103.JDBC.Core.Api
{
    public partial class CoreApi
    {
        private JDBCEntityPermission myPermission;

        public async Task AssignPermission(Guid id, LoginClaim owner, string newuser, KeyValuePair<string, string> permisssion)
        {
            await myPermission.AssignPermission(id,owner,newuser,permisssion);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="operation">read:4,write:2,x:1,operation:"421"</param>
        /// <returns></returns>
        public async Task<bool> Authorization(Guid id, LoginClaim user, string operation)
        {
            return await myPermission.Authorization(id, user, operation);
        }

    }
}
