using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Models;

namespace CassandraMongoDBTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //CoreService cs = new CoreService("JDBC-test");
            //Experiment exp1 = new Experiment("exp1");
            //cs.AddJdbcEntityToAsync(Guid.Empty, exp1).Wait();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
