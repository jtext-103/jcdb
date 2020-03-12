using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jtext103.JDBC.Core.Models
{
    //todo your job
   public class Experiment:JDBCEntity
    {
       //todo ctor just like signal
       public Experiment(string name = "") :base(name)
       {
           EntityType = JDBCEntityType.Experiment;
       }
    }
   
}
