// This was made by aaro4130 on the Unity forums.  Thanks boss!
// It's been optimized and slimmed down for the purpose of loading Quake 3 TGA textures from memory streams.

using System;
using System.IO;
using General;
using UnityEngine;

// ReSharper disable InconsistentNaming

public static class TGALoader
{
    public static Texture2D LoadTGA(string fileName)
    {
        if (!File.Exists(fileName)) return null;

        using (var imageFile = File.OpenRead(fileName))
        {
            return LoadTGA(imageFile);
        }
    }

    private static Texture2D LoadTGA(Stream TGAStream)
    {
        using (BinaryReader r = new BinaryReader(TGAStream))
        {
            // Skip some header info we don't care about.
            // Even if we did care, we have to move the stream seek point to the beginning,
            // as the previous method in the workflow left it at the end.
            r.BaseStream.Seek(12, SeekOrigin.Begin);

            short width = r.ReadInt16();
            short height = r.ReadInt16();
            int bitDepth = r.ReadByte();

            // Skip a byte of header information we don't care about.
            r.BaseStream.Seek(1, SeekOrigin.Current);

            var pulledColors = new Color32[width * height];

            switch (bitDepth)
            {
                case 32:
                {
                    for (var i = 0; i < width * height; i++)
                    {
                        var red = r.ReadByte();
                        var green = r.ReadByte();
                        var blue = r.ReadByte();
                        var alpha = r.ReadByte();

                        pulledColors[i] = new Color32(blue, green, red, alpha);
                    }

                    break;
                }
                case 24:
                {
                    for (var i = 0; i < width * height; i++)
                    {
                        var red = r.ReadByte();
                        var green = r.ReadByte();
                        var blue = r.ReadByte();

                        pulledColors[i] = new Color32(blue, green, red, 1);
                    }

                    break;
                }
                default:
                    throw new Exception("TGA texture had non 32/24 bit depth.");
            }

            var tex = TextureManager.Instance.GetStandardTexture(width, height);
            tex.SetPixels32(pulledColors);
            tex.Apply(false);
            return tex;
        }
    }
}