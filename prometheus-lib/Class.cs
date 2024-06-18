using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public class Class
    {
        public string Definition;
        public List<Method> Methods;

        public Class(string definition, List<Method> methods) {
            Definition = definition;
            Methods = methods;
        }
    }
}
