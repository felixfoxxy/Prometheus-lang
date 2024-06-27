using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public class Application
    {
        public string Name;
        public string EntryClass = "";
        public string EntryMethod = "";
        public List<Class> Classes = new List<Class>();

        public Application() { }
    }
}
