using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace BasicPlugins
{
   public class PythonPlugin
    {
        public PythonPlugin() { }

        public double[] Analysis()
        {
         //   System.Math.
            string filepath = System.AppDomain.CurrentDomain.BaseDirectory + "\\expression.py";
            FileStream fs;
            FileMode mode = FileMode.Create;
            fs = new FileStream(filepath, mode);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLineAsync("import numpy as np");
            sw.WriteLineAsync("import matplotlib.pyplot as plt");
            sw.WriteLineAsync("import scipy.signal");
            sw.WriteLineAsync("import sys");
            sw.Close();
            fs.Close();
            ProcessStartInfo start = new ProcessStartInfo();
            //start.FileName = @"E:\python\python.exe";
            start.FileName = "python";
            start.Arguments = filepath;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                StreamReader reader = process.StandardOutput;
                string result = reader.ReadToEnd();
                reader.Close();
                result = result.Remove(0, 1);
                result = result.Remove(result.Length - 3);
                string[] split = result.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                double[] re = new double[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = split[i].Trim();
                    re[i] = Convert.ToDouble(split[i]);
                }
                return re;
            }
        }
    }
}
