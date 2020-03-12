using System;
using Jtext103.JDBC.Client;

namespace CShapClient {
    class Program {
        static void Main(string[] args) {
           var signal = JDBCEntity.getFixedIntervalWaveSignal("path/jtext/1/ws2");
            var result = signal.GetData();
            Console.ReadLine();
        }
    }
}