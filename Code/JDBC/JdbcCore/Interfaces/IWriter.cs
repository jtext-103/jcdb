using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Interfaces
{
    //public interface IWriter : IDisposable
    //{
    //    Task AppendSampleAsync(List<long> dim, object samples);
    //}

    //public interface IWriter<T> : IDisposable
    //{
    //    Task AppendSampleAsync(List<long> dim, List<T> samples);
    //}
    public interface IWriter<T> 
    {
        Task AppendSampleAsync(List<long> dim, List<T> samples);
        Task DisposeAsync();
    }

}