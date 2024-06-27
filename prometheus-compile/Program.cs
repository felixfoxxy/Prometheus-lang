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
                    //new Instruction(Instruction.OpCode.var, "txt", ""),
                    //new Instruction(Instruction.OpCode.snr, "txt", null),
                    //new Instruction(Instruction.OpCode.syscall, "System.ReadLine", "Input: "),
                    //new Instruction(Instruction.OpCode.syscall, "System.Print", ":[ref]: txt"),
                    //new Instruction(Instruction.OpCode.syscall, "System.PrintLine", ":[ref]: txt"),
                    new Instruction(Instruction.OpCode.var, "i", 1),
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
