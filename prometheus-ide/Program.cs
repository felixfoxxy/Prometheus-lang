using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prometheus_ide
{
    internal static class Program
    {
        public static string GetOwnPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
        }

        [STAThread]
        static void Main()
        {
            string libpath = GetOwnPath() + "libs";
            if(!Directory.Exists(libpath))
                Directory.CreateDirectory(libpath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
