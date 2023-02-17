using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DIPsConsoleCompiler
{
    public class dataArray
    {
        public List<string> strings = new List<string>();
        public List<int> ints = new List<int>();
        public List<uint> uints = new List<uint>();
        public List<byte> bytes = new List<byte>();
        public List<float> floatArgs = new List<float>();
        public List<bool> bools = new List<bool>();
    }

    public class dataArray_Entry
    {
        public string name;
        public short count;
        public byte[] fileStruct;
        public List<dataArray> arrays = new List<dataArray>();
    }

    public class Data_Array
    {
        List<dataArray_Entry> arrays = new List<dataArray_Entry>();

        public void parseTextFile(string inFile)
        {
            Console.WriteLine("Parsing file for compilation.");

            string[] file = File.ReadAllLines(inFile);
            int entryCount = 0;
            int curLine = 1;

            if (file[0].Contains("entryCount"))
            {
                string line = file[0];
                line = line.Remove(0, line.IndexOf('(') + 1);
                line = line.Remove(line.IndexOf(")"));
                //Console.WriteLine(line);
                entryCount = Int32.Parse(line);
            }
            else
            {
                Console.WriteLine("Invalid Table or entryCount isnt on the first line.\nPress any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            for (int i = 0; i < entryCount; i++)
            {
                dataArray_Entry entry = new dataArray_Entry();
                bool count = false, name = false, fileStruct = false;
                int failed = 0;
                while (!count || !name || !fileStruct)
                {
                    string line = file[curLine];
                    if (line.Contains("count"))
                    {
                        line = line.Remove(0, line.IndexOf("(") + 1);
                        line = line.Remove(line.IndexOf(")"));
                        entry.count = Int16.Parse(line);
                        count = true;
                    }
                    else if (line.Contains("name"))
                    {
                        line = line.Remove(0, line.IndexOf("(") + 2);
                        line = line.Remove(line.IndexOf(")") - 1);
                        entry.name = line;
                        name = true;
                        //Console.WriteLine(entry.name);
                    }
                    else if (line.Contains("struct"))
                    {
                        line = line.Remove(0, line.IndexOf("(") + 1);
                        line = line.Remove(line.IndexOf(")"));
                        string[] structArgs = seperateArgs(line);
                        /*for (int j = 0; j < structArgs.Length; j++)
                            Console.WriteLine(structArgs[j]); */
                        List<byte> args = new List<byte>();

                        for (int i2 = 0; i2 < structArgs.Length; i2++)
                        {
                            switch (structArgs[i2])
                            {
                                case "i":
                                    args.Add(0);
                                    break;
                                case "u":
                                    args.Add(1);
                                    break;
                                case "f":
                                    args.Add(2);
                                    break;
                                case "s":
                                    args.Add(3);
                                    break;
                                case "h":
                                    args.Add(4);
                                    break;
                                case "b":
                                    args.Add(5);
                                    break;
                                case null:
                                    Console.WriteLine("Invalid Struct Definition\nPress any key to exit.");
                                    Console.ReadKey();
                                    Environment.Exit(1);
                                    break;
                            }
                        }
                        entry.fileStruct = args.ToArray();
                        fileStruct = true;
                    }
                    else
                        failed++;

                    curLine++;

                    if (failed == 10)
                    {
                        Console.WriteLine("Couldn't find header information within the first 10 lines.\nPress any key to exit.");
                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                }

                for (int i2 = 0; i2 < entry.count; i2++)
                {
                    dataArray tempArray = new dataArray();

                    for (int i3 = 0; i3 < entry.fileStruct.Length; i3++)
                    {
                        string line = file[curLine];
                        if (!String.IsNullOrEmpty(line) || line.Contains(","))
                        {
                            line = line.Trim();
                            line = line.Remove(line.LastIndexOf(","), 1);
                            //Console.WriteLine(entry.fileStruct[i3]);
                            switch (entry.fileStruct[i3])
                            {
                                case 0:
                                    int tempi;
                                    if (!Int32.TryParse(line, out tempi))
                                        errorParsing(curLine);
                                    tempArray.ints.Add(tempi);
                                    break;
                                case 1:
                                    uint tempu;
                                    if (!UInt32.TryParse(line, out tempu))
                                        errorParsing(curLine);
                                    tempArray.uints.Add(tempu);
                                    //Console.WriteLine(tempu);
                                    break;
                                case 2:
                                    float tempf;
                                    if (!float.TryParse(line, out tempf))
                                        errorParsing(curLine);
                                    tempArray.floatArgs.Add(tempf);
                                    break;
                                case 3:
                                    if (!line.StartsWith("\"") && !line.EndsWith("\""))
                                        errorParsing(curLine);
                                    line = line.Remove(0, 1); line = line.Remove(line.LastIndexOf("\""));
                                    tempArray.strings.Add(line);
                                    //Console.WriteLine(line);
                                    break;
                                case 4:
                                    if (!line.StartsWith("0x"))
                                        errorParsing(curLine);
                                    line = line.Remove(0, 2);
                                    byte tempb;
                                    if (!Byte.TryParse(line, out tempb))
                                        errorParsing(curLine);
                                    tempArray.bytes.Add(tempb);
                                    break;
                                case 5:
                                    if (line != "true" && line != "false")
                                        errorParsing(curLine);
                                    if (line == "true")
                                        tempArray.bools.Add(true);
                                    else
                                        tempArray.bools.Add(false);
                                    break;
                            }
                        }
                        else
                            i3--;
                        curLine++;
                    }
                    entry.arrays.Add(tempArray);
                }
                arrays.Add(entry);
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

            void errorParsing(int line)
            {
                Console.WriteLine("Error Parsing line " + line + ".\nPress any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

        }


        public void writeToBinary(string outPath, ushort version)
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(Encoding.ASCII.GetBytes("DPS |")); buffer.Add(0x0A); //header and script type file
            buffer.AddRange(BitConverter.GetBytes(arrays.Count)); //entry count
            buffer.AddRange(BitConverter.GetBytes(version)); //file version
            string signiture = "DIPsConsoleCompiler"; byte[] signSize = { 0x13, 0x00 };
            buffer.AddRange(signSize); buffer.AddRange(Encoding.ASCII.GetBytes(signiture).ToList()); //signiture
            byte[] padding = { 0x00, 0x00, 0x00, 0x00, 0x00 }; buffer.AddRange(padding); //padding at the end of header

            for(int i = 0; i < arrays.Count; i++)
            {
                byte nameSize = (byte)arrays[i].name.Length;
                buffer.Add(nameSize);
                buffer.AddRange(Encoding.ASCII.GetBytes(arrays[i].name));
                buffer.AddRange(BitConverter.GetBytes(arrays[i].count));
                byte structSize = (byte)arrays[i].fileStruct.Length;
                buffer.Add(structSize);
                buffer.AddRange(arrays[i].fileStruct);

                for(int i2 = 0; i2 < arrays[i].arrays.Count; i2++)
                {
                    dataArray tempArray = arrays[i].arrays[i2];
                    int curI = 0, curU = 0, curF = 0, curS = 0, curH = 0, curB = 0;
                    for(int i3 = 0; i3 < arrays[i].fileStruct.Length; i3++)
                    {
                        switch(arrays[i].fileStruct[i3])
                        {
                            case 0:
                                buffer.AddRange(BitConverter.GetBytes(tempArray.ints[curI]));
                                curI++;
                                break;
                            case 1:
                                buffer.AddRange(BitConverter.GetBytes(tempArray.uints[curU]));
                                curU++;
                                break;
                            case 2:
                                buffer.AddRange(BitConverter.GetBytes(tempArray.floatArgs[curF]));
                                curF++;
                                break;
                            case 3:
                                buffer.Add((byte)tempArray.strings[curS].Length);
                                buffer.AddRange(Encoding.ASCII.GetBytes(tempArray.strings[curS]));
                                curS++;
                                break;
                            case 4:
                                buffer.Add(tempArray.bytes[curH]);
                                curH++;
                                break;
                            case 5:
                                if (tempArray.bools[curB])
                                    buffer.Add(0x01);
                                else
                                    buffer.Add(0x00);
                                curB++;
                                break;
                        }
                    }
                }
            }
            FileStream output;
            if (File.Exists(outPath))
                output = File.Open(outPath, FileMode.Create);
            else
                output = File.Create(outPath);
            output.Write(buffer.ToArray(), 0, buffer.Count);
        }
    }
}
