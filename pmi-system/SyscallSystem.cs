using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmi_system
{
    public static class SyscallSystem
    {
        public static void Inject(Executor e)
        {
            InternalMethod println = new InternalMethod("System.PrintLine", "Prints the Value and a new Line");
            println.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        Console.WriteLine(e.variables[(ev.Value as Reference).Variable]);
                    }
                }
                else
                    Console.WriteLine(ev.Value);
            };

            InternalMethod print = new InternalMethod("System.Print", "Prints the Value");
            print.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        Console.Write(e.variables[(ev.Value as Reference).Variable]);
                    }
                }
                else
                    Console.Write(ev.Value);
            };

            InternalMethod readline = new InternalMethod("System.ReadLine", "Reads User input");
            readline.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value as string != null)
                {
                    Console.Write(ev.Value as string);
                }
                readline.Return(Console.ReadLine());
            };

            e.internalMethods.AddRange(new InternalMethod[] { println, print, readline });
        }
    }
}
