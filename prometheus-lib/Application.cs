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

        public List<Class> getAllClasses(Class parent, bool recursive = true)
        {
            List<Class> cs = new List<Class>();
            foreach (Class c in parent.Classes)
            {
                cs.Add(c);
                if(recursive)
                {
                    foreach(Class rc in getAllClasses(c, recursive))
                        cs.Add(rc);
                }
            }
            return cs;
        }

        public List<Method> getAllMethods(Class parent, bool recursive = true)
        {
            List<Method> ms = new List<Method>();
            foreach (Class c in parent.Classes)
            {
                foreach(Method m in c.Methods)
                    ms.Add(m);

                if (recursive)
                {
                    foreach (Class rc in getAllClasses(c, recursive))
                    {
                        foreach(Method m in getAllMethods(rc, recursive))
                            ms.Add(m);
                    }
                }
            }
            return ms;
        }
    }
}
