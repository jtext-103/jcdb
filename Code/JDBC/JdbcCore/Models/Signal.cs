using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Models
{
    public class Signal:JDBCEntity
    {
        /// <summary>
        /// 数据类型，指明了数据存储结构、采样类型、元数据等信息
        /// </summary>
        public string DataType { get; set; }
        /// <summary>
        /// 采样类型
        /// </summary>
        public string SampleType { get { return DataType.Substring(DataType.LastIndexOf('-')+1).ToLower(); } }

        public Signal(string name = "") : base(name)
        {
            EntityType = JDBCEntityType.Signal;
        }
    }
}
