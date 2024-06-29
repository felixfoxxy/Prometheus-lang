using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pmi_system
{
    public static class SyscallSystem
    {
        public static void Inject(Executor e)
        {
            InternalMethod sleep = new InternalMethod("System.Sleep", "Sleeps for a certain amount of Milliseconds");
            sleep.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        if (e.variables[(ev.Value as Reference).Variable] is int)
                        {
                            Thread.Sleep((int)e.variables[(ev.Value as Reference).Variable]);
                        }
                    }
                }
                else
                    Thread.Sleep(int.Parse(ev.Value.ToString()));
            };

            InternalMethod tostring = new InternalMethod("ToString", "Converts a object to a String");
            tostring.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        tostring.Return(e.variables[(ev.Value as Reference).Variable].ToString());
                    }
                }
                else
                    tostring.Return(ev.Value.ToString());
            };

            InternalMethod exit = new InternalMethod("System.Exit", "Exits the application");
            exit.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        if (e.variables[(ev.Value as Reference).Variable] is int)
                        {
                            Environment.Exit((int)e.variables[(ev.Value as Reference).Variable]);
                        }
                    }
                }
                else
                    Environment.Exit((int)ev.Value);
            };

            e.internalMethods.AddRange(new InternalMethod[] { sleep, exit, tostring });
        }
    }
}
