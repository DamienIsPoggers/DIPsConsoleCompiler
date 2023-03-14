using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DIPsConsoleCompiler
{
    public struct Command
    {
        public string name;
        public int id;
        public string[] args;
    }

    public class CommandDB_Reader
    {
        string[] db;
        List<Command> commands = new List<Command>();
        public int errorAt;
        public bool made = false;

        public void init(string dbPath)
        {
            db = File.ReadAllLines(dbPath);
            Console.WriteLine("Parsing CommandDB");
            errorAt = Parse_DB();
        }

        int Parse_DB()
        {
            for (int i = 0; i < db.Length; i++)
            {
                string line = db[i];
                List<string> args = new List<string>();
                string arg = "";
                for (int i2 = 0; i2 < line.Length; i2++)
                {
                    if (line[i2] == ',')
                    {
                        args.Add(arg);
                        arg = "";
                    }
                    else if (line[i2] == ' ')
                        continue;
                    else
                        arg += line[i2];
                }

                if (arg != "")
                    args.Add(arg);

                Command temp = new Command();
                for (int i2 = 0; i2 < args.Count; i2++)
                {
                    int id;
                    //Console.WriteLine(args[i2]);
                    if (Int32.TryParse(args[i2], out id))
                    {
                        temp.id = id;
                        //Console.WriteLine("Parsed");
                    }
                    else if (args[i2][0] == '"' && args[i2][args[i2].Length - 1] == '"')
                    {
                        string tempString = args[i2].Remove(0, 1);
                        tempString = tempString.Remove(tempString.Length - 1, 1);
                        temp.name = tempString;
                    }
                    else
                    {
                        if (args[i2].Contains(" ")) return i;
                        temp.args = parseArgs(args[i2]);
                    }
                }

                if (temp.name == null)
                    temp.name = "null";

                if (args.Count > 0)
                    commands.Add(temp);
            }
            return -1;
        }

        string[] parseArgs(string arg)
        {
            List<string> rtrn = new List<string>();
            string type = "";
            //Console.WriteLine(arg);
            while (arg.Length != 0)
            {
                if (arg[0] == 'n')
                {
                    rtrn.Add("n");
                    break;
                }
                else
                {
                    type += arg[0]; type += arg[1];
                    int tempInt;
                    arg = arg.Remove(0, 2);
                    if(Int32.TryParse(type, out tempInt))
                    {
                        type += arg[0];
                        arg = arg.Remove(0, 1);
                    }
                    rtrn.Add(type);
                    type = "";
                    continue;
                }
            }
            return rtrn.ToArray();
        }

        public bool hasError()
        {
            if (errorAt > -1)
                return true;
            return false;
        }

        public int findCommandID(int id)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (id == commands[i].id)
                    return i;
            }
            return -1;
        }

        public int findCommandName(string name)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (name == commands[i].name)
                    return i;
            }
            return -1;
        }

        public int getEntryDbId()
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].id == 0)
                    return i;
            }
            return -1;
        }

        public Command getCommandInDB(int id)
        {
            return commands[id];
        }

        public string[] getArgs(int dbId)
        {
            return commands[dbId].args;
        }

        public void logAllCommands()
        {
            for (int i = 0; i < commands.Count; i++)
            {
                Console.WriteLine("DB id " + i);
                Console.WriteLine("Name: " + commands[i].name);
                Console.WriteLine("ID: " + commands[i].id);
                Console.WriteLine("Args: ");
                for (int i2 = 0; i2 < commands[i].args.Length; i2++)
                    Console.WriteLine("    " + commands[i].args[i2]);
                Console.WriteLine("\n");
            }
        }
    }
}
