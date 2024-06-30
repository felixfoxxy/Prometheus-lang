using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public bool AddMethodDefClass = true;

        //Language
        
        public List<InternalMethod> internalMethods = new List<InternalMethod>();
        
        //Dynamic
        public Instruction lastInstruction;
        public Dictionary<string, object> variables = new Dictionary<string, object>();
        public List<Method> methods = new List<Method>();

        public bool inBranch = false;
        public bool executeBranch = false;

        public void Index(Application a)
        {
            foreach (Class c in a.Classes)
            {
                Index(c);
            }
        }

        public void Index(Class c)
        {
            if (c.Methods != null)
            {
                foreach (Method meth in c.Methods)
                {
                    if (AddMethodDefClass)
                        Index(meth, c);
                    else
                        Index(meth);
                }
            }
            if (c.Classes != null)
            {
                foreach (Class sc in c.Classes)
                {
                    if (AddMethodDefClass)
                        Index(sc, c);
                    else
                        Index(sc);
                }
            }
        }

        public void Index(Class c, Class pc)
        {
            if (AddMethodDefClass)
                c.Definition = pc.Definition + "." + c.Definition;
            if (c.Methods != null)
            {
                foreach (Method meth in c.Methods)
                {
                    if (AddMethodDefClass)
                        Index(meth, c);
                    else
                        Index(meth);
                }
            }
            if (c.Classes != null)
            {
                foreach (Class sc in c.Classes)
                {
                    Index(sc, c);
                }
            }
        }

        public void Index(Method meth, Class c)
        {
            if (AddMethodDefClass)
                meth.Definition = c.Definition + "." + meth.Definition;

            if (AllowRedefinition)
            {
                methods.Add(meth);
            }
            else
            {
                if (!methods.Contains(meth))
                    methods.Add(meth);
                else
                    Console.WriteLine("-->Index Redefinition blocked: " + meth.Definition);
            }
        }

        public void Index(Method meth)
        {
            if (AllowRedefinition)
            {
                methods.Add(meth);
            }
            else
            {
                if (!methods.Contains(meth))
                    methods.Add(meth);
            }
        }

        public object Execute(Application app, string cls, string def, object args)
        {
            object ret = null;
            foreach (Class c in app.Classes)
            {

            }
            return ret;
        }

        public object Execute(Class c, string def, object args)
        {
            object ret = null;
            bool found = false;
            foreach (Method meth in c.Methods)
            {
                if(meth.Definition == def)
                {
                    if (!AllowRedefinition && found)
                    {
                        Console.WriteLine("-->Method Redefinition blocked: " + meth.Definition);
                        break;
                    }

                    object lret = Execute(meth, args);
                    if (lret != null)
                        ret = lret;
                    found = true;
                }
            }
            if (!found)
            {
                foreach (Class sc in c.Classes)
                    ret = Execute(sc, def, args);
            }
            return ret;
        }

        public object Execute(Method method, object args)
        {
            object ret = null;
            bool r = false;
            foreach (Instruction instruction in method.instructions)
            {
                object lret = Execute(instruction, args, out r);
                if (lret != null)
                    ret = lret;
                if (r)
                    return ret;
            }
            return ret;
        }

        public object Execute(List<Instruction> instructions, object args) {
            object ret = null;
            bool r = false;
            foreach (Instruction instruction in instructions)
            {
                object lret = Execute(instruction, args, out r);
                if (lret != null)
                    ret = lret;
                if (r)
                    return ret;
            }
            return ret;
        }

        public object Execute(Instruction instruction, object args, out bool returned) {
            returned = false;
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

            if (AutoRef && instruction.Value is string && (instruction.Value as string).StartsWith(":[ref]: "))
                instruction.Value = new Reference((instruction.Value as string).Substring(":[ref]: ".Length));

            if (instruction.Value is Reference && (instruction.Value as Reference).Variable == ":[args]:")
                instruction.Value = args;

            object ret = null;

            switch (instruction.opCode)
            {
                case Instruction.OpCode.nop:
                    break;
                case Instruction.OpCode.var:
                    if (variables.ContainsKey(instruction.Target as string) && !AllowRedefinition) break;

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
                                Console.WriteLine("-->Method Redefinition blocked: " + instruction.Target);
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
                    if(!call_executed)
                        Console.WriteLine("-->Method not found: " + instruction.Target);
                    break;
                case Instruction.OpCode.syscall:
                    bool syscall_executed = false;
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
                            syscall_executed = true;
                            break;
                        }
                    }
                    if(!syscall_executed)
                        Console.WriteLine("-->Missing Syscall: " + instruction.Target as string);
                    break;
                case Instruction.OpCode.breql:
                    if (variables.ContainsKey(instruction.Target as string))
                    {
                        if (instruction.Value is Reference)
                        {
                            inBranch = true;
                            executeBranch = JsonHandler.ConvertToString(variables[instruction.Target as string]) == JsonHandler.ConvertToString(variables[((Reference)instruction.Value).Variable]);
                        }
                        else
                        {
                            inBranch = true;
                            executeBranch = JsonHandler.ConvertToString(variables[instruction.Target as string]) == JsonHandler.ConvertToString(instruction.Value);
                        }
                    }
                    break;
                case Instruction.OpCode.brneql:
                    if (variables.ContainsKey(instruction.Target as string))
                    {
                        if (instruction.Value is Reference)
                        {
                            inBranch = true;
                            executeBranch = JsonHandler.ConvertToString(variables[instruction.Target as string]) != JsonHandler.ConvertToString(variables[((Reference)instruction.Value).Variable]);
                        }
                        else
                        {
                            inBranch = true;
                            executeBranch = JsonHandler.ConvertToString(variables[instruction.Target as string]) != JsonHandler.ConvertToString(instruction.Value);
                        }
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
                    {
                        string asnr = "";
                        if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                            asnr = lastInstruction.Target as string;

                        if (int.TryParse(instruction.Value.ToString(), out _))
                        {
                            ret = int.Parse(variables[instruction.Target as string].ToString()) + int.Parse(instruction.Value.ToString());
                        }
                        else
                        {
                            if (instruction.Target is string)
                            {
                                if (instruction.Value is Reference)
                                {
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) + int.Parse(variables[(instruction.Value as Reference).Variable].ToString());
                                }
                                else
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) + int.Parse(instruction.Value.ToString());
                            }
                        }

                        if (asnr != "")
                        {
                            if (!variables.ContainsKey(asnr as string)) break;
                            variables[asnr] = ret;
                        }
                    }
                    break;
                case Instruction.OpCode.ret:
                    ret = instruction.Value;
                    returned = true;
                    break;
                case Instruction.OpCode.snr:
                    break;
                case Instruction.OpCode.cast: //fix
                    if(instruction.Value is string)
                    {
                        if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                        {
                            Type t = Type.GetType(instruction.Value as string);
                            variables[lastInstruction.Target as string] = Convert.ChangeType(variables[instruction.Target as string], t);
                        }
                    }
                    break;
                case Instruction.OpCode.sub:
                    {
                        string asnr = "";
                        if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                            asnr = lastInstruction.Target as string;

                        if (int.TryParse(instruction.Value.ToString(), out _))
                        {
                            ret = int.Parse(variables[instruction.Target as string].ToString()) - int.Parse(instruction.Value.ToString());
                        }
                        else
                        {
                            if (instruction.Target is string)
                            {
                                if (instruction.Value is Reference)
                                {
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) - int.Parse(variables[(instruction.Value as Reference).Variable].ToString());
                                }
                                else
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) - int.Parse(instruction.Value.ToString());
                            }
                        }

                        if (asnr != "")
                        {
                            if (!variables.ContainsKey(asnr as string)) break;
                            variables[asnr] = ret;
                        }
                    }
                    break;
                case Instruction.OpCode.mul:
                    {
                        string asnr = "";
                        if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                            asnr = lastInstruction.Target as string;

                        if (int.TryParse(instruction.Value.ToString(), out _))
                        {
                            ret = int.Parse(variables[instruction.Target as string].ToString()) * int.Parse(instruction.Value.ToString());
                        }
                        else
                        {
                            if (instruction.Target is string)
                            {
                                if (instruction.Value is Reference)
                                {
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) * int.Parse(variables[(instruction.Value as Reference).Variable].ToString());
                                }
                                else
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) * int.Parse(instruction.Value.ToString());
                            }
                        }

                        if (asnr != "")
                        {
                            if (!variables.ContainsKey(asnr as string)) break;
                            variables[asnr] = ret;
                        }
                    }
                    break;
                case Instruction.OpCode.div:
                    {
                        string asnr = "";
                        if (lastInstruction != null && lastInstruction.opCode == Instruction.OpCode.snr)
                            asnr = lastInstruction.Target as string;

                        if (int.TryParse(instruction.Value.ToString(), out _))
                        {
                            ret = int.Parse(variables[instruction.Target as string].ToString()) / int.Parse(instruction.Value.ToString());
                        }
                        else
                        {
                            if (instruction.Target is string)
                            {
                                if (instruction.Value is Reference)
                                {
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) / int.Parse(variables[(instruction.Value as Reference).Variable].ToString());
                                }
                                else
                                    ret = int.Parse(variables[instruction.Target as string].ToString()) / int.Parse(instruction.Value.ToString());
                            }
                        }

                        if (asnr != "")
                        {
                            if (!variables.ContainsKey(asnr as string)) break;
                            variables[asnr] = ret;
                        }
                    }
                    break;
            }

            lastInstruction = instruction;
            //if (ret != null) Console.WriteLine("-->" + ret);
            return ret;
        }
    }
    public static class ext
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
    }
}
