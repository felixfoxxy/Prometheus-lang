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
        public List<Method> Methods = new List<Method>();
        public List<Class> Classes = new List<Class>();

        public Class(string definition, List<Method> methods, List<Class> classes)
        {
            Definition = definition;
            Methods = methods;
            Classes = classes;
        }
    }
}
