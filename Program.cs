using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DIPsConsoleCompiler
{
    class Program
    {

        static void Main(string[] args)
        {
            string inPath = "", outPath = "";
            bool decompile = false, dbValid = false, bb = false, dataArray = false;
            CommandDB_Reader dB_Reader = new CommandDB_Reader();
            Program p = new Program();
            ushort version = 1;

            int argLength = args.Length;

            if (argLength < 2)
            {
                Console.WriteLine("Please supply an input file and output path.");
            }
            else
            {
                inPath = args[0];
                outPath = args[1];

                for (int i = 2; i < argLength; i++)
                {
                    switch (args[i])
                    {
                        default:
                            Console.WriteLine("The argument" + args[i] + "is an invalid argument");
                            break;
                        case "-dec":
                            decompile = true;
                            break;
                        case "-rec":
                            decompile = false;
                            break;
                        case "-bb":
                            bb = true;
                            break;
                        case "-ver":
                            i++;
                            version = UInt16.Parse(args[i]);
                            break;
                        case "-db":
                            i++;
                            dB_Reader.init(args[i]);
                            break;
                        case "-da":
                            dataArray = true;
                            break;
                    }
                }

                if (!dataArray)
                {
                    if (!dB_Reader.made)
                        dB_Reader.init(Environment.CurrentDirectory + "/CommandDB/DPScript_1.0.txt");

                    if (!(dB_Reader.errorAt > -1))
                        dbValid = true;
                    else
                        Console.WriteLine("Error Parsing CommandDB at line " + dB_Reader.errorAt);
                }
            }

            if (dbValid == true)
            {
                Console.WriteLine("CommandDB Successfully Parsed!");
                //dB_Reader.logAllCommands();
                if (decompile)
                    p.Decompiler(inPath, outPath, dB_Reader);
                else
                    p.Compiler(inPath, outPath, dB_Reader, version);
            }

            if(dataArray)
            {
                Data_Array array = new Data_Array();
                if (decompile)
                {
                    
                }
                else
                {
                    array.parseTextFile(inPath);
                    array.writeToBinary(outPath, version);
                }
                
            }


            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }


        public void Decompiler(string inPath, string outPath, CommandDB_Reader db_Reader)
        {
            Console.WriteLine("Parsing file for decompilation.");

            DPS_FileReader fileReader = new DPS_FileReader();
            fileReader.loadScript(inPath, db_Reader);
            scriptFile file = fileReader.getScript();
            List<string> buffer = new List<string>();

            Console.WriteLine("Parsing Complete!\nWriting to Text File.");

            for (int i = 0; i < file.entries.Count; i++)
            {
                scriptEntry tempEntry = file.entries[i];
                string entryString = "entry(\"" + tempEntry.name + "\", ";
                if (tempEntry.subroutine)
                    entryString += "true)";
                else
                    entryString += "false)";

                buffer.Add(entryString);

                int currentIndent = 1;
                for (int i2 = 0; i2 < tempEntry.commands.Count; i2++)
                {
                    string tempString = "";
                    for (int i3 = 0; i3 < currentIndent; i3++)
                        tempString += "\t";

                    Command tempCommand = db_Reader.getCommandInDB(db_Reader.findCommandID(tempEntry.commands[i2].id));
                    tempString += tempCommand.name + "(";
                    int curString = 0, curInt = 0, curUint = 0, curBool = 0, curByte = 0, curFloat = 0;
                    for (int i3 = 0; i3 < tempCommand.args.Length; i3++)
                    {
                        if (tempCommand.args[i3] != "n")
                        {
                            int typeCount = Int32.Parse(tempCommand.args[i3][0].ToString());
                            //Console.WriteLine("Parsed " + tempCommand.name + " args " + i3);
                            for (int i4 = 0; i4 < typeCount; i4++)
                            {
                                switch (tempCommand.args[i3][1])
                                {
                                    case 's':
                                        tempString += "\"" + tempEntry.commands[i2].stringArgs[curString] + "\"";
                                        curString++;
                                        break;
                                    case 'i':
                                        tempString += tempEntry.commands[i2].intArgs[curInt];
                                        curInt++;
                                        break;
                                    case 'u':
                                        tempString += tempEntry.commands[i2].uintArgs[curUint];
                                        curUint++;
                                        break;
                                    case 'f':
                                        tempString += tempEntry.commands[i2].floatArgs[curFloat];
                                        curFloat++;
                                        break;
                                    case 'h':
                                        tempString += "0x" + tempEntry.commands[i2].byteArgs[curByte];
                                        curByte++;
                                        break;
                                    case 'b':
                                        bool tempBool = tempEntry.commands[i2].boolArgs[curBool];
                                        if (tempBool)
                                            tempString += "true";
                                        else
                                            tempString += "false";
                                        curBool++;
                                        break;
                                }
                                if(i3 + 1 != tempCommand.args.Length)
                                    tempString += ", ";
                                else if(i4 + 1 != typeCount)
                                    tempString += ", ";
                            }
                        }
                    }
                    tempString += ")";
                    buffer.Add(tempString);
                }
                buffer.Add("");
            }

            File.WriteAllLines(outPath, buffer);

            Console.WriteLine("Complete!");
        }

        public void Compiler(string inPath, string outPath, CommandDB_Reader db_Reader, ushort version)
        {
            Console.WriteLine("Parsing file for compilation.");

            int entryCount = 0;
            string[] input = File.ReadAllLines(inPath);
            bool valid = true;
            List<int> LineCommand = new List<int>();
            List<string> LineArg = new List<string>();
            List<int> fileLines = new List<int>();

            for (int i = 0; i < input.Length; i++)
            {
                string line = input[i];
                if (line.Contains("(") && line.Contains(")"))
                {
                    line = line.Trim();
                    string com = line.Remove(line.IndexOf("("));
                    //Console.WriteLine(com);
                    string arg = line.Remove(0, line.IndexOf("(") + 1);
                    arg = arg.Remove(arg.IndexOf(")"));
                    //Console.WriteLine(arg);
                    if (com.Contains("Command_"))
                    {
                        string idString = com.Remove(0, 8);
                        //Console.WriteLine(idString);
                        int id = 0;
                        if (Int32.TryParse(idString, out id))
                        {
                            int db_Id = db_Reader.findCommandID(id);
                            if (db_Id == -1)
                            {
                                Console.WriteLine("Invalid command \"" + com + "\"");
                                return;
                            }
                            LineCommand.Add(db_Id);
                        }
                        else
                        {
                            Console.WriteLine("Invalid command \"" + com + "\"");
                            return;
                        }
                    }
                    else
                    {
                        int db_Id = db_Reader.findCommandName(com);
                        if (db_Id == -1)
                        {
                            Console.WriteLine("Invalid command \"" + com + "\"");
                            return;
                        }
                        LineCommand.Add(db_Id);
                    }
                    LineArg.Add(arg);
                    fileLines.Add(i);
                }
            }
            Console.WriteLine("Parsing Complete!\nCompiling into binary.");

            /*
            Console.WriteLine(LineCommand.Count);
            Console.WriteLine(LineArg.Count);
            Console.WriteLine(fileLines.Count);
            */

            int entryId = db_Reader.getEntryDbId();
            if (entryId == -1)
            {
                Console.WriteLine("No entry id in commanddb.");
                return;
            }

            foreach (int i in LineCommand)
            {
                if (LineCommand[i] == entryId)
                    entryCount++;
            }

            List<byte> buffer = new List<byte>();
            buffer.AddRange(Encoding.ASCII.GetBytes("DPS |").ToList()); buffer.Add(0x00); //header and script type file
            buffer.AddRange(BitConverter.GetBytes(entryCount)); //entry count
            buffer.AddRange(BitConverter.GetBytes(version)); //file version
            string signiture = "DIPsConsoleCompiler"; byte[] signSize = { 0x13, 0x00 };
            buffer.AddRange(signSize); buffer.AddRange(Encoding.ASCII.GetBytes(signiture).ToList()); //signiture
            byte[] padding = { 0x00, 0x00, 0x00, 0x00, 0x00 }; buffer.AddRange(padding); //padding at the end of header

            for (int i = 0; i < entryCount; i++)
            {
                List<byte> entry = new List<byte>();
                int entrySize = 0;
                string[] entryHeader = seperateArgs(LineArg[0]);
                int sizePlacement = 1;
                if (compareArgs(db_Reader.getArgs(entryId), entryHeader))
                {
                    //Console.WriteLine("Arg compare sucesful");
                    entryHeader[0] = entryHeader[0].Remove(0, 1); entryHeader[0] = entryHeader[0].Remove(entryHeader[0].Length - 1);
                    int nameSize = entryHeader[0].Length;
                    sizePlacement += nameSize;
                    entry.Add(BitConverter.GetBytes(nameSize)[0]);
                    entry.AddRange(Encoding.ASCII.GetBytes(entryHeader[0]));
                    //write size later
                    if (entryHeader[1] == "true")
                        entry.Add(0x01);
                    else
                        entry.Add(0x00);

                    LineCommand.RemoveAt(0);
                    LineArg.RemoveAt(0);
                    fileLines.RemoveAt(0);
                }
                else
                {
                    Console.WriteLine("Error compiling args at line " + fileLines[0]);
                    valid = false;
                    break;
                }

                while (LineCommand.Count != 0 && LineCommand[0] != entryId)
                {
                    Command temp = db_Reader.getCommandInDB(LineCommand[0]);
                    entry.AddRange(BitConverter.GetBytes(temp.id));
                    entrySize += 4;

                    if (temp.args[0] != "n")
                    {
                        string[] args = seperateArgs(LineArg[0]);
                        uint argCount = 0;
                        for (int i2 = 0; i2 < temp.args.Length; i2++)
                        {
                            uint typeCount = UInt32.Parse(temp.args[i2][0].ToString());
                            for (int i3 = (int)argCount; i3 < argCount + typeCount; i3++)
                            {
                                switch (temp.args[i2][1])
                                {
                                    case 's':
                                        if (args[i3].StartsWith("\"") && args[i3].EndsWith("\""))
                                        { args[i3] = args[i3].Remove(0, 1); args[i3] = args[i3].Remove(args[i3].Length - 1, 1); }
                                        entry.Add(BitConverter.GetBytes(args[i3].Length)[0]);
                                        entrySize += args[i3].Length + 1;
                                        entry.AddRange(Encoding.ASCII.GetBytes(args[i3]));
                                        break;
                                    case 'u':
                                        entry.AddRange(BitConverter.GetBytes(UInt32.Parse(args[i3])));
                                        entrySize += 4;
                                        break;
                                    case 'i':
                                        entry.AddRange(BitConverter.GetBytes(Int32.Parse(args[i3])));
                                        entrySize += 4;
                                        break;
                                    case 'f':
                                        entry.AddRange(BitConverter.GetBytes(float.Parse(args[i3])));
                                        entrySize += 4;
                                        break;
                                    case 'b':
                                        if (args[i3] == "true")
                                            entry.Add(0x01);
                                        else
                                            entry.Add(0x00);
                                        entrySize++;
                                        break;
                                    case 'h':
                                        args[i3] = args[i3].Remove(0, 2);
                                        entry.Add(Byte.Parse(args[i3]));
                                        entrySize += args[i3].Length / 2;
                                        break;
                                }
                            }
                            argCount += typeCount;
                        }
                    }
                    LineCommand.RemoveAt(0);
                    LineArg.RemoveAt(0);
                    fileLines.RemoveAt(0);
                }
                //Console.WriteLine(entrySize);
                entry.InsertRange(sizePlacement, BitConverter.GetBytes(entrySize));

                buffer.AddRange(entry);
            }

            if (valid)
            {
                FileStream output = File.Create(outPath);
                output.Write(buffer.ToArray(), 0, buffer.Count);
            }

            Console.WriteLine("Complete!");
        }

        string[] seperateArgs(string args)
        {
            List<string> seperated = new List<string>();
            string arg = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == ',')
                {
                    seperated.Add(arg);
                    arg = "";
                }
                else
                    arg += args[i];
            }
            if (arg != "")
                seperated.Add(arg);

            for (int i = 0; i < seperated.Count; i++)
                seperated[i] = seperated[i].Trim();

            return seperated.ToArray();
        }

        bool compareArgs(string[] args, string[] input)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[0] == "n" && input.Length == 0)
                    return true;
                string arg = args[i];
                int argCount;
                if (Int32.TryParse(arg[0].ToString(), out argCount) == false)
                    return false;
                //Console.WriteLine("Arg count parsed");
                string type = arg[1].ToString();
                for (int i2 = i; i2 < argCount + i; i2++)
                {
                    switch (type)
                    {
                        case "s":
                            if (!(input[i2].StartsWith("\"") && input[i2].EndsWith("\"")))
                                return false;
                            break;
                        case "i":
                            if (Int32.Parse(input[i2]) > Int32.MaxValue && Int32.Parse(input[i2]) < Int32.MinValue)
                                return false;
                            break;
                        case "u":
                            if (UInt32.Parse(input[i2]) > UInt32.MaxValue && UInt32.Parse(input[i2]) < UInt32.MinValue)
                                return false;
                            break;
                        case "f":
                            if (float.Parse(input[i2]) > float.MaxValue && float.Parse(input[i2]) < float.MinValue)
                                return false;
                            break;
                        case "b":
                            if (input[i2] != "true" && input[i2] != "false")
                                return false;
                            break;
                        case "h":
                            if (!(input[i2].StartsWith("0x")))
                                return false;
                            break;
                    }
                    //Console.WriteLine("Arg " + i + " parsed");
                }
            }
            return true;
        }
    }
}