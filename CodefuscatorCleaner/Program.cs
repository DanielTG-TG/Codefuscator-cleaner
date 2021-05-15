using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodefuscatorCleaner
{
    class Program
    {
        public static int lenghtremoved;
        public static int de4dotremoved;
        public static int stringdecrypted;
        static void Main(string[] args)
        {
            Console.Title = "CodeFuscator Cleaner";
            ModuleDef module = ModuleDefMD.Load(args[0]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loaded assembly: " + module.FullName);
            Stringlenght(module);
            antiDe4dot(module);
            decryptStrings(module);

            Console.WriteLine("Removed " + lenghtremoved + " lenght mutations.");
            Console.WriteLine("Removed " + de4dotremoved + " anti de4dot cases.");
            Console.WriteLine("Decrypted " + stringdecrypted + " base64 encrypted strings.");
            Console.ForegroundColor = ConsoleColor.White;
            module.Write(Path.GetFileNameWithoutExtension(args[0]) + "-cleaned.exe");
            Console.WriteLine("File saved. Press any key to exit.");
            Console.ReadKey();
        }


        public static void antiDe4dot(ModuleDef module)
        {
            foreach (var type in module.Types.ToList().Where(t => t.HasInterfaces))
            {
                for (var i = 0; i < type.Interfaces.Count; i++)
                {
                    if (type.Interfaces[i].Interface.Name.Contains(type.Name) || type.Name.Contains(type.Interfaces[i].Interface.Name))
                    {
                        module.Types.Remove(type);
                        de4dotremoved++;
                    }
                }
            }
        }

        public static void decryptStrings(ModuleDef module)
        {
            foreach (TypeDef t_ in module.Types)
            {
                if (!t_.HasMethods) { continue; }
                foreach (MethodDef methods in t_.Methods)
                {
                    methods.Body.KeepOldMaxStack = true;
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count; x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.OpCode.Equals(OpCodes.Ldstr) && methods.Body.Instructions[x + 1].OpCode.Equals(OpCodes.Call))
                        {
                            string str = inst.Operand.ToString();
                        checkInst:
                            if (methods.Body.Instructions[x + 1].Operand != null && methods.Body.Instructions[x + 1].OpCode.Equals(OpCodes.Call))
                            {
                                if (methods.Body.Instructions[x + 1].Operand.ToString().Contains("StringDecryptor"))
                                {
                                    if (methods.Body.Instructions[x + 1].Operand.ToString().Contains("StringDecryptor"))
                                    {
                                        str = Encoding.UTF8.GetString(Convert.FromBase64String(str));
                                        methods.Body.Instructions.RemoveAt(x + 1);
                                        goto checkInst;
                                    }
                                }
                            }
                            stringdecrypted++;
                            inst.Operand = str;
                        }
                    }
                }
            }
        }
            public static void Stringlenght(ModuleDef module)
           {
            foreach (var TypeDef in module.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Ldstr &&
                            IL[x + 1].OpCode == OpCodes.Ldlen)
                        {
                            IL[x] = Instruction.CreateLdcI4(IL[x].Operand.ToString().Length);
                            IL.RemoveAt(x + 1);
                            lenghtremoved++;
                        }
                        if (IL[x].OpCode == OpCodes.Ldstr &&
                            (IL[x + 1].OpCode == OpCodes.Call || IL[x].OpCode == OpCodes.Callvirt) && IL[x + 1].Operand.ToString().Contains("get_Length"))
                        {
                            IL[x] = Instruction.CreateLdcI4(IL[x].Operand.ToString().Length);
                            IL.RemoveAt(x + 1);
                            lenghtremoved++;
                        }
                    }
                }
            }
        }
    }
}