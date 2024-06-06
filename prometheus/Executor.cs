using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace prometheus
{
    public class Executor
    {
        //Config
        public bool AutoRef = true;
        public bool AllowRedefinition = false;

        //Language
        
        public List<InternalMethod> internalMethods = new List<InternalMethod>();
        
        //Dynamic
        public Instruction lastInstruction;
        public Dictionary<string, object> variables = new Dictionary<string, object>();
        public List<Method> methods = new List<Method>();

        public bool inBranch = false;
        public bool executeBranch = false;

        public object Execute(Method method, object args)
        {
            object ret = null;
            foreach (Instruction instruction in method.instructions)
            {
                object lret = Execute(instruction, args);
                if (lret != null)
                    ret = lret;
            }
            return ret;
        }

        public object Execute(List<Instruction> instructions, object args) {
            object ret = null;
            foreach (Instruction instruction in instructions)
            {
                object lret = Execute(instruction, args);
                if (lret != null)
                    ret = lret;
            }
            return ret;
        }

        public object Execute(Instruction instruction, object args) {
            if (instruction.opCode == Instruction.OpCode.brend)
            {
                inBranch = false;
                executeBranch = false;
                return null;
            }

            if (instruction.opCode == Instruction.OpCode.brk && inBranch)
            {
                executeBranch = false;
                return null;
            }

            if (inBranch && !executeBranch)
                return null;

            if (AutoRef && instruction.Value is string && (instruction.Value as string).StartsWith("ref "))
                instruction.Value = new Reference((instruction.Value as string).Substring(4));

            if (instruction.Value is Reference && (instruction.Value as Reference).Variable == ":args:")
                instruction.Value = args;

            object ret = null;

            switch (instruction.opCode)
            {
                case Instruction.OpCode.nop:
                    break;
                case Instruction.OpCode.var:
                    if (variables.ContainsKey(instruction.Target as string)) break;

                    variables.Add(instruction.Target as string, instruction.Value);
                    break;
                case Instruction.OpCode.set:
                    if (!variables.ContainsKey(instruction.Target as string)) break;
                    variables[instruction.Target as string] = instruction.Value;
                    break;
                case Instruction.OpCode.call:
                    bool call_executed = false;
                    foreach(Method me in methods)
                    {
                        if(instruction.Target as string == me.Definition)
                        {
                            if (!AllowRedefinition && call_executed)
                            {
                                Console.WriteLine("-->Method Redefinition blocked!");
                                break;
                            }
                            string snr = "";
                            if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                                snr = lastInstruction.Target as string;
                            ret = Execute(me.instructions, instruction.Value);
                            if(snr != "")
                            {
                                if (!variables.ContainsKey(snr as string)) break;
                                    variables[snr] = ret;
                            }
                            call_executed = true;
                        }
                    }
                    break;
                case Instruction.OpCode.syscall:
                    foreach(InternalMethod im in internalMethods)
                    {
                        if(instruction.Target as string == im.Definition)
                        {
                            string snr = "";
                            if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                                snr = lastInstruction.Target as string;
                            im.Call(instruction, instruction.Value);
                            ret = im.LastReturnValue;
                            if (snr != "")
                            {
                                if (!variables.ContainsKey(snr as string)) break;
                                variables[snr] = ret;
                            }
                            break;
                        }
                    }
                    break;
                case Instruction.OpCode.breql:
                    if (variables.ContainsKey(instruction.Target as string))
                    {
                        inBranch = true;
                        executeBranch = variables[instruction.Target as string] == instruction.Value;
                    }
                    break;
                case Instruction.OpCode.brneql:
                    if (variables.ContainsKey(instruction.Target as string))
                    {
                        inBranch = true;
                        executeBranch = variables[instruction.Target as string] != instruction.Value;
                    }
                    break;
                case Instruction.OpCode.jmp:
                    if(instruction.Target is string)
                    {
                        foreach (Method me in methods) {
                            if (me.Definition == (instruction.Target as string))
                            {
                                //TODO
                            }
                        }
                    }
                    break;
                case Instruction.OpCode.add:
                    if (instruction.Value is int && instruction.Target is string)
                    {
                        variables[instruction.Target as string] = (int)variables[instruction.Target as string] + (int)instruction.Value;
                    }
                    break;
                case Instruction.OpCode.ret:
                    ret = instruction.Value;
                    break;
                case Instruction.OpCode.snr:
                    break;
            }

            lastInstruction = instruction;
            //if (ret != null) Console.WriteLine("-->" + ret);
            return ret;
        }
    }
}
