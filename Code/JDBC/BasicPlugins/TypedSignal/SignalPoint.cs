using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicPlugins.TypedSignal
{
    //之前通过Int和Double的 class 吧泛型笑掉了这里引入了后面会不会有什么问题？
    public class SignalPoint<T>
    {
        public double X { get; set; }
        public T Y { get; set; }
        public SignalPoint(double x,T y){
            X = x;
            Y = y;
        }
    }
}
