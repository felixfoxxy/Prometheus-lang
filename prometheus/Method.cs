using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public class InternalMethodCallEventArgs : EventArgs
    {
        public Instruction Where { get; private set; }
        public object Value { get; private set; }

        public InternalMethodCallEventArgs(Instruction Where, object Value) {
            this.Where = Where;
            this.Value = Value;
        }
    }
    public class InternalMethod
    {
        public string Definition = "";
        public string Description = "";

        public object LastReturnValue = null;

        public delegate void InternalMethodCallEventHandler(object sender, InternalMethodCallEventArgs e);
        public event InternalMethodCallEventHandler OnCall;

        public InternalMethod(string Definition, string Description) {
            this.Definition = Definition;
            this.Description = Description;
        }

        public void Return(object value)
        {
            LastReturnValue = value;
        }

        public void Call(Instruction Where, object Value)
        {
            if (OnCall == null) return;

            InternalMethodCallEventArgs args = new InternalMethodCallEventArgs(Where, Value);
            OnCall(this, args);
        }
    }

    public class Method
    {
        public string Definition;
        public List<Instruction> instructions;

        public Method(string Definition, List<Instruction> instructions) {
            this.Definition= Definition;
            this.instructions = instructions;
        }
    }
}
