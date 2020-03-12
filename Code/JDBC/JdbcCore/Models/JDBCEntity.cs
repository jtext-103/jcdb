using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Models
{
    public class JDBCEntity
    {
        /// <summary>
        /// 节点标识符
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 父节点标识符
        /// </summary>
        public Guid ParentId { get; set; }
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; } 
        /// <summary>
        /// 节点类型
        /// </summary>
        public JDBCEntityType EntityType { get; set; }
        /// <summary>
        /// 节点路径
        /// </summary>
        public string Path { get; set; }

        public long NumberOfSamples { get; set; }

        public bool IsWritting{ get; set; }

        public string User { get; private set; }

        public bool QueryToParentPermission { get; set; }
     
        public Dictionary<string,string> GroupPermission { get; set; }

        public string OthersPermission { get; set; }

        public DateTime CreatedTime { get; set; }

        protected JDBCEntity(string name = "")
        {
            Name = name == null ? "" : name;
            Id = Guid.NewGuid();
            ExtraInformation = new Dictionary<string, object>();
            User = "";
            OthersPermission = "4";
            GroupPermission = new Dictionary<string, string>();
            QueryToParentPermission = true;//默认可以继承权限
          //  ExtraInformation.Add("NumberOfSamples", 300000);
            NumberOfSamples = 500000; //300000
            CreatedTime = DateTime.Now;
        }

        public void SetUser(string user)
        {
            this.User = user;
        }
        public Dictionary<string, object> ExtraInformation { get; set; }

        #region extra
        /// <summary>
        /// 根据key获取ExtraInformation
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetExtraInformationByKey(string key)
        {
            return ExtraInformation[key];
        }
        /// <summary>
        /// ExtraInformation是否包含key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IfExtraInformationContainsKey(string key) 
        {
            return ExtraInformation.ContainsKey(key);
        }
        /// <summary>
        /// ExtraInformation是否包含value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IfExtraInformationContainsValue(string value) {
            return ExtraInformation.ContainsValue(value);
        }
        /// <summary>
        /// 向ExtraInformation中新加入一条信息
        /// </summary>
        /// <param name="key">格式为"writer-key"的字符串，writer为key的属于的类型，key为key的name</param>
        /// <param name="value"></param>
        public void AddExtraInformation(string key, object value)
        {
            ExtraInformation.Add(key, value);
        }

        /// <summary>
        /// 修改ExtraInformation中指定key的内容
        /// 如果该key不存在，则向ExtraInformation中新加入新信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ModifyExtraInformation(string key, object value)
        {
            if (ExtraInformation.ContainsKey(key))
            {
                ExtraInformation[key] = value;
            }
            else
            {
                ExtraInformation.Add(key, value);
            }
        }

        /// <summary>
        /// 删除ExtraInformation中key的类型为keysWriter的所有值
        /// </summary>
        /// <param name="keysWriter"></param>
        public void RemoveExtraInformation(string keysWriter)
        {
            IEnumerable<string> keys = ExtraInformation.Keys;
            foreach (string key in keys)
            {
                if (key.IndexOf(keysWriter) == 0)
                {
                    ExtraInformation.Remove(key);
                }
            }

        }
        #endregion
    }

    public enum JDBCEntityType
    {
        Experiment,
        Signal,
    }
}
