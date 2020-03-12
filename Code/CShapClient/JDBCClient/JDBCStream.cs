using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Client {
    public class JDBCData {
        private static string urlHead = "http://localhost:12441/stream/";
        public static Stream GetStream(string query, long count = long.MaxValue, double start = double.MinValue, double end = double.MaxValue) {
            string UrlTail = "?";
            //HttpContent
            if (count != long.MaxValue) {
                UrlTail += "&__count=" + count;
            }
            if (start != double.MinValue) {
                UrlTail += "&__start=" + start;
            }
            if (end != double.MaxValue) {
                UrlTail += "&__start=" + end;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlHead + query + UrlTail);
            request.Method = "GET";
            request.ContentType = "text/json;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            //StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            //string retString = myStreamReader.ReadToEnd();
            //myStreamReader.Close();
            /*
            var ws = new MemoryStream();
            var buffer = new byte[100];
            int read;
            while ((read = myResponseStream.Read(buffer, 0, 100)) != 0) {
                ws.Write(buffer, 0, read);
                Debug.WriteLine("read some data");
            }
            ws.Position = 0;
            var back = ProtoBuf.Serializer.Deserialize<List<int>>(ws);
            myResponseStream.Close();*/

            return myResponseStream;
        }

        public static bool PostData(string UrlTail, string postDataStr) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlHead + UrlTail);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] byteArray = Encoding.UTF8.GetBytes(postDataStr);
            request.ContentLength = byteArray.Length;
            Stream myRequestStream = request.GetRequestStream();
            myRequestStream.Write(byteArray, 0, byteArray.Length);
            myRequestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            List<HttpStatusCode> successCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Accepted };
            return successCodes.Contains(response.StatusCode);
        }
    }
}
