using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Api;

namespace CassandraMongoDBTest
{
    public partial class Form1 : Form
    {
        static string path = Application.StartupPath + "\\User.xml";
        static string filepath = Application.StartupPath + "\\Record.txt";
        static internal Thread[] threads;
        static string[] collect;
        Account acc;
        public Form1()
        {
            InitializeComponent();
            InitialzieForm();
        }
        void InitialzieForm()
        {
            if (File.Exists(path))
            {
                XmlDocument myxml = new XmlDocument();
                myxml.Load(path);
                XmlNode root = myxml.SelectSingleNode("Record");
                XmlNodeList xnl = root.ChildNodes;
                foreach (XmlNode xn in xnl)
                {
                    XmlElement xe = (XmlElement)xn;
                    if (xe.Name.Equals("ThreadNum"))
                    {
                        textBoxThreadNum.Text = xe.InnerText;
                    }
                    if (xe.Name.Equals("DataNum"))
                    {
                        textBoxDataNum.Text = xe.InnerText;
                    }
                    if (xe.Name.Equals("SignalNum"))
                    {
                        textBoxSignalNum.Text = xe.InnerText;
                    }
                    if (xe.Name.Equals("AppendNum"))
                    {
                        textBoxAppendNum.Text = xe.InnerText;
                    }
                    if (xe.Name.Equals("CassandraButton"))
                    {
                        CassandraButton.Checked = Convert.ToBoolean(xe.InnerText);
                    }
                    if (xe.Name.Equals("MongoDBButton"))
                    {
                        MongoDBButton.Checked = Convert.ToBoolean(xe.InnerText);
                    }
                }
            }
            else
            {
                XmlDocument myxml = new XmlDocument();
                //  myxml.LoadXml("<Record></Record>");
                XmlElement xmlelem = myxml.CreateElement("", "Record", "");
                myxml.AppendChild(xmlelem);
                XmlNode root = myxml.SelectSingleNode("Record");
                XmlNode threadnum = myxml.CreateElement("ThreadNum");
                XmlNode datanum = myxml.CreateElement("DataNum");
                XmlNode signalnum = myxml.CreateElement("SignalNum");
                XmlNode appendnum = myxml.CreateElement("AppendNum");
                XmlNode CassandraButton = myxml.CreateElement("CassandraButton");
                CassandraButton.InnerText = "false";
                XmlNode MongoDBButton = myxml.CreateElement("MongoDBButton");
                MongoDBButton.InnerText = "false";
                root.AppendChild(threadnum);
                root.AppendChild(datanum);
                root.AppendChild(signalnum);
                root.AppendChild(appendnum);
                root.AppendChild(CassandraButton);
                root.AppendChild(MongoDBButton);
                myxml.Save(path);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            XmlDocument myxml = new XmlDocument();
            myxml.Load(path);
            XmlNode root = myxml.SelectSingleNode("Record");
            XmlNodeList xnl = root.ChildNodes;
            foreach (XmlNode xn in xnl)
            {
                XmlElement xe = (XmlElement)xn;
                if (xe.Name.Equals("ThreadNum"))
                {
                    xe.InnerText = textBoxThreadNum.Text;
                }
                if (xe.Name.Equals("DataNum"))
                {
                    xe.InnerText = textBoxDataNum.Text;
                }
                if (xe.Name.Equals("SignalNum"))
                {
                    xe.InnerText = textBoxSignalNum.Text;
                }
                if (xe.Name.Equals("AppendNum"))
                {
                    xe.InnerText = textBoxAppendNum.Text;
                }
                if (xe.Name.Equals("CassandraButton"))
                {
                    xe.InnerText = CassandraButton.Checked.ToString();
                }
                if (xe.Name.Equals("MongoDBButton"))
                {
                    xe.InnerText = MongoDBButton.Checked.ToString();
                }
            }
            myxml.Save(path);
        }
        private void butSaveData_Click(object sender, EventArgs e)
        {
            FileStream fs = new FileStream(filepath, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            int total = dataGridView1.RowCount;

            sw.WriteLine(DateTime.Now);
            if (CassandraButton.Checked)
            {
                sw.Write("Cassandra:\t");
            }
            else
            {
                sw.Write("MongoDB:\t");
            }
            sw.WriteLine(textBox2.Text);
            for (int i = 0; i < total - 1; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    sw.Write(dataGridView1.Rows[i].Cells[j].Value);
                    sw.Write("\t");
                }
                sw.WriteLine();
            }
            sw.Close();
            fs.Close();
        }

        private void butStart_Click(object sender, EventArgs e)
        {
         
                int threadnum = Convert.ToInt32(textBoxThreadNum.Text.Trim());

                if (MongoDBButton.Checked)
                {
                    threads = new Thread[threadnum];
                    for (int i = 0; i < threadnum; i++)
                    {
                        Thread t = new Thread(new ThreadStart(acc.MongoDBTransactions));
                        t.Name = i.ToString();
                        threads[i] = t;
                    }
                }
                if (CassandraButton.Checked)
                {
                    threads = new Thread[threadnum];
                    for (int i = 0; i < threadnum; i++)
                    {
                        Thread t = new Thread(new ThreadStart(acc.CassandraTransactions));
                        t.Name = i.ToString();
                        threads[i] = t;
                    }
                }

                for (int i = 0; i < threadnum; i++)
                    threads[i].Start();

                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (int i = 0; i < threadnum; i++)
                    threads[i].Join();
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                textBoxShowTime.Text = ts.TotalMilliseconds.ToString();
                DisplayData(true);
            
        }
        private void DisplayData(bool insert,string query="")
        {
            collect = new string[6];
            collect[0] = textBoxThreadNum.Text;
            collect[1] = textBoxDataNum.Text;
            collect[2] = textBoxSignalNum.Text;
            collect[3] = textBoxAppendNum.Text;
            collect[4] = textBoxShowTime.Text;
            collect[5] = "查询"+query;
            if (insert)
            {
                collect[5] = "插入";
            }
            DataGridViewRow row = new DataGridViewRow();
            int index = dataGridView1.Rows.Add(row);
            for (int i = 0; i < 6; i++)
            {
                dataGridView1.Rows[index].Cells[i].Value = collect[i];
            }
        }

        private void butClearTab_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }

        private void butRestore_Click(object sender, EventArgs e)
        {
            Thread initial = new Thread(new ThreadStart(acc.clearDb));
            initial.Start();
        }
      
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBoxThreadNum.Text.Trim().Length <= 0 && textBoxDataNum.Text.Trim().Length <= 0
                         && textBoxSignalNum.Text.Trim().Length <= 0)
            {
                MessageBox.Show("Please input infor!");
            }
            else
            {
                int threadnum = Convert.ToInt32(textBoxThreadNum.Text.Trim());
                int datanum = Convert.ToInt32(textBoxDataNum.Text.Trim());
                int singalnum = Convert.ToInt32(textBoxSignalNum.Text.Trim());
                int appendnum = Convert.ToInt32(textBoxAppendNum.Text.Trim());
                if (MongoDBButton.Checked)
                {
                    acc = new Account(datanum, singalnum, threadnum, appendnum, false);
                }
                if (CassandraButton.Checked)
                {
                    acc = new Account(datanum, singalnum, threadnum, appendnum);
                }
                Thread initial = new Thread(acc.Setup);
                initial.Start();
            }

        }

        private void button_Query_Click(object sender, EventArgs e)
        {

            int threadnum = Convert.ToInt32(textBoxThreadNum.Text.Trim());
            if (MongoDBButton.Checked)
            {
                threads = new Thread[threadnum];
                for (int i = 0; i < threadnum; i++)
                {
                    Thread t = new Thread(new ThreadStart(acc.QueryTransactions));
                    t.Name = i.ToString();
                    threads[i] = t;
                }
            }
            if (CassandraButton.Checked)
            {
                threads = new Thread[threadnum];
                for (int i = 0; i < threadnum; i++)
                {
                    Thread t = new Thread(new ThreadStart(acc.QueryTransactions));
                    t.Name = i.ToString();
                    threads[i] = t;
                }
            }
            for (int i = 0; i < threadnum; i++)
                threads[i].Start();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < threadnum; i++)
                threads[i].Join();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            textBoxShowTime.Text = ts.TotalMilliseconds.ToString();
            DisplayData(false,"1");
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine(DateTime.Now.Millisecond);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int threadnum = Convert.ToInt32(textBoxThreadNum.Text.Trim());
            if (MongoDBButton.Checked)
            {
                threads = new Thread[threadnum];
                for (int i = 0; i < threadnum; i++)
                {
                    Thread t = new Thread(new ThreadStart(acc.QueryTransactions1));
                    t.Name = i.ToString();
                    threads[i] = t;
                }
            }
            if (CassandraButton.Checked)
            {
                threads = new Thread[threadnum];
                for (int i = 0; i < threadnum; i++)
                {
                    Thread t = new Thread(new ThreadStart(acc.QueryTransactions1));
                    t.Name = i.ToString();
                    threads[i] = t;
                }
            }
            for (int i = 0; i < threadnum; i++)
                threads[i].Start();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < threadnum; i++)
                threads[i].Join();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            textBoxShowTime.Text = ts.TotalMilliseconds.ToString();
            DisplayData(false,"2");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(acc.QueryTransactions2));
                t.Start();
            Stopwatch sw = new Stopwatch();
            sw.Start();
                t.Join();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            textBoxShowTime.Text = ts.TotalMilliseconds.ToString();
            DisplayData(false, "3");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(acc.WebTransactions));
            t.Start();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            t.Join();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            textBoxShowTime.Text = ts.TotalMilliseconds.ToString();
            DisplayData(true);
        }
    }
}
