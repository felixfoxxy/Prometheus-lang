using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pmi_console
{
    public static class SyscallConsole
    {
        public static void Inject(Executor e)
        {
            InternalMethod println = new InternalMethod("Console.PrintLine", "Prints the Value and a new Line");
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

            InternalMethod print = new InternalMethod("Console.Print", "Prints the Value");
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

            InternalMethod readline = new InternalMethod("Console.ReadLine", "Reads User input");
            readline.OnCall += (object _, InternalMethodCallEventArgs ev) =>
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
                readline.Return(Console.ReadLine());
            };

            InternalMethod readkey = new InternalMethod("Console.ReadKey", "Reads User key");
            readkey.OnCall += (object _, InternalMethodCallEventArgs ev) =>
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
                readkey.Return(Console.ReadKey());
            };

            InternalMethod curvis = new InternalMethod("Console.CursorVisible", "Gets/Sets Console Cursor visibility");
            curvis.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value != null)
                {
                    bool vis = true;
                    if (ev.Value is Reference)
                    {
                        if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                        {
                            vis = (bool)e.variables[(ev.Value as Reference).Variable];
                        }
                    }
                    else
                        vis = (bool)ev.Value;
                    Console.CursorVisible = vis;
                }
                else
                    curvis.Return(Console.CursorVisible);
            };

            InternalMethod title = new InternalMethod("Console.Title", "Get/Set Console Title");
            title.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value != null)
                {
                    string tit = "";
                    if (ev.Value is Reference)
                    {
                        if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                        {
                            tit = (string)e.variables[(ev.Value as Reference).Variable];
                        }
                    }
                    else
                        tit = (string)ev.Value;
                    Console.Title = tit;
                }
                else
                    curvis.Return(Console.Title);
            };

            InternalMethod clear = new InternalMethod("Console.Clear", "Clear the Console");
            clear.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                Console.Clear();
            };

            e.internalMethods.AddRange(new InternalMethod[] { println, print, readline, readkey, curvis, title, clear });
        }
    }
}
