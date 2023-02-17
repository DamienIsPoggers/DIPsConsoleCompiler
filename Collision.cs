using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPsConsoleCompiler
{
    public class collisionBox
    {
        public int[] x = new int[2];
        public int[] y = new int[2];
        public int[] z = new int[2];
        public byte id;
        public byte type;
    }

    public class collisionChunk
    {
        public byte sprite;
        public byte id;
        public int[] uv = new int[4];
        public int[] origin = new int[2];
        public float[] scale;
        public float[] rotation;
    }

    public class collisionEntry
    {
        public string name;
        public byte boxCount;
        public byte chunkCount;
        public bool sphere = false;
        public bool hasZ = false;
        public List<string> sprites = new List<string>();
        public List<collisionChunk> chunks = new List<collisionChunk>();
        public List<collisionBox> boxes = new List<collisionBox>();
    }

    public class collisionFile
    {
        public string signiture;
        public short version;
        public List<collisionEntry> entries = new List<collisionEntry>();
    }

    public class Collision
    {
        public collisionFile colfile = new collisionFile();


        public void loadJonbinsFromFolder(string folder, bool gbvs)
        {
            Console.WriteLine("Parsing Jonbins");

            foreach(string jonb in Directory.EnumerateFiles(folder))
            {
                BinaryReader file = new BinaryReader(File.Open(jonb, FileMode.Open));
                if (Encoding.ASCII.GetString(file.ReadBytes(4)) != "JONB")
                    continue;

                collisionEntry temp = new collisionEntry();
                temp.name = jonb.Remove(0, jonb.LastIndexOf("\\") + 1);
                //Console.WriteLine(temp.name);
                temp.hasZ = false;
                temp.sphere = false;
                ushort spriteCount = file.ReadUInt16();
                for (int i = 0; i < spriteCount; i++)
                {
                    string tempString = Encoding.ASCII.GetString(file.ReadBytes(32));
                    if (tempString.Contains("."))
                        tempString = tempString.Remove(tempString.IndexOf("."));
                    temp.sprites.Add(tempString);
                    //Console.WriteLine(tempString);
                }

                List<ushort> counts = new List<ushort>();
                byte tempbyte = file.ReadByte();
                //Console.WriteLine(tempbyte);
                for (int i = 0; i < tempbyte; i++)
                    counts.Add(file.ReadUInt16());

                file.ReadBytes(52);

                for(int i = 0; i < counts[1]; i++)
                {
                    temp.chunkCount++;
                    collisionChunk tempChunk = new collisionChunk();
                    if (i > temp.sprites.Count)
                        tempChunk.sprite = 0;
                    else
                        tempChunk.sprite = (byte)i;

                    tempChunk.id = (byte)i;

                    tempChunk.uv = new int[] { (int)file.ReadSingle(), (int)file.ReadSingle(), (int)file.ReadSingle(), (int)file.ReadSingle() };
                    tempChunk.origin = new int[] { (int)file.ReadSingle() * -1, (int)file.ReadSingle() };

                    tempChunk.scale = new float[] { (file.ReadSingle() / (float)tempChunk.uv[2]) * -1, (file.ReadSingle() / (float)tempChunk.uv[3]) };
                    tempChunk.rotation = new float[] { 0, 0, 0 };
                    file.ReadBytes(48);
                    temp.chunks.Add(tempChunk);
                }

                byte boxID = 0;
                for(int i = 3; i < counts.Count; i++)
                    for(int j = 0; j < counts[i]; j++)
                    {
                        temp.boxCount++;
                        file.ReadInt32();

                        collisionBox tempBox = new collisionBox();
                        int x1 = (int)file.ReadSingle(), y1 = (int)file.ReadSingle(),
                            x2 = (int)file.ReadSingle(), y2 = (int)file.ReadSingle();

                        tempBox.x = new int[] {  -x1 -(x2/2), x2 / 2 };
                        tempBox.y = new int[] { -y1 - (y2 / 2), y2 / 2};

                        if (gbvs)
                            file.ReadBytes(4);

                        tempBox.id = boxID;
                        boxID++;

                        switch(i)
                        {
                            default:
                                tempBox.type = 11;
                                break;
                            case 3:
                                tempBox.type = 1;
                                break;
                            case 4:
                                tempBox.type = 2;
                                break;
                            case 5:
                                tempBox.type = 3;
                                break;
                            case 10:
                                tempBox.type = 5;
                                break;
                            case 11:
                                tempBox.type = 6;
                                break;
                            case 14:
                                tempBox.type = 9;
                                break;
                            case 15:
                                tempBox.type = 10;
                                break;

                        }
                        temp.boxes.Add(tempBox);
                    }

                colfile.entries.Add(temp);
                file.Close();
            }
        }

        public void writeToBinary(string outPath, ushort version)
        {
            Console.WriteLine("Compiling to binary.");

            List<byte> buffer = new List<byte>();
            buffer.AddRange(Encoding.ASCII.GetBytes("DPS |").ToList()); buffer.Add(0x01); //header and collision type file
            buffer.AddRange(BitConverter.GetBytes(colfile.entries.Count)); //entry count
            buffer.AddRange(BitConverter.GetBytes(version)); //file version
            string signiture = "DIPsConsoleCompiler"; byte[] signSize = { 0x13, 0x00 };
            buffer.AddRange(signSize); buffer.AddRange(Encoding.ASCII.GetBytes(signiture).ToList()); //signiture
            byte[] padding = { 0x00, 0x00, 0x00, 0x00, 0x00 }; buffer.AddRange(padding); //padding at the end of header


            for(int i = 0; i < colfile.entries.Count; i++)
            {
                collisionEntry entry = colfile.entries[i];
                buffer.Add(BitConverter.GetBytes(entry.name.Length)[0]);
                buffer.AddRange(Encoding.ASCII.GetBytes(entry.name));
                buffer.Add(entry.boxCount);
                buffer.Add(entry.chunkCount);
                if (entry.sphere)
                    buffer.Add(0x01);
                else
                    buffer.Add(0x00);

                if (entry.hasZ)
                    buffer.Add(0x01);
                else
                    buffer.Add(0x00);

                buffer.Add(BitConverter.GetBytes(entry.sprites.Count)[0]);
                for(int i2 = 0; i2 < entry.sprites.Count; i2++)
                {
                    buffer.Add(BitConverter.GetBytes(entry.sprites[i2].Length)[0]);
                    buffer.AddRange(Encoding.ASCII.GetBytes(entry.sprites[i2]));
                }

                for(int i2 = 0; i2 < entry.chunkCount; i2++)
                {
                    collisionChunk chunk = entry.chunks[i2];
                    buffer.Add(chunk.sprite);
                    buffer.Add(chunk.id);
                    for (int i3 = 0; i3 < 4; i3++)
                        buffer.AddRange(BitConverter.GetBytes(chunk.uv[i3]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.origin[0]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.origin[1]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.scale[0]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.scale[1]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.rotation[0]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.rotation[1]));
                    buffer.AddRange(BitConverter.GetBytes(chunk.rotation[2]));
                }

                for(int i2 = 0; i2 < entry.boxCount; i2++)
                {
                    collisionBox box = entry.boxes[i2];
                    buffer.Add(box.id);
                    buffer.Add(box.type);
                    buffer.AddRange(BitConverter.GetBytes(box.x[0]));
                    buffer.AddRange(BitConverter.GetBytes(box.x[1]));
                    buffer.AddRange(BitConverter.GetBytes(box.y[0]));
                    buffer.AddRange(BitConverter.GetBytes(box.y[1]));
                    if (entry.hasZ)
                    {
                        buffer.AddRange(BitConverter.GetBytes(box.z[0]));
                        buffer.AddRange(BitConverter.GetBytes(box.z[1]));
                    }
                }
            }

            FileStream output = File.Create(outPath);
            output.Write(buffer.ToArray(), 0, buffer.Count);
        
            Console.WriteLine("Complete!");
        }
    }
}
