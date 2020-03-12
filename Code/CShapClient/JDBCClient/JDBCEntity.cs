using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Jtext103.JDBC.Client {
    public class JDBCEntity {
        private static string urlHead = "http://localhost:12441/entity/";
        private static string GetNodeJson(string query) 
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlHead + query);
            request.Method = "GET";
            request.ContentType = "text/json;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            return retString.Substring(1, retString.Length - 2);
        }
        public static Node getFixedIntervalWaveSignal(string query)
        {
            return JsonConvert.DeserializeObject<Node>(GetNodeJson(query));
        }
    }
    public enum JDBCEntityType {
        Experiment,
        Signal,
    }
    public class Node {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public JDBCEntityType EntityType { get; set; }
        public string Path { get; set; }
        public Dictionary<string, object> ExtraInformation { get; set; }
        public int ChildrenCount { get; set; }
        /// <summary>
        /// 信号数据
        /// </summary>
        private IEnumerable<object> Data { get; set; }
        public List<object> GetData(long count = long.MaxValue, double start = double.MinValue, double end = double.MaxValue) {
            Stream stream = JDBCData.GetStream("id/" + Id.ToString(), count, start, end);
            var back = ProtoBuf.Serializer.Deserialize<List<object>>(stream);
            return back;
        }
    }
}
