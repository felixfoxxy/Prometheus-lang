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
            nop, //Nothing
            var, //Initialize Variable
            set, //Set Variable
            snr, //Set Variable to next Return Value
            call, //Call Function
            syscall, //Call System Function
            breql, //Equal Branch
            brneql, //Not Equal branch
            brend, //Branch end
            jmp, //Jump to instruction
            brk, //Break
            add, //Add int to int and return output
            ret, //Returns a value and exits the function
            cast, //Returns the target casted to the type of the value
            sub, //Subtract target by value
            mul, //Multiply target by value
            div, //Divide target by value
            stfld, //Set field to value
            ldfld, //Get value of field
        }

        public OpCode opCode;
        public object Target;
        public object Value;

        /*public Instruction(OpCode opCode)
        {
            this.opCode = opCode;
            this.Target = null;
            this.Value = null;
        }

        public Instruction(OpCode opCode, object Target)
        {
            this.opCode = opCode;
            this.Target = Target;
            this.Value = null;
        }*/

        public Instruction(OpCode opCode, object Target, object Value) {
            this.opCode = opCode;
            this.Target = Target;
            this.Value = Value;
        }
    }
}
