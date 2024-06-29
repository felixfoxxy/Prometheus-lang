using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnlib.DotNet;
using prometheus;
using WK.Libraries.BetterFolderBrowserNS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace prometheus_ide
{
    public partial class FormPackage : Form
    {
        public string path;

        public FormPackage(string _path)
        {
            path = _path;
            InitializeComponent();
            loadlibs();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadlibs();
        }

        void loadlibs()
        {
            checkedListBox1.Items.Clear();
            foreach (string f in Directory.EnumerateFiles(Program.GetOwnPath() + "libs"))
            {
                string fn = f.Split(Path.DirectorySeparatorChar)[f.Split(Path.DirectorySeparatorChar).Length - 1];
                if(fn.ToLower().StartsWith("pmi-") && fn.ToLower().EndsWith(".dll"))
                    checkedListBox1.Items.Add(fn);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CompilerOptions co = JsonHandler.ConvertToObj<CompilerOptions>(File.ReadAllText(path + "compiler.json"));

            string tdir = Program.GetOwnPath() + Path.GetRandomFileName();
            string tzip = Program.GetOwnPath() + Path.GetRandomFileName();
            Directory.CreateDirectory(tdir);

            File.Copy(co.BuildPath, tdir + Path.DirectorySeparatorChar + "entry.exe");
            File.Copy(Program.GetOwnPath() + "prometheus-lib.dll", tdir + Path.DirectorySeparatorChar + "prometheus-lib.dll");
            File.Copy(Program.GetOwnPath() + "Newtonsoft.Json.dll", tdir + Path.DirectorySeparatorChar + "Newtonsoft.Json.dll");

            foreach (var itm in checkedListBox1.CheckedItems)
            {
                string file = Program.GetOwnPath() + "libs" + Path.DirectorySeparatorChar + itm as string;
                File.Copy(file, tdir + Path.DirectorySeparatorChar + itm as string);
            }

            ZipFile.CreateFromDirectory(tdir, tzip);
            Directory.Delete(tdir, true);

            ModuleDefMD module = ModuleDefMD.Load(Program.GetOwnPath() + "unpack.bin");
            module.Resources.Add(new EmbeddedResource("Bundle", File.ReadAllBytes(tzip)));
            module.Write(textBox1.Text);
            if (!string.IsNullOrWhiteSpace(textBox2.Text))
                new IconChanger().ChangeIcon(textBox1.Text, textBox2.Text);
            File.Delete(tzip);
            MessageBox.Show("Project Packaged!", "Prometheus Instruction IDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Executables|*.exe|Screensavers|*.scr|Command Files|*.cmd|All Files|*.*";
            sfd.Title = "Package Output";
            if(sfd.ShowDialog() == DialogResult.OK)
                textBox1.Text = sfd.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Icons|*.ico|All Files|*.*";
            ofd.Title = "Package Icon";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
                textBox2.Text = ofd.FileName;
        }
    }
}
