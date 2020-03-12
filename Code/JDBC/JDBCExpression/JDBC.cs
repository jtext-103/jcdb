using System;
using System.Collections.Generic;
using System.Linq;
using ILNumerics;
using Jtext103.JDBC.Core.Interfaces;
//using BasicPlugins.TypedSignal;

namespace Jtext103.JDBC.JDBCExpression
{
    public class JDBC
    {
        /// </summary>
        static private JDBC instance;

        /// <summary>
        /// 获得实例
        /// </summary>
        /// <returns></returns>
        static public JDBC GetInstance()
        {
            if (instance == null)
            {
                instance = new JDBC();
            }
            return instance;
        }

        public static Dictionary<string, ICursor> CursorDictionary { get; set; }
        public static long resultNum { get; set; }

        public JDBC(Dictionary<string, ICursor> CursorDictionary,long resultNum)
        {
            JDBC.CursorDictionary = CursorDictionary;
            JDBC.resultNum = resultNum;
        }
        public JDBC() { }

        //public static ILArray<double> Signal(string path)
        //{
        //    if (CursorDictionary.Keys.Contains(path))
        //    {
        //        ICursor<double> cursor = (ICursor<double>)CursorDictionary[path];
        //        ILArray<double> result = cursor.Read(resultNum).Result.ToArray();
        //        Calculator cal = new Calculator(result);
        //        return result;
        //    }
        //    throw new Exception("Cursor cannot find the path,check the cursorDictionary again");
        //}
        public static Calculator Signal(string path)
        {
            if (CursorDictionary.Keys.Contains(path))
            {
                ICursor<double> cursor = (ICursor<double>)CursorDictionary[path];
                ILArray<double> result = cursor.Read(resultNum).Result.ToArray();
                Calculator cal = new Calculator(result);
                return cal;
            }
            throw new Exception("Cursor cannot find the path,check the cursorDictionary again");
        }
    }
    public class Calculator
    {
        public ILArray<double> Value;
        public Calculator(ILArray<double> value)
        {
            Value = value;
        }
        public static Calculator operator +(Calculator value1, Calculator value12)
        {
            ILArray<double> result = value1.Value + value12.Value;
            return new Calculator(result);
        }

        public static Calculator operator +(Calculator value1, double value2)
        {
            ILArray<double> result = value1.Value + value2;
            return new Calculator(result);
        }
        public static Calculator operator +(double value1, Calculator value2)
        {
            return value2 + value1;
        }
        public static Calculator operator +(Calculator value1,double[] value2 )
        {
            ILArray<double> tmpResult = value2;
            ILArray<double> result = value1.Value + tmpResult;
            return new Calculator(result);;
        }
        public static Calculator operator +(double[] value1, Calculator value2)
        {
            return value2+value1 ;
        }
        public static Calculator operator -(Calculator value1,double[] value2)
        {
            ILArray<double> tmpResult = value2;
            ILArray<double> result = value1.Value - tmpResult;
            return new Calculator(result); 
        }
        public static Calculator operator -(double[] value1, Calculator value2)
        {
            ILArray<double> tmpResult = value1;
            ILArray<double> result = tmpResult- value2.Value  ;
            return new Calculator(result);
        }
        public static Calculator operator -(Calculator value1, double value2)
        {
            ILArray<double> result = value1.Value - value2;
            return new Calculator(result); ;
        }
        public static Calculator operator -( double value1,Calculator value2)
        {
            ILArray<double> result = value1-value2.Value;
            return new Calculator(result); ;
        }

        public static Calculator operator -(Calculator value1, Calculator value2)
        {
            ILArray<double> result = value1.Value -value2.Value;
            return new Calculator(result);
        }

        public static Calculator operator *(Calculator value1, Calculator value2)
        {
            ILArray<double> result = value1.Value * value2.Value;
            return new Calculator(result);
        }

        public static Calculator operator *(Calculator value1, double value2)
        {
            ILArray<double> result = value1.Value * value2;
            return new Calculator(result);
        }
        public static Calculator operator *(double value1, Calculator value2)
        {
            return value2*value1;
        }
        public static Calculator operator *(Calculator value1, double[] value2)
        {
            ILArray<double> tmpResult = value2;
            ILArray<double> result = value1.Value * tmpResult;
            return new Calculator(result);
        }
        public static Calculator operator *(double[] value1, Calculator value2)
        {
            return value2 * value1;
        }

        public static Calculator operator /(Calculator value1, Calculator value2)
        {
            ILArray<double> result = value1.Value / value2.Value;
            return new Calculator(result);
        }

        public static Calculator operator /(Calculator value1, double value2)
        {
            ILArray<double> result = value1.Value / value2;
            return new Calculator(result);
        }
        public static Calculator operator /(double value1, Calculator value2)
        {
            ILArray<double> result = value1 / value2.Value;
            return new Calculator(result);
        }
        public static Calculator operator /(Calculator value1, double[] value2)
        {
            ILArray<double> tmpResult = value2;
            ILArray<double> result = value1.Value / tmpResult;
            return new Calculator(result);
        }
        public static Calculator operator /(double[] value1, Calculator value2)
        {
            ILArray<double> tmpResult = value1;
            ILArray<double> result = tmpResult /value2.Value ;
            return new Calculator(result);
        }

    }
}
