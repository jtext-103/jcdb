using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;

//review ��������ռ��е���
namespace Jtext103.JDBC.Core.StorageEngineInterface
{
    public interface IMatrixStorageEngineInterface : IStorageEngine
    {
        Task<ICursor<T>> GetCursorAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null);

        //Task<IWriter<T>> GetWriter<T>(Guid signalId)
         IWriter<T> GetWriter<T>(JDBCEntity signal);

        /// <summary>
        /// get the samples of a signal in in multiple dimention,
        /// Important:this will return a block of signal in all dimentions make sure the start, count decimationFactor are with the same dimentions
        /// </summary>
        /// <typeparam name="T">the sample type</typeparam>
        /// <param name="start">the start point, hi rank dimension comes in first in the list</param>
        /// <param name="signalId">singal id</param>
        /// <param name="count">how many point in the EACH dim you want to get, hi rank dimension comes in first in the list</param>
        /// <param name="decimationFactor">the decimation  factor in EACH dimensions is larger than one the signal is down
        /// sample to this factor e.g. set to 3, then it will pick 1sample in 3, skip every 2, can not be set less than 1,
        /// hi rank dimension comes in first in the list</param>
        /// <returns>sample array</returns>
        [Obsolete]//review ���и����⣬���ﶼд��obsolete��Ϊʲô�����������ã������һ����Ϊɶ������������˻��Ǿ���Ϊ���ܹ��������õ�
        //mongodb д���������  cassandra û���������
        Task<IEnumerable<T>> GetSamplesAsync<T>(Guid signalId, List<long> start, List<long> count, List<long> decimationFactor = null);
 

        //review �����������ӿں���û��ע�ͣ���������;ѽ
        /// <summary>
        /// д������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signalId"></param>
        /// <param name="dim"></param>
        /// <param name="samples"></param>
        /// <param name="createNewSEPayload"></param>
        /// <returns></returns>
        Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples, bool createNewSEPayload = false);
        /// <summary>
        /// �Զ���Start��Endд������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signalId"></param>
        /// <param name="dim"></param>
        /// <param name="samples"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="createNewSEPayload"></param>
        /// <returns></returns>
        Task AppendSampleAsync<T>(Guid signalId, List<long> dim, List<T> samples,long start,long end, bool createNewSEPayload = false);

        [Obsolete]//review ͬ����6�����ü��һ��
        //Mongodbʵ���ˣ�cassandraûʵ�֣��������ǲ���
        Task<IEnumerable<Guid>> GetPayloadIdsByParentIdAsync(Guid parentId);

        /// <summary>
        /// return the dimensions of the signal
        /// </summary>
        /// <param name="signalId">signal id, only applis to signal</param>
        /// <returns>the dimension of the signal, hi rank dimension comes in first in the list</returns>
        Task<List<long>> GetDimentionsAsync(Guid signalId);

        /// <summary>
        /// return the size of a singal or a SEPayload,
        /// if used on a signal, it returns the combine size of all its SEPayload,size is in byte
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<long> GetSizeAsync(Guid id);
    }
}