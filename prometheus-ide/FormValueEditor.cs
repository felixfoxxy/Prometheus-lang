using prometheus;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prometheus_ide
{
    public partial class FormValueEditor : Form
    {
        public object obje;
        public bool apply = false;

        public FormValueEditor(object o)
        {
            obje = o;
            InitializeComponent();
            richTextBox1.Text = JsonHandler.ConvertToString(obje);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                obje = JsonHandler.ConvertToObj<object>(richTextBox1.Text);
                apply = true;
                this.Close();
            }catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Prometheus Instruction IDE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            obje = Type.GetType(textBox1.Text).GetDefaultValue();
            richTextBox1.Text = JsonHandler.ConvertToString(obje);
        }
    }
    static class ext
    {
        public static object GetDefaultValue(this Type type)
        {
            // Validate parameters.
            if (type == null) throw new ArgumentNullException("type");

            // We want an Func<object> which returns the default.
            // Create that expression here.
            Expression<Func<object>> e = Expression.Lambda<Func<object>>(
                // Have to convert to object.
                Expression.Convert(
                    // The default value, always get what the *code* tells us.
                    Expression.Default(type), typeof(object)
                )
            );

            // Compile and return the value.
            return e.Compile()();
        }
        public static T GetDefaultValue<T>()
        {
            // We want an Func<T> which returns the default.
            // Create that expression here.
            Expression<Func<T>> e = Expression.Lambda<Func<T>>(
                // The default value, always get what the *code* tells us.
                Expression.Default(typeof(T))
            );

            // Compile and return the value.
            return e.Compile()();
        }
    }
}
