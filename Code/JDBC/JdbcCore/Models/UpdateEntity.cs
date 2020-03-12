using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Models
{
    public class UpdateEntity
    {
        public OperatorType Operator { get; set; }
        public object Value { get; set; }

    }
    public enum OperatorType
    {
        Set,
        Push
    }
}
