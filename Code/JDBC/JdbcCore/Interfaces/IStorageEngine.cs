using System;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Services;

namespace Jtext103.JDBC.Core.Interfaces
{
    //review 这里一点注释都没有。。。
    /// <summary>
    /// StorageEngine接口
    /// </summary>
    public interface IStorageEngine
    {
        /// <summary>
        /// Initializing StorageEngine
        /// </summary>
        /// <param name="configString"></param>
        void Init(string configString);

        /// <summary>
        /// Connect StorageEngine
        /// </summary>
        /// <param name="configString"></param>
        void Connect(string configString);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <param name="signalId"></param>
        /// <returns></returns>
        Task DeleteDataAsync(Guid signalId);

        Task CopyDataAsync(Guid sourceSignalId,Guid targetSignalId);
        Task ClearDb();
    }
}