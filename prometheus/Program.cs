using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Executor executor = new Executor();
            executor.AllowRedefinition = true;

            #region InternalMethods
            InternalMethod print = new InternalMethod("System.Print", "Prints the Value");
            print.OnCall += (object _, InternalMethodCallEventArgs ev) => {
                if (ev.Value is Reference) {
                    if (executor.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        Console.WriteLine(executor.variables[(ev.Value as Reference).Variable]);
                    }
                } else
                    Console.WriteLine(ev.Value);
            };
            
            InternalMethod rettest = new InternalMethod("System.rt", "returns the Value");
            rettest.OnCall += (object _, InternalMethodCallEventArgs ev) => {
                rettest.Return(2);
            };

            executor.internalMethods.Add(print);
            executor.internalMethods.Add(rettest);
            #endregion

            #region Methods
            executor.methods.Add(new Method("App.test", new List<Instruction>()
            {
                new Instruction(Instruction.OpCode.Syscall, "System.Print", "ref :args:"),
                new Instruction(Instruction.OpCode.Ret, null, "lol")
            }));

            executor.methods.Add(new Method("App.Main", new List<Instruction>()
            {
                new Instruction(Instruction.OpCode.Syscall, "System.Print", "ref :args:"),
                
                new Instruction(Instruction.OpCode.Var, "retval", null),

                new Instruction(Instruction.OpCode.Snr, "retval", null),
                new Instruction(Instruction.OpCode.Call, "App.test", 69),
                
                new Instruction(Instruction.OpCode.Syscall, "System.Print", "ref retval"),

                new Instruction(Instruction.OpCode.Snr, "retval", null),
                new Instruction(Instruction.OpCode.Syscall, "System.rt", null),

                new Instruction(Instruction.OpCode.Syscall, "System.Print", "ref retval"),
            }));
            #endregion

            executor.Execute(new Instruction(Instruction.OpCode.Call, "App.Main", "ref :args:"), "uwu");
            Console.WriteLine("---Execution Finished---");
            Console.ReadLine();
        }
    }
}
