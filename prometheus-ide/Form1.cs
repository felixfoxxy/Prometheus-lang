using Microsoft.VisualBasic;
using prometheus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

        public string ProjectDir = "";
        public string ProjectSourcePath = "";
        prometheus.Application app;

        public Form1()
        {
            InitializeComponent();
            resize();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            resize();
        }

        void resize()
        {
            treeView1.Height = this.Height - (treeView1.Location.Y + 51);
            dataGridView1.Height = this.Height - (dataGridView1.Location.Y + 51);
            dataGridView1.Width = this.Width - (dataGridView1.Location.X + 28);
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
            app = JsonHandler.ConvertToObj<prometheus.Application>(File.ReadAllText(path));
            TreeNode node = new TreeNode(app.Name);
            node.Tag = app;
            treeView1.Nodes.Add(node);
            foreach (Class c in app.Classes)
                loadClass(c, node);
        }

        public void reloadProject()
        {
            prometheus.Application app = treeView1.Nodes[0].Tag as prometheus.Application;
            treeView1.Nodes.Clear();
            TreeNode node = new TreeNode(app.Name);
            node.Tag = app;
            treeView1.Nodes.Add(node);
            foreach (Class c in app.Classes)
                loadClass(c, node);
        }

        public void loadClass(Class c, TreeNode pnode)
        {
            TreeNode node = new TreeNode(c.Definition);
            node.Tag = c;
            pnode.Nodes.Add(node);
            foreach (Method m in c.Methods)
                loadMethod(m, node);
        }

        public void loadMethod(Method m, TreeNode pnode)
        {
            TreeNode node = new TreeNode(m.Definition);
            node.Tag = m;
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
            Process cmp = Process.Start(GetOwnPath() + "prometheus-compile.exe", ProjectSourcePath + " " + outpath);
            while (!cmp.HasExited)
            {
                Thread.Sleep(100);
            }
        }

        void loadinsts()
        {
            dataGridView1.Rows.Clear();
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
                    dataGridView1.Rows.Add("🠵", "", "");
                    dataGridView1.Rows.Add("", "", "");
                }
            }
            dataGridView1.Tag = treeView1.SelectedNode.Tag;
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
                    Class nc = new Class(input, new List<Method>());
                    ((prometheus.Application)treeView1.SelectedNode.Tag).Classes.Add(nc);
                    reloadProject();
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
            if(e.Button == MouseButtons.Right)
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
            File.WriteAllText(ProjectSourcePath, JsonHandler.ConvertToString(treeView1.Nodes[0].Tag as prometheus.Application));
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
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
            compile(ProjectDir + Path.DirectorySeparatorChar + "build" + Path.DirectorySeparatorChar + "build.exe");
            Process.Start(ProjectDir + Path.DirectorySeparatorChar + "build" + Path.DirectorySeparatorChar + "build.exe");
        }

        private void packageToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
                    ((Method)dataGridView1.Tag).instructions.Insert(pos, new Instruction(Instruction.OpCode.nop, null, "asd"));
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach (Method meth in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (meth.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            int pos = meth.instructions.IndexOf(dataGridView1.SelectedRows[0].Tag as Instruction);
                            meth.instructions.Insert(pos, new Instruction(Instruction.OpCode.nop, null, "asd"));
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
                    ((Method)dataGridView1.Tag).instructions.Insert(pos + 1, new Instruction(Instruction.OpCode.nop, null, "sad"));
                }
                else if (dataGridView1.Tag is Class)
                {
                    foreach (Method meth in ((Class)dataGridView1.Tag).Methods)
                    {
                        if (meth.instructions.Contains(dataGridView1.SelectedRows[0].Tag as Instruction))
                        {
                            int pos = meth.instructions.IndexOf(dataGridView1.SelectedRows[0].Tag as Instruction);
                            meth.instructions.Insert(pos + 1, new Instruction(Instruction.OpCode.nop, null, "sad"));
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
