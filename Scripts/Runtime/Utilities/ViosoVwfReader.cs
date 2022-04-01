using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Utility class for loading Vioso projector warp-blend textures.
    /// </summary>
    internal class ViosoVwfReader
    {
        internal abstract class Block
        {

            abstract public void GetData(Stream stream);
            abstract public override string ToString();
        }

        internal class Vwf1 : Block
        {

            public long Indice;
            public string MagicNumber;
            public long NumBlocks;
            public long Offs;
            public long Reserved;

            public Vwf1(long ind)
            {
                Indice = ind;
                MagicNumber = "vwf1";
            }

            public override void GetData(Stream stream)
            {
                stream.Position = Indice;

                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                MagicNumber = System.Text.Encoding.UTF8.GetString(buffer);

                stream.Read(buffer, 0, 4);
                NumBlocks = (int)BitConverter.ToInt16(buffer, 0);

                stream.Read(buffer, 0, 4);
                Offs = (int)BitConverter.ToInt16(buffer, 0);

                stream.Read(buffer, 0, 4);
                Reserved = (int)BitConverter.ToInt16(buffer, 0);
            }

            public override string ToString()
            {
                string txt = "";
                txt += "---------\n";
                txt += "Indice: " + Indice.ToString() + " \n";
                txt += "MagicNumber: " + MagicNumber + " \n";
                txt += "NumBlocks: " + NumBlocks.ToString() + " \n";
                txt += "Offs: " + Offs.ToString() + " \n";
                txt += "Reserved: " + Reserved.ToString() + " \n";
                txt += "---------\n";
                return txt;
            }
        }

        internal class Vwf2 : Block
        {

            public long Indice;
            public string MagicNumber;

            public long SizeHeader;
            public int Flags;
            public int HMonitor;
            public long Size;
            public long Width;
            public long Height;
            public float[] White;
            public float[] Black;
            public float[] ReservedInfo;
            public string Name;

            public long BmpSize;

            public Vwf2(long ind)
            {
                Indice = ind;
                MagicNumber = "vwf2";
            }

            public override void GetData(Stream stream)
            {
                stream.Position = Indice;

                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                MagicNumber = System.Text.Encoding.UTF8.GetString(buffer);

                stream.Read(buffer, 0, 4);
                SizeHeader = (int)BitConverter.ToInt32(buffer, 0);

                stream.Read(buffer, 0, 4);
                Flags = (int)BitConverter.ToInt32(buffer, 0);

                stream.Read(buffer, 0, 4);
                HMonitor = (int)BitConverter.ToInt32(buffer, 0);

                stream.Read(buffer, 0, 4);
                Size = (int)BitConverter.ToInt32(buffer, 0);

                stream.Read(buffer, 0, 4);
                Width = (int)BitConverter.ToInt32(buffer, 0);

                stream.Read(buffer, 0, 4);
                Height = (int)BitConverter.ToInt32(buffer, 0);

                White = new float[4];
                for (int i = 0; i < White.Length; i++)
                {
                    stream.Read(buffer, 0, 4);
                    White[i] = (float)BitConverter.ToSingle(buffer, 0);
                }

                Black = new float[4];
                for (int i = 0; i < Black.Length; i++)
                {
                    stream.Read(buffer, 0, 4);
                    Black[i] = (float)BitConverter.ToSingle(buffer, 0);
                }

                ReservedInfo = new float[16];
                for (int i = 0; i < ReservedInfo.Length; i++)
                {
                    stream.Read(buffer, 0, 4);
                    ReservedInfo[i] = (float)BitConverter.ToSingle(buffer, 0);
                }

                buffer = new byte[256];
                stream.Read(buffer, 0, 256);
                Name = System.Text.Encoding.UTF8.GetString(buffer);

                BmpSize = Width * Height * 3 + 54;
            }

            public override string ToString()
            {
                string txt = "";
                txt += "---------\n";
                txt += "Indice: " + Indice.ToString() + " \n";
                txt += "MagicNumber: " + MagicNumber + " \n";
                txt += "SizeHeader: " + SizeHeader.ToString() + " \n";
                txt += "Flags: " + Flags.ToString() + " \n";
                txt += "HMonitor: " + HMonitor.ToString() + " \n";
                txt += "Size: " + Size.ToString() + " \n";
                txt += "Width: " + Width.ToString() + " \n";
                txt += "Height: " + Height.ToString() + " \n";

                txt += "White: \n";
                if (White != null)
                {
                    for (int i = 0; i < White.Length; i++)
                    {
                        txt += "    " + White[i].ToString() + " \n";
                    }
                }
                txt += "Black: \n";
                if (Black != null)
                {
                    for (int i = 0; i < Black.Length; i++)
                    {
                        txt += "    " + Black[i].ToString() + " \n";
                    }
                }
                txt += "ReservedInfo: \n";
                if (ReservedInfo != null)
                {
                    string[] names = { "Row ind", "Column ind", "Row quantum", "Column quantum", "Original display width", "Original display height", "Type", "OffsetX", "OffsetY", "BlackLevel correction", "BlackLevel dark", "BlackLevel bright", "Identifier", "???", "???", "???", "???" };
                    for (int i = 0; i < ReservedInfo.Length; i++)
                    {
                        txt += "    " + names[i] + ": " + ReservedInfo[i].ToString() + " \n";
                    }
                }
                txt += "Name: " + Name.ToString() + " \n";
                txt += "---------\n";
                return txt;
            }

            public Texture2D GetBlendTexture(Stream stream)
            {
                Texture2D ret = new Texture2D((int)Width, (int)Height, TextureFormat.RGB24, false);
                byte[] buffer = new byte[BmpSize - 54];
                stream.Position = SizeHeader + Size + Indice + 54;
                int n = stream.Read(buffer, 0, (int)BmpSize - 54);
                ret.LoadRawTextureData(buffer);
                ret.Apply();
                return ret;
            }
            public Texture2D GetWarpTexture(Stream stream)
            {
                Texture2D ret = new Texture2D((int)Width, (int)Height, TextureFormat.RGBAFloat, false);
                long numPixels = Width * Height;
                int bytesPerTexel = 16;
                byte[] buffer = new byte[16];
                byte[] bitBuffer = new byte[4];
                byte[] texBuffer = new byte[(int)numPixels * bytesPerTexel];
                float val;
                stream.Position = SizeHeader + Indice;
                for (int n = 0; n < numPixels; n++)
                {
                    stream.Read(buffer, 0, 16);
                    int k = n * bytesPerTexel;

                    val = BitConverter.ToSingle(buffer, 0);
                    bitBuffer = BitConverter.GetBytes(val);
                    texBuffer[k] = bitBuffer[0];
                    texBuffer[k + 1] = bitBuffer[1];
                    texBuffer[k + 2] = bitBuffer[2];
                    texBuffer[k + 3] = bitBuffer[3];

                    val = BitConverter.ToSingle(buffer, 4);
                    bitBuffer = BitConverter.GetBytes(val);
                    texBuffer[k + 4] = bitBuffer[0];
                    texBuffer[k + 5] = bitBuffer[1];
                    texBuffer[k + 6] = bitBuffer[2];
                    texBuffer[k + 7] = bitBuffer[3];

                    if (bytesPerTexel > 8)
                    {
                        val = BitConverter.ToSingle(buffer, 8);
                        bitBuffer = BitConverter.GetBytes(val);
                        texBuffer[k + 8] = bitBuffer[0];
                        texBuffer[k + 9] = bitBuffer[1];
                        texBuffer[k + 10] = bitBuffer[2];
                        texBuffer[k + 11] = bitBuffer[3];

                        val = 1f;
                        bitBuffer = BitConverter.GetBytes(val);
                        texBuffer[k + 12] = bitBuffer[0];
                        texBuffer[k + 13] = bitBuffer[1];
                        texBuffer[k + 14] = bitBuffer[2];
                        texBuffer[k + 15] = bitBuffer[3];
                    }
                }
                ret.LoadRawTextureData((byte[])texBuffer);
                ret.Apply();
                return ret;
            }
        }

        internal List<Texture2D> blendTextures;
        internal List<Texture2D> warpTextures;
        internal List<Block> blocks;
        Stream fileStreamIn;

        public ViosoVwfReader()
        {
            blocks = new List<Block>();
            blendTextures = new List<Texture2D>();
            warpTextures = new List<Texture2D>();
        }

        public bool ReadVwf(string fileName)
        {
            blocks = new List<Block>();
            blendTextures = new List<Texture2D>();
            warpTextures = new List<Texture2D>();

            fileStreamIn = File.Open(fileName, FileMode.Open);
            fileStreamIn.Position = 0;
            byte[] buffer = new byte[4];

            long cc = fileStreamIn.Length - 4;

            while (fileStreamIn.CanRead && fileStreamIn.Position < cc - 4)
            {
                long i = fileStreamIn.Position;
                fileStreamIn.Read(buffer, 0, 4);
                string head = System.Text.Encoding.UTF8.GetString(buffer);
                if (head.Length >= 4 && string.Equals(head.Substring(0, 3), "vwf"))
                {
                    if (string.Equals(head, "vwf1"))
                    {
                        Vwf1 block = new Vwf1(i);
                        blocks.Add(block);
                        block.GetData(fileStreamIn);
                        if (block.Offs + block.Indice < fileStreamIn.Position)
                        {
                            Debug.LogError("HEVS: vwf1 offs: " + block.Offs + ", Index: " + block.Indice);
                            return false;
                        }
                        fileStreamIn.Position = block.Offs + block.Indice;
                    }
                    else if (string.Equals(head, "vwf2") || string.Equals(head, "vwf0"))
                    {
                        Vwf2 block = new Vwf2(i);
                        blocks.Add(block);
                        block.GetData(fileStreamIn);
                        blendTextures.Add(block.GetBlendTexture(fileStreamIn));
                        warpTextures.Add(block.GetWarpTexture(fileStreamIn));
                        fileStreamIn.Position = block.SizeHeader + block.Size + block.Indice + block.BmpSize;
                    }
                }
                else
                {
                    Debug.LogError("HEVS: found something unexpected in vioso file!..");
                    return false;
                }
            }
            return (blocks != null && blocks.Count > 1);
        }
    }
}
