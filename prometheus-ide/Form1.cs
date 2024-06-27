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
            Process.Start(GetOwnPath() + "prometheus-compile.exe", "");
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
            prometheus.Application app = JsonHandler.ConvertToObj<prometheus.Application>(File.ReadAllText(path));
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

        private void classToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is prometheus.Application)
            {
                Class nc = new Class("new-class", new List<Method>());
                ((prometheus.Application)treeView1.SelectedNode.Tag).Classes.Add(nc);
                reloadProject();
            }
        }

        private void methodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Class)
            {
                Method nm = new Method("new-method", new List<Instruction>() { new Instruction(Instruction.OpCode.nop, null, null) });
                ((Class)treeView1.SelectedNode.Tag).Methods.Add(nm);
                reloadProject();
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            dataGridView1.Rows.Clear();
            if (treeView1.SelectedNode.Tag is Method)
            {
                Method meth = treeView1.SelectedNode.Tag as Method;
                for(int i = 0; i < meth.instructions.Count; i++)
                {
                    Instruction inst = meth.instructions[i];
                    dataGridView1.Rows.Add(i, inst.opCode.ToString(), inst.Target != null ? inst.Target.ToString() : "", inst.Value != null ? inst.Value.ToString() : "");
                }
            }
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
