using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicPlugins.TypedSignal
{
    /// <summary>
    /// a complex object hold the fixed interval timebase wave data
    /// </summary>
    public class FixedIntervalWaveComplex<T>
    {
        /// <summary>
        /// signal title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// begin time
        /// </summary>
        public double Start { get; set; }
        
        /// <summary>
        /// end time 
        /// </summary>
        public double End { get; set; }

        /// <summary>
        /// the stored sample interval
        /// </summary>
        public double OrignalSampleInterval { get; set; }
        
        /// <summary>
        /// the requested sample intval
        /// </summary>
        public double DecimatedSampleInterval { get; set; }

        /// <summary>
        /// the start index in the payload
        /// </summary>
        public long StartIndex { get; set; }
        public long Count { get; set; }

        public long DecimationFactor { get; set; }

        public List<T> Data {  get; set; }

        public string Unit { get; set; }
    }
}
