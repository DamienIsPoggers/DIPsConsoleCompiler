using System;
using System.Collections.Generic;
using System.IO;

namespace DIPsConsoleCompiler
{
    internal class Encryption
    {
        public void Encrypt(string input, string output, string keyString)
        {
            List<byte> buffer = new List<byte>(), key = new List<byte>();

            if(keyString.Length != 256)
            {
                Console.WriteLine("key length isn't 256. Stopping");
                return;
            }

            for(int i = 0; i < 256; i++)
            {
                byte b = Convert.ToByte(keyString[i]);
                key.Add(b);
            }

            byte pointInKey = 0;
            BinaryReader file = new BinaryReader(File.Open(input, FileMode.Open));

            for(int i = 0; i < file.BaseStream.Length; i++)
            {
                byte b = file.ReadByte();
                if (pointInKey == 0)
                    b = (byte)(b + ((key[pointInKey] * key[pointInKey + 1]) - key[pointInKey]));
                else if (pointInKey == 255)
                    b = (byte)(b + ((key[pointInKey] * key[pointInKey - 1]) - key[pointInKey]));
                else
                    b = (byte)(b + ((key[pointInKey] * key[pointInKey + 1]) - key[pointInKey - 1]));

                pointInKey += (byte)(key[pointInKey] ^ 2);

                buffer.Add(b);
            }

            FileStream end = File.Create(output);
            end.Write(buffer.ToArray(), 0, buffer.Count);

        }
        public void Decrypt(string input, string output, string keyString)
        {
            List<byte> buffer = new List<byte>(), key = new List<byte>();

            if (keyString.Length != 256)
            {
                Console.WriteLine("key length isn't 256. Stopping");
                return;
            }

            for (int i = 0; i < 256; i++)
            {
                byte b = Convert.ToByte(keyString[i]);
                key.Add(b);
            }

            byte pointInKey = 0;
            BinaryReader file = new BinaryReader(File.Open(input, FileMode.Open));

            for (int i = 0; i < file.BaseStream.Length; i++)
            {
                byte b = file.ReadByte();
                if (pointInKey == 0)
                    b = (byte)(b - ((key[pointInKey] * key[pointInKey + 1]) - key[pointInKey]));
                else if (pointInKey == 255)
                    b = (byte)(b - ((key[pointInKey] * key[pointInKey - 1]) - key[pointInKey]));
                else
                    b = (byte)(b - ((key[pointInKey] * key[pointInKey + 1]) - key[pointInKey - 1]));

                pointInKey += (byte)(key[pointInKey] ^ 2);

                buffer.Add(b);
            }

            FileStream end = File.Create(output);
            end.Write(buffer.ToArray(), 0, buffer.Count);

        }
    }
}
