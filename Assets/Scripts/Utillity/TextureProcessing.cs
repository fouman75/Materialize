using UnityEngine;

namespace Utility
{
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
            textureBlack.Resize(size.x, size.y, TextureFormat.ARGB32, false);

            var mat = new Material(Shader.Find("Blit/Blit_MaskMap"));
            mat.SetTexture(MetallicTex, metallicMap ? metallicMap : textureBlack);
            mat.SetTexture(SmoothnessTex, smoothnessMap ? smoothnessMap : textureBlack);
            mat.SetTexture(AoTex, aoMap ? aoMap : textureBlack);
            var emptyTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            var map = RenderTexture.GetTemporary(size.x, size.y, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(emptyTex, map, mat);
            RenderTexture.active = map;
            var maskMap = new Texture2D(map.width, map.height, TextureFormat.ARGB32, false, true);
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

            normalMap.Apply();

            return normalMap;
        }
    }
}