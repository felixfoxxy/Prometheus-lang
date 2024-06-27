using prometheus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prometheus_loader
{
    internal class Program
    {
        
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static void ExecuteAsAdmin(string fileName)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = fileName;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
            catch { }
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

        public static string prometheusPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + Path.DirectorySeparatorChar + "prometheus-" + Encoding.Unicode.GetString(GetEmbeddedResource("Version")) + Path.DirectorySeparatorChar + "prometheus.exe";

        public static string GetOwnExe()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static string GetOwnPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        static void Main(string[] args)
        {
            bool local = bool.Parse(Encoding.Unicode.GetString(GetEmbeddedResource("Local")));
            if (!local)
            {
                if (!File.Exists(prometheusPath)){
                    MessageBox.Show("This Application requires prometheus " + Encoding.Unicode.GetString(GetEmbeddedResource("Version")).Replace("_", "."), "Prometheus Error");
                    return;
                }

                Process.Start(prometheusPath, GetOwnExe());

            }
            else
                new load().exec(GetOwnPath(), args);
        }
    }
}
