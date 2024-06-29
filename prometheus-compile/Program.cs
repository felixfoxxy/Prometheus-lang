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
            if (args.Length >= 2)
            {
                try {
                    if (!File.Exists(args[0]))
                    {
                        Console.WriteLine("Invalid Source File!");
                        return;
                    }
                    DateTime start = DateTime.Now;

                    Application app = JsonHandler.ConvertToObj<Application>(File.ReadAllText(args[0]));
                    string enc = JsonHandler.ConvertToString(app);

                    ModuleDefMD module = ModuleDefMD.Load("loader.bin");
                    module.Resources.Add(new EmbeddedResource("AllowRedefinition", Encoding.Unicode.GetBytes(args.Contains("-redef").ToString())));
                    module.Resources.Add(new EmbeddedResource("AutoRef", Encoding.Unicode.GetBytes(args.Contains("-autoref").ToString())));
                    module.Resources.Add(new EmbeddedResource("AddMethodDefClass", Encoding.Unicode.GetBytes(args.Contains("-mdefc").ToString())));

                    module.Resources.Add(new EmbeddedResource("Source", Encoding.Unicode.GetBytes(Convert.ToBase64String(Encoding.Unicode.GetBytes(enc)))));
                    module.Resources.Add(new EmbeddedResource("Version", Encoding.Unicode.GetBytes("1_0")));
                    module.Resources.Add(new EmbeddedResource("Local", Encoding.Unicode.GetBytes(true.ToString())));
                    module.Write(args[1]);
                    DateTime end = DateTime.Now;
                    TimeSpan finish = end.Subtract(start);
                    Console.WriteLine("Compilation finished in " + finish.Hours + ":" + finish.Minutes + ":" + finish.Seconds + ":" + finish.Milliseconds);
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("---");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Usage: prometheus-compile [Source File] [Output File]");
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
