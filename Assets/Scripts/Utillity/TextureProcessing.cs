using System;
using System.IO;
using General;
using UnityEngine;

public static class TextureProcessing
{
    private static readonly int MetallicTex = Shader.PropertyToID("_MetallicTex");
    private static readonly int SmoothnessTex = Shader.PropertyToID("_SmoothnessTex");
    private static readonly int AoTex = Shader.PropertyToID("_AoTex");

    public static Texture2D BlitMaskMap(Texture2D metallicMap, Texture2D smoothnessMap, Texture2D aoMap)
    {
        Vector2Int size;
        if (metallicMap)
            size = new Vector2Int(metallicMap.width, metallicMap.height);
        else if (smoothnessMap)
            size = new Vector2Int(smoothnessMap.width, smoothnessMap.height);
        else if (aoMap)
            size = new Vector2Int(aoMap.width, aoMap.height);
        else return null;

        var textureBlack = new Texture2D(1, 1);
        textureBlack.SetPixel(0, 0, Color.black);
        textureBlack.Resize(size.x, size.y, TextureFormat.ARGB32, true);

        var mat = new Material(Shader.Find("Blit/Blit_MaskMap"));
        mat.SetTexture(MetallicTex, metallicMap ? metallicMap : textureBlack);
        mat.SetTexture(SmoothnessTex, smoothnessMap ? smoothnessMap : textureBlack);
        mat.SetTexture(AoTex, aoMap ? aoMap : textureBlack);
        var emptyTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        var map = TextureManager.GetTempRenderTexture(size.x, size.y);
        Graphics.Blit(emptyTex, map, mat);
        RenderTexture.active = map;
        var maskMap = TextureManager.Instance.GetStandardTexture(map.width, map.height);
        maskMap.ReadPixels(new Rect(0, 0, map.width, map.height), 0, 0);
        maskMap.Apply(false);
        RenderTexture.ReleaseTemporary(map);
        return maskMap;
    }

    public static Texture2D FlipNormalMapY(Texture2D normalMap)
    {
        if (normalMap == null) return null;
        for (var i = 0; i < normalMap.width; i++)
        for (var j = 0; j < normalMap.height; j++)
        {
            var pixelColor = normalMap.GetPixel(i, j);
            pixelColor.g = 1.0f - pixelColor.g;
            normalMap.SetPixel(i, j, pixelColor);
        }

        normalMap.Apply(false);

        return normalMap;
    }

    private static Texture2D LoadPngBmpJpg(string path)
    {
        var newTexture = TextureManager.Instance.GetStandardTexture(2, 2);
        if (!File.Exists(path)) return null;

        var fileData = File.ReadAllBytes(path);

        newTexture.LoadImage(fileData);

        return newTexture;
    }


    private static ProgramEnums.FileFormat GetFormat(string path)
    {
        var format = path.Substring(path.LastIndexOf(".", StringComparison.Ordinal) + 1, 3);
        Debug.Log($"Carregando {format}");

        if (!Enum.TryParse(format, true, out ProgramEnums.FileFormat fileFormat))
            return ProgramEnums.FileFormat.Invalid;

        Debug.Log("Tipo encontrado : " + fileFormat);
        return fileFormat;
    }

    public static Texture2D GetTextureFromFile(string pathToFile)
    {
        var fileFormat = TextureProcessing.GetFormat(pathToFile);

        Texture2D newTexture = null;
        switch (fileFormat)
        {
            case ProgramEnums.FileFormat.Png:
            case ProgramEnums.FileFormat.Jpg:
            case ProgramEnums.FileFormat.Bmp:
                newTexture = TextureProcessing.LoadPngBmpJpg(pathToFile);
                break;
            case ProgramEnums.FileFormat.Tga:
                var tex = TextureManager.Instance.GetStandardTexture(2, 2);
                newTexture = TGALoader.LoadTGA(pathToFile);
                break;
            case ProgramEnums.FileFormat.Exr:
            case ProgramEnums.FileFormat.Invalid:
                Debug.Log("Tipo de arquivo invalido " + fileFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return newTexture;
    }
}