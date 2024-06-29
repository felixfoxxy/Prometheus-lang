using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus_ide
{
    public class CompilerOptions
    {
        public bool AllowRedefinition = false;
        public bool AutoRef = true;
        public bool AddMethodDefClass = true;
        public string CompilerPath = "";
        public string BuildPath = "";

        public CompilerOptions() { }
    }
}
