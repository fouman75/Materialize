#region

using System;
using System.IO;
using General;
using UnityEngine;
using Logger = General.Logger;
using Object = UnityEngine.Object;

#endregion

public static class TextureProcessing
{
    private static readonly int MetallicTex = Shader.PropertyToID("MetallicInput");
    private static readonly int SmoothnessTex = Shader.PropertyToID("SmoothnessInput");
    private static readonly int AoTex = Shader.PropertyToID("AoInput");

    public static Texture2D BlitMaskMap(Texture2D metallicMap, Texture2D smoothnessMap, Texture2D aoMap)
    {
        var renderMaskMap = RenderMaskMap(metallicMap, smoothnessMap, aoMap);

        RenderTexture.active = renderMaskMap;
        var maskMap = TextureManager.Instance.GetStandardTexture(renderMaskMap.width, renderMaskMap.height);
        maskMap.ReadPixels(new Rect(0, 0, renderMaskMap.width, renderMaskMap.height), 0, 0);
        maskMap.Apply(true);
        RenderTexture.ReleaseTemporary(renderMaskMap);

        return maskMap;
    }

    public static RenderTexture RenderMaskMap(Texture2D metallicMap, Texture2D smoothnessMap, Texture2D aoMap)
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
        textureBlack.Apply(true);

        var maskMapCompute = TextureManager.Instance.MaskMapCompute;
        var kernel = maskMapCompute.FindKernel("CSMaskMap");

        maskMapCompute.SetTexture(kernel, MetallicTex, metallicMap ? metallicMap : textureBlack);
        maskMapCompute.SetTexture(kernel, SmoothnessTex, smoothnessMap ? smoothnessMap : textureBlack);
        maskMapCompute.SetTexture(kernel, AoTex, aoMap ? aoMap : textureBlack);

        var renderMaskMap = TextureManager.Instance.GetTempRenderTexture(size.x, size.y);
        maskMapCompute.SetVector("_ImageSize", (Vector2) size);
        maskMapCompute.SetTexture(kernel, "Result", renderMaskMap);
        var groupsX = (int) Mathf.Ceil(size.x / 8f);
        var groupsY = (int) Mathf.Ceil(size.y / 8f);
        maskMapCompute.Dispatch(kernel, groupsX, groupsY, 1);


        return renderMaskMap;
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

    public static Texture2D ConvertToGama(Texture2D texture)
    {
        var compute = TextureManager.Instance.TextureProcessingCompute;
        var kernel = compute.FindKernel("ConvertToGama");
        var renderTexture = TextureManager.Instance.GetTempRenderTexture(texture.width, texture.height);
        var imageSizeX = texture.width;
        var imageSizeY = texture.height;
        compute.SetVector("_ImageSize", new Vector2(imageSizeX, imageSizeY));
        compute.SetTexture(kernel, "Input", texture);
        compute.SetTexture(kernel, "Result", renderTexture);
        var groupsX = (int) Mathf.Ceil(imageSizeX / 8f);
        var groupsY = (int) Mathf.Ceil(imageSizeY / 8f);
        compute.Dispatch(kernel, groupsX, groupsY, 1);

        RenderTexture.active = renderTexture;
        var tex = TextureManager.Instance.GetStandardTexture(texture.width, texture.height);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply(false);
        RenderTexture.ReleaseTemporary(renderTexture);

        return tex;
    }

    private static Texture2D LoadPngBmpJpg(string path)
    {
        var newTexture = new Texture2D(2, 2);
        if (!File.Exists(path)) return null;

        var fileData = File.ReadAllBytes(path);

        newTexture.LoadImage(fileData);
//        newTexture = ConvertToLinear(newTexture);

        return ConvertToStandard(newTexture);
    }


    private static ProgramEnums.FileFormat GetFormat(string path)
    {
        var format = path.Substring(path.LastIndexOf(".", StringComparison.Ordinal) + 1, 3);
        Logger.Log($"Carregando {format}");

        if (!Enum.TryParse(format, true, out ProgramEnums.FileFormat fileFormat))
            return ProgramEnums.FileFormat.Invalid;

        Logger.Log("Tipo encontrado : " + fileFormat);
        return fileFormat;
    }

    public static Texture2D GetTextureFromFile(string pathToFile)
    {
        pathToFile = Uri.UnescapeDataString(pathToFile);

        var fileFormat = GetFormat(pathToFile);

        Texture2D newTexture = null;
        switch (fileFormat)
        {
            case ProgramEnums.FileFormat.Png:
            case ProgramEnums.FileFormat.Jpg:
            case ProgramEnums.FileFormat.Bmp:
                newTexture = LoadPngBmpJpg(pathToFile);
                break;
            case ProgramEnums.FileFormat.Tga:
                newTexture = TGALoader.LoadTGA(pathToFile);
                break;
            case ProgramEnums.FileFormat.Exr:
            case ProgramEnums.FileFormat.Invalid:
                Logger.Log("Tipo de arquivo invalido " + fileFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return newTexture;
    }

    public static Texture2D ConvertToStandard(Texture2D newTexture, bool linear = false)
    {
        var tempRenderTexture = TextureManager.Instance.GetTempRenderTexture(newTexture.width, newTexture.height);

        Graphics.CopyTexture(newTexture, 0, 0, tempRenderTexture, 0, 0);
        TextureManager.Instance.GetTextureFromRender(tempRenderTexture, out var converted);
        Object.Destroy(newTexture);
        return converted;
    }
}