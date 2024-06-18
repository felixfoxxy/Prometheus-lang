using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public class Reference
    {
        public string Variable;

        public Reference(string Variable) { 
            this.Variable = Variable;
        }
    }
}
