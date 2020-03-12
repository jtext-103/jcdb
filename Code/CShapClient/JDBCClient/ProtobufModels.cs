using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Jtext103.JDBC.Client {
    [ProtoContract]
    class IntArray {
        [ProtoMember(1)]
        public int[] data { get; set; }
    }
    [ProtoContract]
    class DoubleArray {
        [ProtoMember(1)]
        public double[] data { get; set; }
    }
}
