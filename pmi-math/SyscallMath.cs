using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmi_math
{
    public static class SyscallMath
    {
        public static void Inject(Executor e)
        {
            InternalMethod toint = new InternalMethod("Int.Parse", "Converts the value into a int and returns it");
            toint.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        toint.Return(int.Parse((string)e.variables[(ev.Value as Reference).Variable]));
                    }
                }
                else
                    toint.Return(int.Parse((string)ev.Value));
            };

            InternalMethod add = new InternalMethod("Int.Add", "Adds the value to the target");
            add.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        if (e.variables[(ev.Value as Reference).Variable] is int)
                        {
                            e.variables[ev.Where.Target as string] = int.Parse((string)e.variables[(string)ev.Where.Target]) + int.Parse((string)e.variables[(string)ev.Where.Target]);
                        }
                    }
                }
                else
                    e.variables[ev.Where.Target as string] = int.Parse((string)e.variables[(string)ev.Where.Target]) + int.Parse((string)ev.Value);
            };

            e.internalMethods.AddRange(new InternalMethod[] { toint, add });
        }
    }
}
