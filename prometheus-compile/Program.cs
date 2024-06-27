using dnlib.DotNet;
using prometheus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace prometheus_compile
{
    internal class Program
    {
        public static string GetOwnPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        static void Main(string[] args)
        {
            bool test = true;
            if (test)
            {
                prometheus.Application app = new prometheus.Application();
                app.Name = "test-app";
                app.Classes = new List<Class>()
                {
                    new Class("App", new List<Method>()
                    {
                        new Method("Main", new List<Instruction>()
                        {
                            new Instruction(Instruction.OpCode.var, "i", 0),
                            new Instruction(Instruction.OpCode.call, "loop", null),
                        }),
                        new Method("loop", new List<Instruction>()
                        {
                            new Instruction(Instruction.OpCode.syscall, "System.PrintLine", ":[ref]: i"),
                            new Instruction(Instruction.OpCode.syscall, "System.Sleep", 1000),
                            new Instruction(Instruction.OpCode.breql, "i", 10),
                            new Instruction(Instruction.OpCode.syscall, "System.PrintLine", "end"),
                            new Instruction(Instruction.OpCode.ret, null, null),
                            new Instruction(Instruction.OpCode.brend, null, null),
                            new Instruction(Instruction.OpCode.add, "i", 1),
                            new Instruction(Instruction.OpCode.call, "loop", null),
                        })
                    })
                };
                string enc = JsonHandler.ConvertToString(app);
                Console.WriteLine(enc);
                ModuleDefMD module = ModuleDefMD.Load("loader.bin");
                module.Resources.Add(new EmbeddedResource("Source", Encoding.Unicode.GetBytes(Convert.ToBase64String(Encoding.Unicode.GetBytes(enc))))); module.Resources.Add(new EmbeddedResource("Version", Encoding.Unicode.GetBytes("1_0")));
                module.Resources.Add(new EmbeddedResource("Version", Encoding.Unicode.GetBytes("1_0")));
                module.Resources.Add(new EmbeddedResource("Local", Encoding.Unicode.GetBytes(true.ToString())));
                module.Write("build.exe");
                return;
            }
            if (args.Length == 2)
            {
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine("Invalid Source File!");
                    return;
                }

                ModuleDefMD module = ModuleDefMD.Load("loader.bin");
                module.Resources.Add(new EmbeddedResource("Source", Encoding.Unicode.GetBytes(Convert.ToBase64String(File.ReadAllBytes(args[0])))));
                module.Resources.Add(new EmbeddedResource("Version", Encoding.Unicode.GetBytes("1_0")));
                module.Resources.Add(new EmbeddedResource("Local", Encoding.Unicode.GetBytes(true.ToString())));
                module.Write(args[1]);
            }
            else
            {
                Console.WriteLine("Usage: prometheus-compile [Source File] [Output File]");
            }
        }
    }
}
