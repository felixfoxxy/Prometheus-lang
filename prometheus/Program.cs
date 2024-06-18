using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace prometheus
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Executor executor = new Executor();
            executor.AllowRedefinition = false;

            InternalMethod println = new InternalMethod("System.Println", "Prints the Value and a new Line");
            println.OnCall += (object _, InternalMethodCallEventArgs ev) => {
                if (ev.Value is Reference)
                {
                    if (executor.variables.ContainsKey((ev.Value as Reference).Variable))
                    {
                        Console.WriteLine(executor.variables[(ev.Value as Reference).Variable]);
                    }
                }
                else
                    Console.WriteLine(ev.Value);
            };
            executor.internalMethods.Add(println);

            try
            {
                Class loaded = JsonHandler.ConvertToObj<Class>(Encoding.Unicode.GetString(Convert.FromBase64String(args[0])));
                executor.Execute(loaded, "Main", null);
                Console.WriteLine("---Execution Finished---");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            Console.ReadLine();
        }
    }
}


/*
#region InternalMethods


InternalMethod print = new InternalMethod("System.Print", "Prints the Value");
print.OnCall += (object _, InternalMethodCallEventArgs ev) => {
    if (ev.Value is Reference)
    {
        if (executor.variables.ContainsKey((ev.Value as Reference).Variable))
        {
            Console.Write(executor.variables[(ev.Value as Reference).Variable]);
        }
    }
    else
        Console.Write(ev.Value);
};

InternalMethod sleep = new InternalMethod("System.Sleep", "Pauses the Thread for a specified time");
sleep.OnCall += (object _, InternalMethodCallEventArgs ev) => {
    if (ev.Value is int)
    {
        Thread.Sleep((int)ev.Value);
    }
};

Random rnd = new Random();
InternalMethod random = new InternalMethod("System.Random", "Returns a Random Value");
random.OnCall += (object _, InternalMethodCallEventArgs ev) => {
    if (ev.Value is int)
    {
        random.Return(rnd.Next((int)ev.Value));
        return;
    }
    random.Return(rnd.Next());
};

InternalMethod ilt = new InternalMethod("Int.LessThan", "Returns a Random Value");
ilt.OnCall += (object _, InternalMethodCallEventArgs ev) => {
    if (ev.Value is string)
    {
        string cmb = ev.Value as string;
        string v1 = cmb.Split('-')[0], v2 = cmb.Split('-')[1];
        ilt.Return((executor.variables.ContainsKey(v1) ? (int)executor.variables[v1] : int.Parse(v1)) < (executor.variables.ContainsKey(v2) ? (int)executor.variables[v2] : int.Parse(v2)));
    }
    ilt.Return(false);
};

executor.internalMethods.Add(print);
executor.internalMethods.Add(println);
executor.internalMethods.Add(ilt);
executor.internalMethods.Add(sleep);
executor.internalMethods.Add(random);
#endregion

#region Methods
executor.methods.Add(new Method("App.test1_1", new List<Instruction>()
{
    new Instruction(Instruction.OpCode.syscall, "System.Print", "ref :args:"),
    new Instruction(Instruction.OpCode.ret, null, "lol")
}));

executor.methods.Add(new Method("App.test1", new List<Instruction>()
{
    new Instruction(Instruction.OpCode.syscall, "System.Print", "ref :args:"),

    new Instruction(Instruction.OpCode.var, "retval", null),

    new Instruction(Instruction.OpCode.snr, "retval", null),
    new Instruction(Instruction.OpCode.call, "App.test1_1", 69),

    new Instruction(Instruction.OpCode.syscall, "System.Print", "ref retval"),

    new Instruction(Instruction.OpCode.snr, "retval", null),
    new Instruction(Instruction.OpCode.syscall, "System.Random", null),

    new Instruction(Instruction.OpCode.syscall, "System.Print", "ref retval"),
}));

executor.methods.Add(new Method("App.test2_1", new List<Instruction>()
{
    new Instruction(Instruction.OpCode.snr, "eq", null),
    new Instruction(Instruction.OpCode.syscall, "System.Random", null),
    new Instruction(Instruction.OpCode.syscall, "System.Print", "wrong password - "),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "ref eq"),
    new Instruction(Instruction.OpCode.syscall, "System.Sleep", 100),
    new Instruction(Instruction.OpCode.call, "App.test2_1", null)
}));

executor.methods.Add(new Method("App.test2", new List<Instruction>()
{
    new Instruction(Instruction.OpCode.syscall, "System.Print", "Input: "),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "ref :args:"),

    new Instruction(Instruction.OpCode.var, "eq", "ref :args:"),

    new Instruction(Instruction.OpCode.breql, "eq", "password"),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "logged in"),
    new Instruction(Instruction.OpCode.brend, null, null),

    new Instruction(Instruction.OpCode.brneql, "eq", "password"),
    new Instruction(Instruction.OpCode.call, "App.test2_1", null),
    new Instruction(Instruction.OpCode.brend, null, null)

}));

executor.methods.Add(new Method("App.test3", new List<Instruction>()
{
    new Instruction(Instruction.OpCode.var, "lt", "a"),

    new Instruction(Instruction.OpCode.breql, "lt", "a"),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "a"),
    new Instruction(Instruction.OpCode.brk, null, null),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "b"),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "c"),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "d"),
    new Instruction(Instruction.OpCode.syscall, "System.Println", "e"),
    new Instruction(Instruction.OpCode.brend, null, null),
}));
#endregion
*/
//Console.Write("Args: ");
//string arg = Console.ReadLine();
