#region

using System;
using System.IO;
using UnityEngine;
using Utility;
using Logger = Utility.Logger;
using Object = UnityEngine.Object;

#endregion

namespace Materialize.General
{
    public static class TextureProcessing
    {
        private static readonly int MetallicTex = Shader.PropertyToID("MetallicInput");
        private static readonly int SmoothnessTex = Shader.PropertyToID("SmoothnessInput");
        private static readonly int AoTex = Shader.PropertyToID("AoInput");
        private static readonly int ImageSizeId = Shader.PropertyToID("_ImageSize");


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
            RunKernel(maskMapCompute, kernel, null, renderMaskMap);

            return renderMaskMap;
        }


        public static Texture2D FlipNormalMapY(Texture2D normalMap)
        {
            if (!normalMap) return null;

            var compute = TextureManager.Instance.TextureProcessingCompute;
            var kernel = compute.FindKernel("FlipNormalY");
            var renderTexture = TextureManager.Instance.GetTempRenderTexture(normalMap.width, normalMap.height);
            RunKernel(compute, kernel, normalMap, renderTexture);
            TextureManager.Instance.GetTextureFromRender(renderTexture, out normalMap);

            return normalMap;
        }

        public static Texture2D ConvertToGama(Texture2D texture)
        {
            var compute = TextureManager.Instance.TextureProcessingCompute;
            var kernel = compute.FindKernel("ConvertToGama");
            var renderTexture = TextureManager.Instance.GetTempRenderTexture(texture.width, texture.height);
            RunKernel(compute, kernel, texture, renderTexture);

            TextureManager.Instance.GetTextureFromRender(renderTexture, out var converted);
            RenderTexture.ReleaseTemporary(renderTexture);

            return converted;
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
                    newTexture = ConvertToStandard(newTexture);
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
            RenderTexture.ReleaseTemporary(tempRenderTexture);
            Object.Destroy(newTexture);
            return converted;
        }

        private static void RunKernel(ComputeShader computeShader, int kernel, Texture source, Texture destiny)
        {
            var imageSize =
                source ? new Vector2(source.width, source.height) : new Vector2(destiny.width, destiny.height);
            computeShader.SetVector(ImageSizeId, imageSize);
            if (source) computeShader.SetTexture(kernel, "ImageInput", source);
            computeShader.SetTexture(kernel, "Result", destiny);
            var groupsX = (int) Mathf.Ceil(imageSize.x / 8f);
            var groupsY = (int) Mathf.Ceil(imageSize.y / 8f);
            computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }
    }
}