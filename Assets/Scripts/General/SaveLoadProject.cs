#region

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Gui;
using Settings;
using UnityEngine;
using StringExt = Plugins.Extension.StringExt;

#endregion

namespace General
{
    public class SaveLoadProject : MonoBehaviour
    {
        public static SaveLoadProject Instance;
        private char _pathChar;
        private ProjectObject _thisProject;

        [HideInInspector] public bool Busy;

        // Use this for initialization
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _pathChar = ProgramManager.Instance.PathChar;
            _thisProject = new ProjectObject();
        }

        public void LoadProject(string pathToFile)
        {
            Logger.Log("Loading Project: " + pathToFile);

            var serializer = new XmlSerializer(typeof(ProjectObject));
            var stream = new FileStream(pathToFile, FileMode.Open);
            _thisProject = serializer.Deserialize(stream) as ProjectObject;
            stream.Close();
            MainGui.Instance.HeightFromDiffuseGuiScript.SetValues(_thisProject);
            MainGui.Instance.EditDiffuseGuiScript.SetValues(_thisProject);
            MainGui.Instance.NormalFromHeightGuiScript.SetValues(_thisProject);
            MainGui.Instance.MetallicGuiScript.SetValues(_thisProject);
            MainGui.Instance.SmoothnessGuiScript.SetValues(_thisProject);
            MainGui.Instance.AoFromNormalGuiScript.SetValues(_thisProject);
            MainGui.Instance.MaterialGuiScript.SetValues(_thisProject);

            TextureManager.Instance.ClearAllTextures();

            StartCoroutine(LoadAllTextures(pathToFile));
        }

        public void SaveProject(string pathToFile)
        {
            var projectName = Path.GetFileNameWithoutExtension(pathToFile);
            if (Path.HasExtension(pathToFile))
                pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(".", StringComparison.Ordinal));

            Logger.Log("Saving Project: " + pathToFile);

            var extension = MainGui.Instance.SelectedFormat.ToString().ToLower();

            Logger.Log("Project Name " + projectName);

            MainGui.Instance.HeightFromDiffuseGuiScript.GetValues(_thisProject);
            if (TextureManager.Instance.HeightMap != null)
                _thisProject.HeightMapPath = projectName + "_height." + extension;
            else
                _thisProject.HeightMapPath = "null";

            MainGui.Instance.EditDiffuseGuiScript.GetValues(_thisProject);
            if (TextureManager.Instance.DiffuseMap != null)
                _thisProject.DiffuseMapPath = projectName + "_diffuse." + extension;
            else
                _thisProject.DiffuseMapPath = "null";

            if (TextureManager.Instance.DiffuseMapOriginal != null)
                _thisProject.DiffuseMapOriginalPath = projectName + "_diffuseOriginal." + extension;
            else
                _thisProject.DiffuseMapOriginalPath = "null";

            MainGui.Instance.NormalFromHeightGuiScript.GetValues(_thisProject);
            if (TextureManager.Instance.NormalMap != null)
                _thisProject.NormalMapPath = projectName + "_normal." + extension;
            else
                _thisProject.NormalMapPath = "null";

            MainGui.Instance.MetallicGuiScript.GetValues(_thisProject);
            if (TextureManager.Instance.MetallicMap != null)
                _thisProject.MetallicMapPath = projectName + "_metallic." + extension;
            else
                _thisProject.MetallicMapPath = "null";

            MainGui.Instance.SmoothnessGuiScript.GetValues(_thisProject);
            if (TextureManager.Instance.SmoothnessMap != null)
                _thisProject.SmoothnessMapPath = projectName + "_smoothness." + extension;
            else
                _thisProject.SmoothnessMapPath = "null";

            if (TextureManager.Instance.MaskMap != null)
                _thisProject.MaskMapPath = projectName + "_maskMap." + extension;
            else
                _thisProject.MaskMapPath = "null";

            MainGui.Instance.AoFromNormalGuiScript.GetValues(_thisProject);
            if (TextureManager.Instance.AoMap != null)
                _thisProject.AoMapPath = projectName + "_ao." + extension;
            else
                _thisProject.AoMapPath = "null";

            MainGui.Instance.MaterialGuiScript.GetValues(_thisProject);

            var serializer = new XmlSerializer(typeof(ProjectObject));
            var stream = new FileStream(pathToFile + ".mtz", FileMode.Create);
            serializer.Serialize(stream, _thisProject);
            stream.Close();

            SaveAllFiles(pathToFile);
        }

        private void SaveAllFiles(string pathToFile)
        {
            StartCoroutine(SaveAllTextures(pathToFile));
        }

        public void SaveFile(string pathToFile, Texture2D textureToSave)
        {
            StartCoroutine(SaveTexture(textureToSave, pathToFile));
        }

        public void PasteFile(ProgramEnums.MapType mapTypeToLoad)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
            const string filePrefix = "file:///";
            string pathToFile;

            var pathToTextFile = Path.GetTempFileName();
            BashRunner.Run($"xclip -selection clipboard -t TARGETS -o > {pathToTextFile}");
            var bashOut = File.ReadAllText(pathToTextFile);

//            General.Logger.Log($"Out : {bashOut}");
            File.Delete(pathToTextFile);

            if (bashOut.Contains("image/png"))
            {
                pathToFile = Path.GetTempFileName() + ".png";
                BashRunner.Run($"xclip -selection clipboard -t image/png -o > {pathToFile}");
            }
            else
            {
                BashRunner.Run($"xclip -selection clipboard -o > {pathToTextFile}");
                bashOut = File.ReadAllText(pathToTextFile);

                if (!bashOut.Contains(filePrefix)) return;
                var supported = ProgramManager.LoadFormats.Any(format => bashOut.Contains(format));
                if (!supported) return;

                var firstIndex = bashOut.IndexOf("file:///", StringComparison.Ordinal);
                var lastIndex = bashOut.IndexOf("\n", firstIndex, StringComparison.Ordinal);
                var length = lastIndex - firstIndex;
                pathToFile = bashOut.Substring(firstIndex, length);
                pathToFile = pathToFile.Replace("file:///", "/");
            }

            File.Delete(pathToTextFile);


            StartCoroutine(LoadTexture(mapTypeToLoad, pathToFile));
        }

        //==============================================//
        //			Texture Saving Coroutines			//
        //==============================================//


        private IEnumerator SaveAllTextures(string pathToFile)
        {
            var path = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);
            yield return StartCoroutine(SaveTexture(TextureManager.Instance.HeightMap,
                path + _thisProject.HeightMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.DiffuseMap,
                path + _thisProject.DiffuseMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.DiffuseMapOriginal,
                path + _thisProject.DiffuseMapOriginalPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.NormalMap,
                path + _thisProject.NormalMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.MetallicMap,
                path + _thisProject.MetallicMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.SmoothnessMap,
                path + _thisProject.SmoothnessMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.MaskMap, path + _thisProject.MaskMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.AoMap, path + _thisProject.AoMapPath));
        }

        public IEnumerator SaveTexture(string extension, Texture2D textureToSave, string pathToFile)
        {
            yield return StartCoroutine(SaveTexture(textureToSave, pathToFile + "." + extension));
        }

        private IEnumerator SaveTexture(Texture2D textureToSave, string pathToFile)
        {
            if (!textureToSave || StringExt.IsNullOrEmpty(pathToFile)) yield break;
            Logger.Log($"Salvando {textureToSave} como {pathToFile}");
            if (!textureToSave.isReadable) Logger.LogError($"Texture {pathToFile} somente leitura");

            if (!pathToFile.Contains(".")) pathToFile = $"{pathToFile}.{MainGui.Instance.SelectedFormat}";

            var fileIndex = pathToFile.LastIndexOf('.');
            var extension = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

            if (File.Exists(pathToFile)) File.Delete(pathToFile);

            var isHdr = textureToSave.format == TextureFormat.RGBAFloat ||
                        textureToSave.format == TextureFormat.RGBAHalf;

            var renderTexture = TextureManager.Instance.GetTempRenderTexture(textureToSave.width, textureToSave.height);

            Graphics.Blit(textureToSave, renderTexture);
            RenderTexture.active = renderTexture;
            textureToSave.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
            textureToSave.Apply(false);

            if (extension != "exr" || !isHdr)
            {
                textureToSave = TextureProcessing.ConvertToGama(textureToSave);
            }

            byte[] bytes;
            switch (extension)
            {
                case "png":
                {
                    bytes = textureToSave.EncodeToPNG();
                    break;
                }
                case "jpg":
                {
                    bytes = textureToSave.EncodeToJPG();
                    break;
                }
                case "tga":
                {
                    bytes = textureToSave.EncodeToTGA();
                    break;
                }
                case "exr":
                {
                    const Texture2D.EXRFlags flags = Texture2D.EXRFlags.CompressZIP;
                    bytes = textureToSave.EncodeToEXR(flags);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(extension), extension, null);
            }

            if (bytes == null) yield break;
            File.WriteAllBytes(pathToFile, bytes);

            Resources.UnloadUnusedAssets();


            yield return new WaitForSeconds(0.1f);
        }

        //==============================================//
        //			Texture Loading Coroutines			//
        //==============================================//

        private IEnumerator LoadAllTextures(string pathToFile)
        {
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);

            if (_thisProject.HeightMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Height, pathToFile + _thisProject.HeightMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.DiffuseMapOriginalPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.DiffuseOriginal,
                    pathToFile + _thisProject.DiffuseMapOriginalPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.DiffuseMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Diffuse, pathToFile + _thisProject.DiffuseMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.NormalMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Normal, pathToFile + _thisProject.NormalMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.MetallicMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Metallic, pathToFile + _thisProject.MetallicMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.SmoothnessMapPath != "null")
                StartCoroutine(
                    LoadTexture(ProgramEnums.MapType.Smoothness, pathToFile + _thisProject.SmoothnessMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.MaskMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.MaskMap, pathToFile + _thisProject.MaskMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            if (_thisProject.AoMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Ao, pathToFile + _thisProject.AoMapPath));

            while (Busy) yield return new WaitForSeconds(0.01f);

            yield return new WaitForSeconds(0.01f);
            TextureManager.Instance.SetFullMaterial();
        }

        public IEnumerator LoadTexture(ProgramEnums.MapType textureToLoad, string pathToFile)
        {
            Busy = true;

            if (StringExt.IsNullOrEmpty(pathToFile)) yield break;

            var newTexture = TextureProcessing.GetTextureFromFile(pathToFile);
            if (!newTexture) yield break;

            if (!Mathf.IsPowerOfTwo(newTexture.width))
            {
                var size = Mathf.NextPowerOfTwo(newTexture.width);
                newTexture.Resize(size, size);
            }

            if (newTexture && newTexture.format != TextureManager.DefaultHdrTextureFormat)
            {
                newTexture = TextureProcessing.ConvertToStandard(newTexture);
            }

            if (!newTexture) yield break;
            newTexture.anisoLevel = 9;

            switch (textureToLoad)
            {
                case ProgramEnums.MapType.Height:
                    TextureManager.Instance.HeightMap = newTexture;
                    break;
                case ProgramEnums.MapType.Diffuse:
                    TextureManager.Instance.DiffuseMap = newTexture;
                    break;
                case ProgramEnums.MapType.AnyDiffuse:
                case ProgramEnums.MapType.DiffuseOriginal:
                    TextureManager.Instance.DiffuseMapOriginal = newTexture;
                    break;
                case ProgramEnums.MapType.Normal:
                    TextureManager.Instance.NormalMap = newTexture;
                    break;
                case ProgramEnums.MapType.Metallic:
                    TextureManager.Instance.MetallicMap = newTexture;
                    break;
                case ProgramEnums.MapType.Smoothness:
                    TextureManager.Instance.SmoothnessMap = newTexture;
                    break;
                case ProgramEnums.MapType.MaskMap:
                    TextureManager.Instance.MaskMap = newTexture;
                    break;
                case ProgramEnums.MapType.Ao:
                    TextureManager.Instance.AoMap = newTexture;
                    break;
                case ProgramEnums.MapType.Property:
                    break;
                case ProgramEnums.MapType.None:
                    break;
                case ProgramEnums.MapType.Any:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(textureToLoad), textureToLoad, null);
            }

            Resources.UnloadUnusedAssets();

            yield return new WaitForSeconds(0.01f);

            Busy = false;
        }
    }
}