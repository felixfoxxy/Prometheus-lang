using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public class Instruction
    {
        public enum OpCode
        {
            Nop, //Nothing
            Var, //Initialize Variable
            Set, //Set Variable
            Snr, //Set Variable to next Return Value
            Call, //Call Function
            Syscall, //Call System Function
            Ret //Set Return Value
        }

        public OpCode opCode;
        public object Target;
        public object Value;

        public Instruction(OpCode opCode, object Target, object Value) {
            this.opCode = opCode;
            this.Target = Target;
            this.Value = Value;
        }
    }
}
