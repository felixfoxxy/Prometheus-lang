using prometheus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WK.Libraries.BetterFolderBrowserNS;

namespace prometheus_ide
{
    public partial class FormNewProject : Form
    {
        public string ProjectDir = "";
        public string ProjectSourcePath = "";

        public FormNewProject()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BetterFolderBrowser bfb = new BetterFolderBrowser();
            bfb.Multiselect = false;
            bfb.RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            bfb.Title = "Select Project Parent Directory";
            if (bfb.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = bfb.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rootdir = textBox1.Text + Path.DirectorySeparatorChar + textBox2.Text;
            string buildpath = rootdir + Path.DirectorySeparatorChar + "build";
            string srcpath = rootdir + Path.DirectorySeparatorChar + textBox2.Text + ".json";
            Directory.CreateDirectory(rootdir);
            Directory.CreateDirectory(buildpath);
            prometheus.Application app = new prometheus.Application();
            app.Name = textBox2.Text;
            File.WriteAllText(srcpath, JsonHandler.ConvertToString(app));
            ProjectDir = rootdir;
            ProjectSourcePath = srcpath;
            this.Close();
        }
    }
}
