using Microsoft.VisualBasic;
using prometheus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WK.Libraries.BetterFolderBrowserNS;

namespace prometheus_ide
{
    public partial class Form1 : Form
    {
        public string GetOwnPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
        }

        private bool tvpreventExpand = false;
        private DateTime tvlastMouseDown = DateTime.Now;

        public string cmppath = "";
        public string ProjectDir = "";
        public string ProjectSourcePath = "";
        prometheus.Application app = new prometheus.Application();
        CompilerOptions compilerOptions = new CompilerOptions();

        public Form1()
        {
            InitializeComponent();
            treeIcons.TransparentColor = Color.FromArgb(0, 128, 0);
            foreach (var res in Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true))
            {
                DictionaryEntry dres = (DictionaryEntry)res;
                if (dres.Key.ToString().StartsWith("Class_Browser"))
                {
                    treeIcons.Images.Add(dres.Key.ToString(), ((System.Drawing.Image)dres.Value));
                }
            }
            resize();
            reloadProject(false);
            updatelibs();
            //MessageBox.Show(JsonHandler.ConvertToString(Type.GetType("System.Int32")));
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            resize();
        }

        void resize()
        {
            treeView1.Height = this.Height - (treeView1.Location.Y + 51);
            tabControl1.Height = this.Height - (tabControl1.Location.Y + 51);
            tabControl1.Width = this.Width - (tabControl1.Location.X + 28);
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compile(ProjectDir + Path.DirectorySeparatorChar + "build" + Path.DirectorySeparatorChar + "build.exe");
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormNewProject fnp = new FormNewProject();
            fnp.ShowDialog();
            if(!string.IsNullOrWhiteSpace(fnp.ProjectDir) && !string.IsNullOrWhiteSpace(fnp.ProjectSourcePath))
            {
                ProjectDir = fnp.ProjectDir;
                ProjectSourcePath = fnp.ProjectSourcePath;
                openProject(fnp.ProjectSourcePath);
            }
        }

        #region ProjectLoader
        public void openProject(string path)
        {
            treeView1.Nodes.Clear();
            cmppath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + "compiler.json";
            app = JsonHandler.ConvertToObj<prometheus.Application>(File.ReadAllText(path));
            if(File.Exists(cmppath))
                compilerOptions = JsonHandler.ConvertToObj<CompilerOptions>(File.ReadAllText(cmppath));
            else
            {
                compilerOptions = new CompilerOptions();
                compilerOptions.BuildPath = ProjectDir + Path.DirectorySeparatorChar + "build" + Path.DirectorySeparatorChar + "build.exe";
                compilerOptions.CompilerPath = GetOwnPath() + "prometheus-compile.exe";
                File.WriteAllText(cmppath, JsonHandler.ConvertToString(compilerOptions));
            }
            reloadProject();
        }

        public void reloadProject(bool open = true)
        {
            if (open)
            {
                //app = treeView1.Nodes[0].Tag as prometheus.Application;
                treeView1.Nodes.Clear();
                TreeNode node = new TreeNode(app.Name);
                node.Tag = app;
                node.ImageKey = "Class_Browser16_151";
                node.SelectedImageKey = "Class_Browser16_151";
                treeView1.Nodes.Add(node);
                List<Class> cs = new List<Class>();
                foreach (Class c in app.Classes)
                    loadClass(c, node);
            }
            checkBox1.Checked = compilerOptions.AllowRedefinition;
            checkBox2.Checked = compilerOptions.AutoRef;
            checkBox3.Checked = compilerOptions.AddMethodDefClass;
            textBox1.Text = app.EntryClass;
            if(app.EntryMethod.Contains('.'))
                textBox2.Text = app.EntryMethod.Split('.')[app.EntryMethod.Split('.').Length -1];
            else
                textBox2.Text = app.EntryMethod;
            updatelibs();
        }

        public void loadClass(Class c, TreeNode pnode)
        {
            TreeNode node = new TreeNode(c.Definition);
            node.Tag = c;
            node.ImageKey = "Class_Browser16_7";
            node.SelectedImageKey = "Class_Browser16_7";
            pnode.Nodes.Add(node);
            foreach (Method m in c.Methods)
                loadMethod(m, node);
            if(c.Classes != null)
                foreach(Class sc in c.Classes)
                    loadClass(sc, node);
        }

        public void loadMethod(Method m, TreeNode pnode)
        {
            TreeNode node = new TreeNode(m.Definition);
            node.Tag = m;
            node.ImageKey = "Class_Browser16_61";
            node.SelectedImageKey = "Class_Browser16_61";
            pnode.Nodes.Add(node);
        }
        #endregion

        void compile(string outpath)
        {
            try
            {
                File.Delete(outpath);
            }
            catch (Exception) { }
            save();
            Process cmp = Process.Start(compilerOptions.CompilerPath, ProjectSourcePath + " " + outpath + (compilerOptions.AllowRedefinition ? " -redef" : "") + (compilerOptions.AutoRef ? " -autoref" : "") + (compilerOptions.AddMethodDefClass ? " -mdefc" : ""));
            while (!cmp.HasExited)
            {
                Thread.Sleep(100);
            }
        }

        void loadinsts()
        {
            dataGridView1.Rows.Clear();
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Tag is Method)
                {
                    Method meth = treeView1.SelectedNode.Tag as Method;
                    for (int i = 0; i < meth.instructions.Count; i++)
                    {
                        Instruction inst = meth.instructions[i];
                        dataGridView1.Rows.Add(i, inst.opCode.ToString(), inst.Target != null ? inst.Target.ToString() : "", inst.Value != null ? inst.Value.ToString() : "");
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].Tag = inst;
                    }
                }
                else if (treeView1.SelectedNode.Tag is Class)
                {
                    loadinstsc(treeView1.SelectedNode.Tag as Class, 0);
                }
                dataGridView1.Tag = treeView1.SelectedNode.Tag;
            }
        }

        void loadinstsc(Class c, int tab)
        {
            int spc = 5;
            int stab = tab * spc;
            //dataGridView1.Rows.Add("🠷", "", "", c.Definition);
            //dataGridView1.Rows[dataGridView1.Rows.Count - 1].Tag = c;

            //foreach (Class sc in c.Classes)
            //{
            //    loadinstsc(sc, tab + 1);
            //}

            //new string(' ', stab + 1)

            foreach (Method meth in (treeView1.SelectedNode.Tag as Class).Methods)
            {
                dataGridView1.Rows.Add("🠷", "", "", meth.Definition);
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Tag = meth;
                for (int i = 0; i < meth.instructions.Count; i++)
                {
                    Instruction inst = meth.instructions[i];
                    dataGridView1.Rows.Add(i, inst.opCode.ToString(), inst.Target != null ? inst.Target.ToString() : "", inst.Value != null ? inst.Value.ToString() : "");
                    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Tag = inst;
                }
                dataGridView1.Rows.Add("🠵", "", "", "");
                if((treeView1.SelectedNode.Tag as Class).Methods.IndexOf(meth) != (treeView1.SelectedNode.Tag as Class).Methods.Count -1)
                    dataGridView1.Rows.Add("", "", "", "");
            }
            //dataGridView1.Rows.Add("🠵", "", "", c.Definition);
        }

        void stylegrid()
        {
            DataGridViewCellStyle offset = new DataGridViewCellStyle();
            offset.BackColor = Color.Beige;
            offset.ForeColor = Color.Blue;
            offset.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewCellStyle pos = new DataGridViewCellStyle();
            pos.BackColor = Color.Beige;
            pos.ForeColor = Color.Magenta;
            pos.Alignment = DataGridViewContentAlignment.MiddleLeft;
            DataGridViewCellStyle opcode = new DataGridViewCellStyle();
            opcode.BackColor = Color.Beige;
            opcode.ForeColor = Color.Green;
            opcode.Alignment = DataGridViewContentAlignment.MiddleLeft;
            DataGridViewCellStyle operand = new DataGridViewCellStyle();
            operand.BackColor = Color.Beige;
            operand.ForeColor = Color.DarkMagenta;
            operand.Alignment = DataGridViewContentAlignment.MiddleLeft;
            DataGridViewCellStyle method = new DataGridViewCellStyle();
            method.BackColor = Color.Beige;
            method.ForeColor = Color.Black;
            method.Alignment = DataGridViewContentAlignment.MiddleLeft;


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells[0].Style = offset;
                dataGridView1.Rows[i].Cells[1].Style = pos;
                dataGridView1.Rows[i].Cells[2].Style = opcode;
                dataGridView1.Rows[i].Cells[3].Style = operand;
            }
        }

        private void classToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is prometheus.Application)
            {
                string input = Interaction.InputBox("Class Name", "Prometheus Instruction IDE");
                if (!string.IsNullOrWhiteSpace(input))
                {
                    Class nc = new Class(input, new List<Method>(), new List<Class>());
                    ((prometheus.Application)treeView1.SelectedNode.Tag).Classes.Add(nc);
                    reloadProject();
                    loadinsts();
                    stylegrid();
                }
            }
            else if (treeView1.SelectedNode.Tag is Class)
            {
                string input = Interaction.InputBox("Class Name", "Prometheus Instruction IDE");
                if (!string.IsNullOrWhiteSpace(input))
                {
                    Class nc = new Class(input, new List<Method>(), new List<Class>());
                    if (((Class)treeView1.SelectedNode.Tag).Classes == null)
                        ((Class)treeView1.SelectedNode.Tag).Classes = new List<Class>();
                    ((Class)treeView1.SelectedNode.Tag).Classes.Add(nc);
                    reloadProject();
                    loadinsts();
                    stylegrid();
                }
            }
        }

        private void methodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Class)
            {
                string input = Interaction.InputBox("Method Name", "Prometheus Instruction IDE");
                if (!string.IsNullOrWhiteSpace(input))
                {
                    Method nm = new Method(input, new List<Instruction>() { new Instruction(Instruction.OpCode.nop, null, null) });
                    ((Class)treeView1.SelectedNode.Tag).Methods.Add(nm);
                    reloadProject();
                    loadinsts();
                    stylegrid();
                }
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            loadinsts();
            stylegrid();
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            int delta = (int)DateTime.Now.Subtract(tvlastMouseDown).TotalMilliseconds;
            tvpreventExpand = (delta < SystemInformation.DoubleClickTime);
            tvlastMouseDown = DateTime.Now;

            if (e.Button == MouseButtons.Right)
            {
                if(treeView1.SelectedNode != null)
                {
                    contextTree.Show(Cursor.Position);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        void save()
        {
            if (!string.IsNullOrWhiteSpace(ProjectSourcePath))
            {
                File.WriteAllText(ProjectSourcePath, JsonHandler.ConvertToString(treeView1.Nodes[0].Tag as prometheus.Application));
                MessageBox.Show("Project Saved!", "Prometheus Instruction IDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                ProjectDir = Path.GetDirectoryName(ofd.FileName);
                ProjectSourcePath = ofd.FileName;
                openProject(ofd.FileName);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ProjectDir = "";
            ProjectSourcePath = "";
            cmppath = "";
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compile(compilerOptions.BuildPath);
            Process.Start(compilerOptions.BuildPath);
        }

        private void packageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            compile(compilerOptions.BuildPath);
            new FormPackage(ProjectDir + Path.DirectorySeparatorChar).ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                if (dataGridView1.Tag is Method)
                {
                    ((Method)dataGridView1.Tag).instructions.Remove(dataGridView1.SelectedRows[0].Tag as Instruction);
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach(Method meth in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (meth.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            meth.instructions.Remove(dataGridView1.SelectedRows[0].Tag as Instruction);
                        }
                    }
                }
            }
            loadinsts();
            stylegrid();
        }

        private void modifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                new FormModifyInst(dataGridView1.SelectedRows[0].Tag as Instruction).ShowDialog();
                loadinsts();
                stylegrid();
            }
        }

        private void beforeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                if (dataGridView1.Tag is Method)
                {
                    int pos = ((Method)dataGridView1.Tag).instructions.IndexOf(dataGridView1.SelectedRows[0].Tag as Instruction);
                    ((Method)dataGridView1.Tag).instructions.Insert(pos, new Instruction(Instruction.OpCode.nop, null, null));
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach (Method meth in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (meth.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            int pos = meth.instructions.IndexOf(dataGridView1.SelectedRows[0].Tag as Instruction);
                            meth.instructions.Insert(pos, new Instruction(Instruction.OpCode.nop, null, null));
                        }
                    }
                }
            }
            loadinsts();
            stylegrid();
        }

        private void afterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                if (dataGridView1.Tag is Method)
                {
                    int pos = ((Method)dataGridView1.Tag).instructions.IndexOf(dataGridView1.SelectedRows[0].Tag as Instruction);
                    ((Method)dataGridView1.Tag).instructions.Insert(pos + 1, new Instruction(Instruction.OpCode.nop, null, null));
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach (Method meth in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (meth.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            int pos = meth.instructions.IndexOf(dataGridView1.SelectedRows[0].Tag as Instruction);
                            meth.instructions.Insert(pos + 1, new Instruction(Instruction.OpCode.nop, null, null));
                        }
                    }
                }
            }
            loadinsts();
            stylegrid();
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                if (dataGridView1.SelectedRows[0].Tag is Instruction)
                {
                    contextInst.Show(Cursor.Position);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            app.EntryClass = textBox1.Text;
            app.EntryMethod = checkBox3.Checked ? textBox1.Text + "." + textBox2.Text : textBox2.Text;
        }

        private void upToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                ((Method)dataGridView1.Tag).instructions.Insert((int)dataGridView1.SelectedRows[0].Cells[0].Value -1, (Instruction)dataGridView1.SelectedRows[0].Tag);
                ((Method)dataGridView1.Tag).instructions.RemoveAt((int)dataGridView1.SelectedRows[0].Cells[0].Value + 1);
                loadinsts();
                stylegrid();
            }
        }

        private void downToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                ((Method)dataGridView1.Tag).instructions.Insert((int)dataGridView1.SelectedRows[0].Cells[0].Value + 2, (Instruction)dataGridView1.SelectedRows[0].Tag);
                ((Method)dataGridView1.Tag).instructions.RemoveAt((int)dataGridView1.SelectedRows[0].Cells[0].Value);
                loadinsts();
                stylegrid();
            }
        }

        void delclass(Class c)
        {
            if (c.Classes.Contains(treeView1.SelectedNode.Tag as Class))
                c.Classes.Remove(treeView1.SelectedNode.Tag as Class);
            else
                foreach (Class sc in c.Classes)
                    delclass(sc);
        }

        void delmeth(Class c)
        {
            if (c.Methods.Contains(treeView1.SelectedNode.Tag as Method))
                c.Methods.Remove(treeView1.SelectedNode.Tag as Method);
            else
                foreach (Class sc in c.Classes)
                    delmeth(sc);
        }

        List<Class> getClasses(Class c)
        {
            List<Class> classes = new List<Class>();
            foreach(Class sc in c.Classes)
                classes.Add(sc);
            return classes;
        }

        private void confirmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode != null)
            {
                if(treeView1.SelectedNode.Tag is Class)
                {
                    //List<Class> cs = new List<Class>();
                    foreach (Class c in app.Classes)
                        delclass(c);
                }
                else if (treeView1.SelectedNode.Tag is Method)
                {
                    //List<Class> cs = new List<Class>();
                    foreach (Class c in app.Classes)
                        delmeth(c);
                    
                }
                reloadProject();
                loadinsts();
                stylegrid();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            compilerOptions.AllowRedefinition = checkBox1.Checked;
            compilerOptions.AutoRef = checkBox2.Checked;
            compilerOptions.AddMethodDefClass = checkBox3.Checked;
            File.WriteAllText(cmppath, JsonHandler.ConvertToString(compilerOptions));
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = tvpreventExpand;
            tvpreventExpand = false;
        }

        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = tvpreventExpand;
            tvpreventExpand = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string pp = Path.GetDirectoryName(compilerOptions.BuildPath) + Path.DirectorySeparatorChar + "prometheus-lib.dll";
            string np = Path.GetDirectoryName(compilerOptions.BuildPath) + Path.DirectorySeparatorChar + "Newtonsoft.Json.dll";
            
            if(File.Exists(pp))
                File.Delete(pp);
            if(File.Exists(np))
                File.Delete(np);

            File.Copy(Program.GetOwnPath() + "prometheus-lib.dll", pp);
            File.Copy(Program.GetOwnPath() + "Newtonsoft.Json.dll", np);
            
            MessageBox.Show("Added prometheus-lib to build Directory!", "Prometheus Instruction IDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            updatelibs();
        }

        void updatelibs()
        {
            checkedListBox1.Items.Clear();
            foreach (string f in Directory.EnumerateFiles(Program.GetOwnPath() + "libs"))
            {
                string fn = f.Split(Path.DirectorySeparatorChar)[f.Split(Path.DirectorySeparatorChar).Length - 1];
                
                if (!(fn.ToLower().StartsWith("pmi-") && fn.ToLower().EndsWith(".dll")))
                    continue;

                checkedListBox1.Items.Add(fn);
                if (File.Exists(compilerOptions.BuildPath))
                    if(File.Exists(Path.GetDirectoryName(compilerOptions.BuildPath) + Path.DirectorySeparatorChar + fn))
                        checkedListBox1.SetItemChecked(checkedListBox1.Items.Count - 1, true);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                string fn = checkedListBox1.Items[i] as string;
                if (checkedListBox1.GetItemChecked(i))
                {
                    string np = Path.GetDirectoryName(compilerOptions.BuildPath) + Path.DirectorySeparatorChar + fn;
                    if (File.Exists(np))
                        File.Delete(np);
                    File.Copy(GetOwnPath() + "libs" + Path.DirectorySeparatorChar + fn, np);
                }
                else
                {
                    string np = Path.GetDirectoryName(compilerOptions.BuildPath) + Path.DirectorySeparatorChar + fn;
                    if (File.Exists(np))
                        File.Delete(np);
                }
            }
            updatelibs();
            MessageBox.Show("Changed Libraries in build Directory!", "Prometheus Instruction IDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                Clipboard.SetText(JsonHandler.ConvertToString(dataGridView1.SelectedRows[0].Tag as Instruction));
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                Clipboard.SetText(JsonHandler.ConvertToString(dataGridView1.SelectedRows[0].Tag as Instruction));
                if (dataGridView1.Tag is Method)
                {
                    ((Method)dataGridView1.Tag).instructions.Remove(dataGridView1.SelectedRows[0].Tag as Instruction);
                }
                else if(dataGridView1.Tag is Class)
                {
                    foreach(Method m in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (m.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            m.instructions.Remove(dataGridView1.SelectedRows[0].Tag as Instruction);
                            break;
                        }
                    }
                }
                loadinsts();
                stylegrid();
            }
        }

        private void beforeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                Instruction ins = (JsonHandler.ConvertToObj<Instruction>(Clipboard.GetText()));
                if (dataGridView1.Tag is Method)
                {
                    ((Method)dataGridView1.Tag).instructions.Insert(((Method)dataGridView1.Tag).instructions.IndexOf((Instruction)dataGridView1.SelectedRows[0].Tag), ins);
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach (Method m in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (m.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            m.instructions.Insert(m.instructions.IndexOf((Instruction)dataGridView1.SelectedRows[0].Tag), ins);
                            break;
                        }
                    }
                }
                loadinsts();
                stylegrid();
            }
        }

        private void afterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows[0].Tag is Instruction)
            {
                Instruction ins = (JsonHandler.ConvertToObj<Instruction>(Clipboard.GetText()));
                if (dataGridView1.Tag is Method)
                {
                    ((Method)dataGridView1.Tag).instructions.Insert(((Method)dataGridView1.Tag).instructions.IndexOf((Instruction)dataGridView1.SelectedRows[0].Tag) + 1, ins);
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach (Method m in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (m.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            m.instructions.Insert(m.instructions.IndexOf((Instruction)dataGridView1.SelectedRows[0].Tag) + 1, ins);
                            break;
                        }
                    }
                }
                loadinsts();
                stylegrid();
            }
        }

        private void moveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
        }
    }

    class DataGridViewLabelCell : DataGridViewTextBoxCell
    {
        protected override void Paint(Graphics graphics,
                                      Rectangle clipBounds,
                                      Rectangle cellBounds,
                                      int rowIndex,
                                      DataGridViewElementStates cellState,
                                      object value,
                                      object formattedValue,
                                      string errorText,
                                      DataGridViewCellStyle cellStyle,
                                      DataGridViewAdvancedBorderStyle advancedBorderStyle,
                                      DataGridViewPaintParts paintParts)
        {
            // Call the base class method to paint the default cell appearance.
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
                value, formattedValue, errorText, cellStyle,
                advancedBorderStyle, paintParts);

            if (base.Tag != null)
            {
                string tag = base.Tag.ToString();
                Point point = new Point(base.ContentBounds.Location.X, base.ContentBounds.Location.Y);
                graphics.DrawString(tag, new Font("Arial", 7.0F), new SolidBrush(Color.Red), cellBounds.X + cellBounds.Width - 15, cellBounds.Y);
            }
        }
    }
    public class DataGridViewLabelCellColumn : DataGridViewColumn
    {
        public DataGridViewLabelCellColumn()
        {
            this.CellTemplate = new DataGridViewLabelCell();
        }
    }
}
