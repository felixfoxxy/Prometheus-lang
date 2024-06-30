using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WK.Libraries.BetterFolderBrowserNS;

namespace prometheus_ide
{
    public partial class FormDecompile : Form
    {
        public FormDecompile()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Executables|*.exe|Screensavers|*.scr|Command Files|*.cmd|All Files|*.*";
            ofd.Title = "Assembly to decompile";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
                textBox1.Text = ofd.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BetterFolderBrowser bfb = new BetterFolderBrowser();
            bfb.Multiselect = false;
            bfb.RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            bfb.Title = "Select decompile Output Directory";
            if (bfb.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = bfb.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(Program.GetOwnPath() + "prometheus-decompile.exe", textBox1.Text + " " + textBox2.Text);
            this.Close();
        }
    }
}
