using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace prometheus_decompile
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 2)
            {
                string f = args[0];
                string p = args[1];

                if(!p.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    p += Path.DirectorySeparatorChar;

                if (!File.Exists(f))
                {
                    Console.WriteLine("Invalid Input File!");
                    return;
                }
                if (!Directory.Exists(p))
                {
                    Console.WriteLine("Invalid Output Directory!");
                    return;
                }

                try
                {
                    Console.WriteLine("Loading Assembly...");
                    Assembly asm = Assembly.LoadFile(f);
                    if (asm.GetManifestResourceNames().Contains("Bundle"))
                    {
                        Console.WriteLine("Assembly is Packed!");
                        bundle(asm, p);
                    }
                    else if (asm.GetManifestResourceNames().Contains("Source"))
                    {
                        Console.WriteLine("Assembly is not Packed!");
                        loader(asm, p);
                    }
                    Console.WriteLine("Decompilation finished!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("---");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Usage: prometheus-decompile.exe [Application] [Output Directory]");
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void bundle(Assembly asm, string op)
        {
            Console.WriteLine("Getting Bundle...");
            var stream = asm.GetManifestResourceStream("Bundle");
            byte[] bundle = new byte[stream.Length];
            stream.Read(bundle, 0, bundle.Length);
            stream.Close();

            Console.WriteLine("Writing Bundle to disk...");
            File.WriteAllBytes(op + "Bundle.zip", bundle);
            Console.WriteLine("Extracting Bundle...");
            ZipFile.ExtractToDirectory(op + "Bundle.zip", op);
            Console.WriteLine("Loading unpacked Assembly...");
            GC.Collect();
            Thread.Sleep(1000);
            Process.Start(Assembly.GetExecutingAssembly().Location, op + "entry.exe " + op);
            Environment.Exit(0);
        }

        static void loader(Assembly asm, string op)
        {
            Console.WriteLine("Getting Source...");
            var stream = asm.GetManifestResourceStream("Source");
            byte[] src = new byte[stream.Length];
            stream.Read(src, 0, src.Length);
            stream.Close();
            Console.WriteLine("Writing Base64 encoded Source to disk...");
            File.WriteAllBytes(op + "Source_Base64.txt", src);
            Console.WriteLine("Writing Source to disk...");
            cflags(asm, op);
            File.WriteAllText(op + "Source.json", Encoding.Unicode.GetString(Convert.FromBase64String(Encoding.Unicode.GetString(src))));
        }

        static void cflags(Assembly asm, string op)
        {
            try
            {
                Console.WriteLine("Getting Compiler Flags...");
                Console.WriteLine("Getting AddMethodDefClass...");
                var mdefc = asm.GetManifestResourceStream("AddMethodDefClass");
                byte[] mdefc_src = new byte[mdefc.Length];
                mdefc.Read(mdefc_src, 0, mdefc_src.Length);
                Console.WriteLine("Getting AllowRedefinition...");
                var aredef = asm.GetManifestResourceStream("AllowRedefinition");
                byte[] aredef_src = new byte[aredef.Length];
                aredef.Read(aredef_src, 0, aredef_src.Length);
                Console.WriteLine("Getting AutoRef...");
                var aref = asm.GetManifestResourceStream("AutoRef");
                byte[] aref_src = new byte[aref.Length];
                aref.Read(aref_src, 0, aref_src.Length);

                Console.WriteLine("Writing Values...");
                bool b_mdefc = bool.Parse(Encoding.Unicode.GetString(mdefc_src));
                bool b_aredef = bool.Parse(Encoding.Unicode.GetString(aredef_src));
                bool b_aref = bool.Parse(Encoding.Unicode.GetString(aref_src));

                File.WriteAllText(op + "CompilerFlags.txt", "AddMethodDefClass=" + b_mdefc.ToString() + Environment.NewLine + "AllowRedefinition=" + b_aredef.ToString() + Environment.NewLine + "AutoRef=" + b_aref.ToString());

                mdefc.Close();
                aredef.Close();
                aref.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("---");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
