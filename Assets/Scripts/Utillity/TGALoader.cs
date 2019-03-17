#region

using System;
using System.IO;
using UnityEngine;

#endregion

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
        using (var r = new BinaryReader(TGAStream))
        {
            // Skip some header info we don't care about.
            // Even if we did care, we have to move the stream seek point to the beginning,
            // as the previous method in the workflow left it at the end.
            r.BaseStream.Seek(12, SeekOrigin.Begin);

            var width = r.ReadInt16();
            var height = r.ReadInt16();
            int bitDepth = r.ReadByte();

            // Skip a byte of header information we don't care about.
            r.BaseStream.Seek(1, SeekOrigin.Current);

            Texture2D tex;
            var pulledColors = new Color[width * height];

            switch (bitDepth)
            {
                case 32:
                {
                    tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                    for (var i = 0; i < width * height; i++)
                    {
                        var red = r.ReadByte() / 256f;
                        var green = r.ReadByte() / 256f;
                        var blue = r.ReadByte() / 256f;
                        var alpha = r.ReadByte() / 256f;

                        pulledColors[i] = new Color(blue, green, red, alpha);
                    }

                    break;
                }
                case 24:
                {
                    tex = new Texture2D(width, height, TextureFormat.RGB24, false, false);
                    for (var i = 0; i < width * height; i++)
                    {
                        var red = r.ReadByte() / 256f;
                        var green = r.ReadByte() / 256f;
                        var blue = r.ReadByte() / 256f;

                        pulledColors[i] = new Color(blue, green, red, 1);
                    }

                    break;
                }
                default:
                    throw new Exception("TGA texture had non 32/24 bit depth.");
            }

            tex.SetPixels(pulledColors);
            tex.Apply(false);
            return TextureProcessing.ConvertToStandard(tex);
        }
    }
}