using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.JdbcCassandraIndexEngine.Models
{
   public class SEPayload
    {
        public Guid parentid { get; set; }
        public string dimensions { get; set; }
        public long indexes { get; set; }
        public Byte[] samples { get; set; }
        public SEPayload() { }
    }
}
