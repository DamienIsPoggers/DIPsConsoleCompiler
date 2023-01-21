using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DIPsConsoleCompiler
{
    public class scriptCommand
    {
        public int id;
        public List<string> stringArgs = new List<string>();
        public List<int> intArgs = new List<int>();
        public List<uint> uintArgs = new List<uint>();
        public List<byte> byteArgs = new List<byte>();
        public List<float> floatArgs = new List<float>();
        public List<bool> boolArgs = new List<bool>();
    }

    public class scriptEntry
    {
        public string name;
        public bool subroutine;
        public List<scriptCommand> commands = new List<scriptCommand>();
    }

    public class scriptFile
    {
        public ushort version;
        public string signiture;
        public List<scriptEntry> entries = new List<scriptEntry>();
    }

    class DPS_FileReader
    {
        scriptFile script = new scriptFile();
        
        public void loadScript(string path, CommandDB_Reader db_Reader)
        {
            BinaryReader file = new BinaryReader(File.Open(path, FileMode.Open));
            string indent = Encoding.ASCII.GetString(file.ReadBytes(5));
            if(indent != "DPS |" || file.ReadByte() != 0x00 )
            {
                Console.WriteLine("Invalid Script Binary\nPress any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            uint entryCount = file.ReadUInt32();
            script.version = file.ReadUInt16();
            script.signiture = Encoding.ASCII.GetString(file.ReadBytes(file.ReadUInt16()));
            //Console.WriteLine(script.signiture);
            file.ReadBytes(5);

            for(int i = 0; i < entryCount; i++)
            {
                scriptEntry entry = new scriptEntry();
                byte nameSize = file.ReadByte();
                //Console.WriteLine(nameSize);
                entry.name = Encoding.ASCII.GetString(file.ReadBytes(nameSize));
                //Console.WriteLine(entry.name);
                uint entrySize = file.ReadUInt32();
                entry.subroutine = file.ReadBoolean();

                uint position = 0;
                while(position < entrySize)
                {
                    position += 4;
                    scriptCommand com = new scriptCommand();
                    com.id = file.ReadInt32();
                    //Console.WriteLine(com.id);
                    Command temp = db_Reader.getCommandInDB(db_Reader.findCommandID(com.id));
                    //Console.WriteLine(temp.name);
                    string[] args = temp.args;
                    for(int i2 = 0; i2 < args.Length; i2++)
                    {
                        uint typeCount;
                        UInt32.TryParse(args[i2][0].ToString(), out typeCount);
                        for (int i3 = 0; i3 < typeCount; i3++)
                        {
                            switch (temp.args[i2][1])
                            {
                                case 's':
                                    string tempString = Encoding.ASCII.GetString(file.ReadBytes(file.ReadByte()));
                                    com.stringArgs.Add(tempString);
                                    position += (uint)tempString.Length + 1;
                                    break;
                                case 'i':
                                    com.intArgs.Add(file.ReadInt32());
                                    position += 4;
                                    break;
                                case 'u':
                                    com.uintArgs.Add(file.ReadUInt32());
                                    position += 4;
                                    break;
                                case 'f':
                                    com.floatArgs.Add(file.ReadSingle());
                                    position += 4;
                                    break;
                                case 'h':
                                    com.byteArgs.Add(file.ReadByte());
                                    position++;
                                    break;
                                case 'b':
                                    com.boolArgs.Add(file.ReadBoolean());
                                    position++;
                                    break;
                            }
                        }
                    }
                    entry.commands.Add(com);
                }
                script.entries.Add(entry);
            }
        }

        public scriptFile getScript()
        {
            return script;
        }
    }
}
