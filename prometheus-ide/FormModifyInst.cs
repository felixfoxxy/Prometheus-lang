using prometheus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prometheus_ide
{
    public partial class FormModifyInst : Form
    {
        public Instruction inst;
        public object value;

        public FormModifyInst(Instruction ins)
        {
            inst = ins;
            InitializeComponent();

            int opcode = 0;
            for (int i = 0; i < Enum.GetNames(typeof(Instruction.OpCode)).Length; i++) {
                comboBox1.Items.Add(Enum.GetNames(typeof(Instruction.OpCode))[i]);

                if ((Instruction.OpCode)i == inst.opCode)
                    opcode = i;
            }
            comboBox1.SelectedItem = comboBox1.Items[opcode];
            textBox1.Text = inst.Target as string;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            inst.opCode = (Instruction.OpCode)Enum.Parse(typeof(Instruction.OpCode), comboBox1.GetItemText(comboBox1.SelectedItem));
            inst.Target = textBox1.Text;
            inst.Value = value;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FormValueEditor fve = new FormValueEditor(inst.Value);
            fve.ShowDialog();
            if (fve.apply)
            {
                value = fve.obje;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
