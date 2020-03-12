using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Interfaces;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.StorageEngineInterface;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Diagnostics;
using ILNumerics;

namespace BasicPlugins
{
    class Cursor<T> :ICursor<T>
    {
        private Dictionary<string, ICursor> CursorDictionary { get; set; }

        private CoreService myCoreService { get; set; }

        private IMatrixStorageEngineInterface myStorageEngine { get; set; }

        private long leftPoint { get; set; }
        private Signal mySignal { get; set; }

        public long LeftPoint
        {
            get
            {
                return leftPoint;
            }
        }
        public Cursor(CoreService CoreService, Signal signal,List<long> start, List<long> count, List<long> decimationFactor = null)
        {
            leftPoint = 1;
            mySignal = signal;
            for (int i = 0; i < count.Count(); i++)
            {
                leftPoint *= count[i];
            }
            myCoreService = CoreService;
            myStorageEngine = myCoreService.StorageEngine as IMatrixStorageEngineInterface;
            if (signal.ExtraInformation.Keys.Contains("expression"))
            {
                string code = (string)signal.ExtraInformation["expression"];
                if (code.Contains("JDBC.Signal"))
                {
                    CursorDictionary = new Dictionary<string, ICursor>();
                    MatchCollection mc = Regex.Matches(code, "(?<=JDBC.Signal\\(\").*?(?=\"\\))");
                    foreach (Match item in mc)
                    {
                        if (!CursorDictionary.Keys.Contains(item.Value))
                        {
                            JDBCEntity waveSig= myCoreService.GetOneByPathAsync(item.Value).Result;
                            ICursor cursor=  myStorageEngine.GetCursorAsync<T>(waveSig.Id,start,count, decimationFactor).Result;
                            CursorDictionary.Add(item.Value,cursor);
                        }
                    }
                }
            }
        }
        //public Cursor(CoreService CoreService, Signal signal, string fragment)
        //{
        //    mySignal = signal;
        //    var waveSig = signal as FixedIntervalWaveSignal;
        //    myCoreService = CoreService;
        //    myStorageEngine = myCoreService.StorageEngine as IMatrixStorageEngineInterface;
        //    if (signal.ExtraInformation.Keys.Contains("expression"))
        //    {
        //        string code = (string)signal.ExtraInformation["expression"];
        //        if (code.Contains("JDBC.Signal"))
        //        {
        //            CursorDictionary = new Dictionary<string, ICursor>();
        //            MatchCollection mc = Regex.Matches(code, "(?<=JDBC.Signal\\(\").*?(?=\"\\))");
        //            List<Match> matchList = mc.Cast<Match>().ToList();
        //            FixedIntervalWaveSignal codeSig = (FixedIntervalWaveSignal)myCoreService.GetOneByPathAsync(matchList.FirstOrDefault().Value).Result;
                   
        //            WaveFragment frag = null;
        //            try
        //            {
        //                frag = WaveFragment.Parse(waveSig, fragment);
        //            }
        //            catch (Exception)
        //            {
        //                throw new Exception(ErrorMessages.NotValidSignalFragment);
        //            }
        //            var startIndex = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
        //            var count = (long)Math.Round((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor) + 1;
        //            leftPoint = count;
        //            foreach (Match item in mc)
        //            {
        //                if (!CursorDictionary.Keys.Contains(item.Value))
        //                {
        //                    JDBCEntity codeEntity = myCoreService.GetOneByPathAsync(item.Value).Result;
        //                    ICursor cursor = myStorageEngine.GetCursorAsync<T>(codeEntity.Id, new List<long> { startIndex }, new List<long> {count} , new List<long> { frag.DecimationFactor }).Result;
        //                    CursorDictionary.Add(item.Value, cursor);
        //                }
        //            }
        //        }
        //    }
        //}

        public IEnumerable<object> IterateCursor(long resultNum)
        {
            throw new NotImplementedException();
        }

        public async Task<object> ReadObject(long resultNum)
        {
            return await Read(resultNum);
        }

        public async Task<IEnumerable<T>> Read(long resultNum)
        {
            string tmpCode = "";
            List<T> result = new List<T>();
            if (mySignal.ExtraInformation.Keys.Contains("expression"))
            {
                tmpCode = (string)mySignal.ExtraInformation["expression"];

                Match mc = Regex.Match(tmpCode, "(?<=return )(?<name>.*?)(?=\\;)");
                string tmp = mc.Groups["name"].Value;
                int index = tmpCode.LastIndexOf("return");
                string code = tmpCode.Substring(0,index) + "return (" + tmp + ").Value;";

                MethodInfo function = getDataFunction(code);
                var betterFunction = (Func<Dictionary<string, ICursor>, long, Object>)Delegate.CreateDelegate(typeof(Func<Dictionary<string, ICursor>, long, Object>), function);
                var tmpResult = (ILArray<T>)betterFunction(CursorDictionary, resultNum);
                result = tmpResult.ToList();
                leftPoint = leftPoint - resultNum;
            }
            return result;
        }

        private MethodInfo getDataFunction(string code)
        {
            string sourceCode = @"
        using System;
        using System.Collections.Generic;
        using Jtext103.JDBC.JDBCExpression;
        using Jtext103.JDBC.Core.Interfaces;
        using ILNumerics;
        namespace UserFunctions
        {                
            public class MatrixFunction
            {                
                public static object GetData(Dictionary<string,ICursor>CursorDictionary ,long resultNum)
                {
                    JDBC jdbc=new JDBC(CursorDictionary,resultNum);
                    userCode;
                }
            }

        }
         ";
         
            string finalCode = sourceCode.Replace("userCode", code);
            var cdp = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.ReferencedAssemblies.Add(@"F:\JTEXTSVN\JtextDBCloud\Code\JDBC\JDBCExpression\bin\Debug\JDBCExpression.dll");
            compilerParams.ReferencedAssemblies.Add(@"F:\JTEXTSVN\JtextDBCloud\Code\JDBC\JDBCExpression\bin\Debug\ILNumerics.Core.dll");
            compilerParams.ReferencedAssemblies.Add(@"F:\JTEXTSVN\JtextDBCloud\Code\JDBC\JdbcCore\bin\Debug\JdbcCore.dll");
            CompilerResults results = cdp.CompileAssemblyFromSource(compilerParams, finalCode);
            if (results.Errors.Count>0)
            {
                string errorText = "";
                foreach (CompilerError ce in results.Errors)
                {
                    if (ce.IsWarning) continue;
                    Debug.WriteLine("{0}({1},{2}: error {3}: {4}", ce.FileName, ce.Line, ce.Column, ce.ErrorNumber, ce.ErrorText);
                    errorText = errorText + ce.ErrorText + ";";
                }
                throw new Exception(errorText);
            }
            Type binaryFunction = results.CompiledAssembly.GetType("UserFunctions.MatrixFunction");
            return binaryFunction.GetMethod("GetData");
        }
    }
}
