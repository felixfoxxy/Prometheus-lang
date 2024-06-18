using dnlib.DotNet;
using prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus_compile
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Class main = new Class("Entry", new List<Method>()
            {
                new Method("Main", new List<Instruction>()
                {
                    new Instruction(Instruction.OpCode.syscall, "System.Println", "Hello World!"),
                })
            });

            string enc = JsonHandler.ConvertToString(main);
            Console.WriteLine(enc);

            ModuleDefMD module = ModuleDefMD.Load("loader.bin");
            module.Resources.Add(new EmbeddedResource("Source", Encoding.Unicode.GetBytes(Convert.ToBase64String(Encoding.Unicode.GetBytes(enc)))));
            module.Resources.Add(new EmbeddedResource("Version", Encoding.Unicode.GetBytes("1_0")));
            module.Resources.Add(new EmbeddedResource("Local", Encoding.Unicode.GetBytes(true.ToString())));
            module.Write("build.exe");
        }
    }
}
