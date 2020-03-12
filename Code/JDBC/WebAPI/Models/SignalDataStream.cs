using Jtext103.JDBC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web;
using ProtoBuf;

namespace WebAPI.Models
{
    /// <summary>
    /// SignalDataStream
    /// </summary>
    public class SignalDataStream
    {
        private ITypedSignal signal;
        private string fragment;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="fragment"></param>
        public SignalDataStream(ITypedSignal signal,string fragment)
        {
            this.signal = signal;
            this.fragment = fragment;
        }
        /// <summary>
        /// 获得输出流
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                await Task.Run(() => Serializer.Serialize<IEnumerable<object>>(outputStream, signal.IterateData(fragment)));
            }
            catch (Exception ex)
            {
                return;
            }
            finally
            {
                outputStream.Close();
            }
        }
    }
}