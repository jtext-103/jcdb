using System;
using System.Collections.Generic;


namespace Jtext103.JDBC.MongoStorageEngine.Models
{
    abstract public class SEPayload 
    {
        public Guid Id { get; set; }

        /// <summary>
        /// the parent entity id
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// Absolute path like: /A/B/C/D
        /// </summary>
        public string Path { get; set; }

        /// <summary> 
        /// this is a list storing the dimension number from 2nd dim up, 
        /// the lower dimension is in the front of this list
        /// </summary>
        public List<long> Dimensions { get; set; }

        /// <summary>
        /// the first dim start index of this payload
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// the first dim end index of thi payload,
        /// Note that end-start=samples.count-1
        /// </summary>
        public long End { get; set; }

        //todo ctor just like signal
        protected SEPayload(string name)
        {
            Id = new Guid();
            Dimensions = new List<long>();
        }

        protected SEPayload()
        {
            Id = new Guid();
            Dimensions = new List<long>();
        }
    }
}
