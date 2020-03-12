using System.Collections.Generic;


namespace Jtext103.JDBC.MongoStorageEngine.Models
{
    public class SEPayload<T> : SEPayload
    {

        public List<T> Samples { get; set; }

        //todo ctor just like signal
        public SEPayload(string name)
            : base(name)
        {
            Dimensions = new List<long>();
            Samples = new List<T>();
        }

        public SEPayload()
            : base()
        {
            Dimensions = new List<long>();
            Samples = new List<T>();
        }
    }
}
