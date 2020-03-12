using System;

namespace Jtext103.JDBC.MongoStorageEngine.Models
{
    public class SESampleAppendResult
    {
        /// <summary>
        /// the payload you have just insert into
        /// </summary>
        public Guid PayloadId { get; set; }

        /// <summary>
        /// the sample count you have just inserted into this payload
        /// </summary>
        public long SampleCount { get; set; }
        public SESampleAppendResult() { }
        public SESampleAppendResult(Guid payloadid, long samplecount)
        {
            PayloadId = payloadid;
            SampleCount = samplecount;
        }
    }
}
