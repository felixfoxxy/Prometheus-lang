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
        public byte[] GetEmbeddedResource(string resourceName)
        {
            var self = System.Reflection.Assembly.GetExecutingAssembly();

            using (var rs = self.GetManifestResourceStream(resourceName))
            {
                if (rs != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        rs.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public void exec(string path, string[] args)
        {
            Executor executor = new Executor();
            executor.AllowRedefinition = false;

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
                Class loaded = JsonHandler.ConvertToObj<Class>(Encoding.Unicode.GetString(Convert.FromBase64String(Encoding.Unicode.GetString(GetEmbeddedResource("Source")))));
                //loaded.Methods[0].instructions.Insert(0, new Instruction(Instruction.OpCode.syscall, "System.Println", "uwu"));
                executor.Index(loaded);
                executor.Execute(loaded, "Main", args);
                Console.WriteLine("---Execution Finished---");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
