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

namespace prometheus_unpack
{
    internal class Program
    {
        public static string GetOwnExe()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static string GetOwnPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public static byte[] GetEmbeddedResource(string resourceName)
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

        static void Main(string[] args)
        {
            string runpath = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName() + Path.DirectorySeparatorChar;
            string zippath = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
            File.WriteAllBytes(zippath, GetEmbeddedResource("Bundle"));
            Directory.CreateDirectory(runpath);
            ZipFile.ExtractToDirectory(zippath, runpath);
            File.Delete(zippath);
            string a = "";
            foreach(string arg in args)
                a += arg;
            a = a.Trim();
            Process p = Process.Start(runpath + "entry.exe", a);
            while (!p.HasExited)
                Thread.Sleep(100);
            Directory.Delete(runpath, true);
        }
    }
}
