using prometheus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace prometheus_loader
{
    public class load
    {
        public void exec(string path, string[] args)
        {
            Executor executor = new Executor();
            executor.AllowRedefinition = bool.Parse(Encoding.Unicode.GetString(Program.GetEmbeddedResource("AllowRedefinition")));
            executor.AutoRef = bool.Parse(Encoding.Unicode.GetString(Program.GetEmbeddedResource("AutoRef")));
            executor.AddMethodDefClass = bool.Parse(Encoding.Unicode.GetString(Program.GetEmbeddedResource("AddMethodDefClass")));
            try
            {
                foreach (string f in Directory.GetFiles(path))
                {
                    if (f.Split(Path.DirectorySeparatorChar)[f.Split(Path.DirectorySeparatorChar).Length -1].ToLower().StartsWith("pmi-") && f.ToLower().EndsWith(".dll"))
                    {
                        Assembly asm = Assembly.LoadFrom(f);
                        foreach (Type t in asm.GetTypes())
                        {
                            if (t.GetMethod("Inject") != null)
                            {
                                t.GetMethod("Inject", new Type[] { typeof(Executor) }).Invoke(this, new object[] { executor });
                            }
                        }
                    }
                }
                Application loaded = JsonHandler.ConvertToObj<Application>(Encoding.Unicode.GetString(Convert.FromBase64String(Encoding.Unicode.GetString(Program.GetEmbeddedResource("Source")))));
                //loaded.Methods[0].instructions.Insert(0, new Instruction(Instruction.OpCode.syscall, "System.Println", "uwu"));
                executor.Index(loaded);
                bool executed = false;
                foreach(Class c in loaded.Classes)
                {
                    if(c.Definition == loaded.EntryClass)
                    {
                        if (c.Methods == null) continue;
                        foreach(Method m in c.Methods)
                        {
                            if(m.Definition == loaded.EntryMethod)
                            {
                                if (!executor.AllowRedefinition && executed)
                                {
                                    Console.WriteLine("-->Method Redefinition blocked: " + m.Definition);
                                    break;
                                }

                                executor.Execute(m, args);
                                executed = true;
                            }
                        }
                    }
                }
                //executor.Execute(loaded, loaded.EntryClass, loaded.EntryMethod, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("---");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
