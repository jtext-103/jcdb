using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Interfaces
{
    //review 又是啥注释没有。。。。我记得我们当好似讨论了为啥要ReadObject，这个非generic方法，现在不记得了，要是有注释就好了
    public interface ICursor
    {
        //review point是啥？不是因该叫sample吗？left也不对呀。remaining吧，
        long LeftPoint { get; }

        Task<object> ReadObject(long resultNum);

        IEnumerable<object> IterateCursor(long resultNum);
    }
    public interface ICursor<T>:ICursor
    {
        Task<IEnumerable<T>> Read(long resultNum);
    }
}
