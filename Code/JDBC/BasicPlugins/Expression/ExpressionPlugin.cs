using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Jtext103.JDBC.Core.Services;
using Jtext103.JDBC.Core.Models;
using Jtext103.JDBC.Core.StorageEngineInterface;
using Jtext103.JDBC.Core.Interfaces;
using BasicPlugins.TypedSignal;
using System.Text.RegularExpressions;

namespace BasicPlugins
{
   public class ExpressionPlugin: IDataTypePlugin
    {
        private CoreService myCoreService { get; set; }
        private IMatrixStorageEngineInterface myStorageEngine { get; set; }

        public string Name
        {
            get
            {
               return "ExpressionPlugin"; 
            }
        }

        public ExpressionPlugin(CoreService coreService)
        {
            myCoreService = coreService;
            myStorageEngine = coreService.StorageEngine as IMatrixStorageEngineInterface;
            if (myStorageEngine == null)
            {
                throw new Exception(ErrorMessages.NotValidStorageEngine);
            }
        }

        public int ScoreDatatype(string dataType)
        {
            return dataType.IndexOf("Expression") == 0 ? 100 : 0;
        }

        public Signal CreateSignal(string datatype, string name, string initPathString="")
        {
            Signal signal = null;
            if (datatype.Equals("Expression"))
            {
                signal = new FixedIntervalWaveSignal(name, "Expression", initPathString);
                return signal;
            }
            throw new Exception(ErrorMessages.NotValidSignalError);
        }

        public async Task<object> GetDataAsync(Signal signal, string fragment, string format)
        {
            var waveSig = signal as FixedIntervalWaveSignal;
            WaveFragment frag =  WaveFragment.Parse(waveSig, fragment);

            var startIndex = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
            var count = (long)Math.Floor((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor) + 1;

            ICursor<double> myCursor = (ICursor<double>) await GetCursorAsync(signal, fragment);
          //  ICursor<double> myCursor = new Cursor<double>(myCoreService, signal, fragment);
          
            List<double> resultArray = new List<double>();

            //防止读时越界 review cursor 目前一次只能读1000个点？
            while (myCursor.LeftPoint > 1000)
            {
                resultArray.AddRange(await myCursor.Read(1000));
            }
            if (myCursor.LeftPoint > 0)
            {
                resultArray.AddRange(await myCursor.Read(myCursor.LeftPoint));
            }
            if (format == "complex")
            {
                return new FixedIntervalWaveComplex<double>
                {
                    Title = waveSig.Path,
                    Start = startIndex * waveSig.SampleInterval + waveSig.StartTime,
                    End = waveSig.StartTime + ((count - 1) * frag.DecimationFactor + startIndex) * waveSig.SampleInterval,
                    Count = count,
                    Data = resultArray,
                    DecimatedSampleInterval = waveSig.SampleInterval * frag.DecimationFactor,
                    OrignalSampleInterval = waveSig.SampleInterval,
                    DecimationFactor = frag.DecimationFactor,
                    StartIndex = startIndex,
                    Unit = waveSig.Unit
                };
            }
            else if (format == "point")
            {
                var points = new List<SignalPoint<double>>();
                //添加中间详细点
                double x = frag.Start;
                foreach (var data in resultArray)
                {
                    points.Add(new SignalPoint<double>(x, data));
                    x += waveSig.SampleInterval * frag.DecimationFactor;
                }
                return points;
            }
            else
            {
                return resultArray;
            }
        }
   
        public Task PutDataAsync(Signal signal, string fragment, object data)
        {
            throw new Exception(ErrorMessages.NotValidOperateofSignalError);
        }

        public async Task<ICursor> GetCursorAsync(Signal signal, string dataFragment)
        {
            var waveSig = signal as FixedIntervalWaveSignal;
            if (waveSig == null)
            {
                throw new Exception(ErrorMessages.NotValidSignalError);
            }
            if (signal.ExtraInformation.Keys.Contains("expression"))
            {
                string code = (string)signal.ExtraInformation["expression"];
                if (code.Contains("JDBC.Signal"))
                {

                    MatchCollection mc = Regex.Matches(code, "(?<=JDBC.Signal\\(\").*?(?=\"\\))");
                    List<Match> matchList = mc.Cast<Match>().ToList();
                    FixedIntervalWaveSignal codeSig = (FixedIntervalWaveSignal)await myCoreService.GetOneByPathAsync(matchList.FirstOrDefault().Value);
                    waveSig.StartTime = codeSig.StartTime;
                    waveSig.EndTime = codeSig.EndTime;
                    waveSig.SampleInterval = codeSig.SampleInterval;
                    waveSig.Unit = codeSig.Unit;
                    await myCoreService.SaveAsync(waveSig);
                }
                else
                {
                    throw new Exception(ErrorMessages.NotValidExpressionStringError);
                }
            }
            else
            {
                throw new Exception(ErrorMessages.ExpressionNotFoundError);
            }
            WaveFragment frag = null;
            try
            {
                frag = WaveFragment.Parse(waveSig, dataFragment);
            }
            catch (Exception)
            {
                throw new Exception(ErrorMessages.NotValidSignalFragmentError);
            }

            var startIndex = (long)Math.Ceiling((frag.Start - waveSig.StartTime) / waveSig.SampleInterval);
            var count = (long)Math.Floor((frag.End - frag.Start) / waveSig.SampleInterval / frag.DecimationFactor) + 1;

            ICursor<double> myCursor = new Cursor<double>(myCoreService, signal, new List<long> { startIndex }, new List<long> { count }, new List<long> { frag.DecimationFactor });
            return myCursor;
        }

        public Task DisposeAsync(Guid id)
        {
            return null;
            // throw new NotImplementedException();
        }
    }
}
