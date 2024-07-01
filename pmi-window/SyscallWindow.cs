using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pmi_window
{
    public static class SyscallWindow
    {
        public static void Inject(Executor e)
        {
            Form f = new Form();

            InternalMethod create = new InternalMethod("Window.Create", "Creates a new Window");
            create.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                f = new Form();
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        f.Text = (string)e.variables[(ev.Value as Reference).Variable];
                    }
                }
                else if (ev.Value is string)
                {
                    f.Text = (string)ev.Value;
                }
            };
            /*InternalMethod title = new InternalMethod("Window.Title", "Prints the Value and a new Line");
            title.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Form)
                {
                    if (ev.Value is Reference)
                    {
                        if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                        {
                            sel.Text = (string)e.variables[(ev.Value as Reference).Variable];
                        }
                    }
                    else if(ev.Value is string)
                    {
                        sel.Text = (string)ev.Value;
                    }
                    else if(ev.Value is null)
                    {
                        title.Return(sel.Text);
                    }
                }
            };*/
            InternalMethod show = new InternalMethod("Window.Show", "Creates a empty Window");
            show.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        if (bool.Parse(e.variables[((Reference)ev.Value).Variable].ToString()))
                            f.Show();
                        else
                            f.Hide();
                    }
                }
                else
                {
                    if (bool.Parse(ev.Value.ToString()))
                        f.Show();
                    else
                        f.Hide();
                }
            };
            InternalMethod run = new InternalMethod("Window.Run", "Creates a empty Window");
            run.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is bool)
                {
                    if(bool.Parse(ev.Value.ToString()))
                        new Thread(() => System.Windows.Forms.Application.Run(f)).Start();
                    else
                        System.Windows.Forms.Application.Run(f);
                }
                else
                    System.Windows.Forms.Application.Run(f);
            };
            InternalMethod title = new InternalMethod("Window.Title", "Creates a empty Window");
            title.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if(ev.Value is null)
                {
                    title.Return(f.Text);
                }
                else if (ev.Value is Reference)
                {
                    if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        f.Text = e.variables[((Reference)ev.Value).Variable] as string;
                    }
                }
                else if (ev.Value is string)
                {
                    f.Text = ev.Value as string;
                }
            };
            InternalMethod fld = new InternalMethod("Window.Field", "Creates a empty Window");
            fld.OnCall += (object _, InternalMethodCallEventArgs ev) =>
            {
                if (ev.Value is string[] || ev.Value is Reference)
                {
                    Instruction lastInstruction = e.lastInstruction;
                    string fld_snr = "";
                    if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                        fld_snr = lastInstruction.Target as string;

                    if (fld_snr != "")
                    {
                        if (ev.Value is Reference)
                        {
                            if (e.variables.ContainsKey((ev.Value as Reference).Variable))
                            {
                                string[] args = e.variables[((Reference)ev.Value).Variable] as string[];
                                typeof(Form).GetField(args[0]).SetValue(f, args[1]);
                            }
                        }
                        else
                        {
                            string[] args = ev.Value as string[];
                            typeof(Form).GetField(args[0]).SetValue(f, args[1]);
                        }
                    }
                }
                if(ev.Value is string)
                {
                    fld.Return(typeof(Form).GetField(ev.Value as string).GetValue(f));
                }
            };
            e.internalMethods.AddRange(new InternalMethod[] { create, show, run, title, fld });
        }
    }
}
