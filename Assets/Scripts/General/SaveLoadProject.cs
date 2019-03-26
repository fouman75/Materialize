#region

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Gui;
using Plugins.Extension;
using Settings;
using UnityEngine;
using Graphics = UnityEngine.Graphics;

#if UNITY_STANDALONE_WIN
using System.Windows.Forms;
using System.Drawing.Imaging;

#endif

#endregion

namespace General
{
    public class SaveLoadProject : MonoBehaviour
    {
        public static SaveLoadProject Instance;
        private char _pathChar;
        public ProjectObject ThisProject;

        // Use this for initialization
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _pathChar = ProgramManager.Instance.PathChar;
            ThisProject = new ProjectObject();
        }

        public void LoadProject(string pathToFile)
        {
            Logger.Log("Loading Project: " + pathToFile);

            var serializer = new XmlSerializer(typeof(ProjectObject));
            using (var stream = new FileStream(pathToFile, FileMode.Open))
            {
                ThisProject = serializer.Deserialize(stream) as ProjectObject;
                stream.Close();
            }

            MainGui.Instance.HeightFromDiffuseGuiScript.SetValues(ThisProject);
            MainGui.Instance.EditDiffuseGuiScript.SetValues(ThisProject);
            MainGui.Instance.NormalFromHeightGuiScript.SetValues(ThisProject);
            MainGui.Instance.MetallicGuiScript.SetValues(ThisProject);
            MainGui.Instance.SmoothnessGuiScript.SetValues(ThisProject);
            MainGui.Instance.AoFromNormalGuiScript.SetValues(ThisProject);
            MainGui.Instance.MaterialGuiScript.SetValues(ThisProject);

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

            MainGui.Instance.HeightFromDiffuseGuiScript.GetValues(ThisProject);
            if (TextureManager.Instance.HeightMap != null)
                ThisProject.HeightMapPath = projectName + "_height." + extension;
            else
                ThisProject.HeightMapPath = "null";

            MainGui.Instance.EditDiffuseGuiScript.GetValues(ThisProject);
            if (TextureManager.Instance.DiffuseMap != null)
                ThisProject.DiffuseMapPath = projectName + "_diffuse." + extension;
            else
                ThisProject.DiffuseMapPath = "null";

            if (TextureManager.Instance.DiffuseMapOriginal != null)
                ThisProject.DiffuseMapOriginalPath = projectName + "_diffuseOriginal." + extension;
            else
                ThisProject.DiffuseMapOriginalPath = "null";

            MainGui.Instance.NormalFromHeightGuiScript.GetValues(ThisProject);
            if (TextureManager.Instance.NormalMap != null)
                ThisProject.NormalMapPath = projectName + "_normal." + extension;
            else
                ThisProject.NormalMapPath = "null";

            MainGui.Instance.MetallicGuiScript.GetValues(ThisProject);
            if (TextureManager.Instance.MetallicMap != null)
                ThisProject.MetallicMapPath = projectName + "_metallic." + extension;
            else
                ThisProject.MetallicMapPath = "null";

            MainGui.Instance.SmoothnessGuiScript.GetValues(ThisProject);
            if (TextureManager.Instance.SmoothnessMap != null)
                ThisProject.SmoothnessMapPath = projectName + "_smoothness." + extension;
            else
                ThisProject.SmoothnessMapPath = "null";

            if (TextureManager.Instance.MaskMap != null)
                ThisProject.MaskMapPath = projectName + "_maskMap." + extension;
            else
                ThisProject.MaskMapPath = "null";

            MainGui.Instance.AoFromNormalGuiScript.GetValues(ThisProject);
            if (TextureManager.Instance.AoMap != null)
                ThisProject.AoMapPath = projectName + "_ao." + extension;
            else
                ThisProject.AoMapPath = "null";

            MainGui.Instance.MaterialGuiScript.GetValues(ThisProject);

            var serializer = new XmlSerializer(typeof(ProjectObject));
            using (var stream = new FileStream(pathToFile + ".mtz", FileMode.Create))
            {
                serializer.Serialize(stream, ThisProject);
                stream.Close();
            }

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
            var pathToFile = GetClipboardImagePath();
            if (pathToFile.IsNullOrEmpty()) return;

            if (!File.Exists(pathToFile)) return;

            StartCoroutine(LoadTexture(mapTypeToLoad, pathToFile));
        }

        //==============================================//
        //			Texture Saving Coroutines			//
        //==============================================//


        private IEnumerator SaveAllTextures(string pathToFile)
        {
            var path = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);
            ThisProject.ProjectPath = path;
            yield return StartCoroutine(SaveTexture(TextureManager.Instance.HeightMap,
                path + ThisProject.HeightMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.DiffuseMap,
                path + ThisProject.DiffuseMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.DiffuseMapOriginal,
                path + ThisProject.DiffuseMapOriginalPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.NormalMap,
                path + ThisProject.NormalMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.MetallicMap,
                path + ThisProject.MetallicMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.SmoothnessMap,
                path + ThisProject.SmoothnessMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.MaskMap, path + ThisProject.MaskMapPath));

            yield return StartCoroutine(SaveTexture(TextureManager.Instance.AoMap, path + ThisProject.AoMapPath));

            ClearPanelQuickSave();
        }

        public IEnumerator SaveTexture(string extension, Texture2D textureToSave, string pathToFile)
        {
            yield return StartCoroutine(SaveTexture(textureToSave, pathToFile + "." + extension));
        }

        private IEnumerator SaveTexture(Texture2D textureToSave, string pathToFile)
        {
            if (!textureToSave || string.IsNullOrEmpty(pathToFile)) yield break;

            while (!ProgramManager.Lock()) yield return null;

            MessagePanel.ShowMessage($"Saving to {pathToFile}");

            Logger.Log($"Salvando {textureToSave} como {pathToFile}");
            if (!textureToSave.isReadable)
            {
                Logger.LogError($"Texture {pathToFile} somente leitura");
                MessagePanel.HideMessage();
                ProgramManager.Unlock();
                yield break;
            }

            if (!pathToFile.Contains(".")) pathToFile = $"{pathToFile}.{MainGui.Instance.SelectedFormat}";

            var fileIndex = pathToFile.LastIndexOf('.');
            var extension = pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

            if (File.Exists(pathToFile)) File.Delete(pathToFile);

            var isHdr = textureToSave.format == TextureFormat.RGBAFloat ||
                        textureToSave.format == TextureFormat.RGBAHalf;

            var renderTexture = TextureManager.Instance.GetTempRenderTexture(textureToSave.width, textureToSave.height);

            Graphics.Blit(textureToSave, renderTexture);
            RenderTexture.active = renderTexture;
            textureToSave = TextureManager.Instance.GetStandardTexture(renderTexture.width, renderTexture.height);
            textureToSave.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
            textureToSave.Apply(false);

            if (extension != "exr" || !isHdr) textureToSave = TextureProcessing.ConvertToGama(textureToSave);

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
                    bytes = ImageConversion.EncodeToTGA(textureToSave);
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

            if (bytes == null)
            {
                MessagePanel.HideMessage();
                ProgramManager.Unlock();
                yield break;
            }

            File.WriteAllBytes(pathToFile, bytes);

            yield return new WaitForSeconds(0.1f);

            MessagePanel.HideMessage();
            ProgramManager.Unlock();
        }

        //==============================================//
        //			Texture Loading Coroutines			//
        //==============================================//

        private IEnumerator LoadAllTextures(string pathToFile)
        {
            pathToFile = pathToFile.Substring(0, pathToFile.LastIndexOf(_pathChar) + 1);
            ThisProject.ProjectPath = pathToFile;

            if (ThisProject.HeightMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Height, pathToFile + ThisProject.HeightMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.DiffuseMapOriginalPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.DiffuseOriginal,
                    pathToFile + ThisProject.DiffuseMapOriginalPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.DiffuseMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Diffuse, pathToFile + ThisProject.DiffuseMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.NormalMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Normal, pathToFile + ThisProject.NormalMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.MetallicMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Metallic, pathToFile + ThisProject.MetallicMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.SmoothnessMapPath != "null")
                StartCoroutine(
                    LoadTexture(ProgramEnums.MapType.Smoothness, pathToFile + ThisProject.SmoothnessMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.MaskMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.MaskMap, pathToFile + ThisProject.MaskMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            if (ThisProject.AoMapPath != "null")
                StartCoroutine(LoadTexture(ProgramEnums.MapType.Ao, pathToFile + ThisProject.AoMapPath));

            while (ProgramManager.IsLocked) yield return new WaitForSeconds(0.01f);

            yield return new WaitForSeconds(0.01f);
            TextureManager.Instance.SetFullMaterialAndUpdate();

            ClearPanelQuickSave();
        }

        public IEnumerator LoadTexture(ProgramEnums.MapType textureToLoad, string pathToFile)
        {
            if (pathToFile.IsNullOrEmpty()) yield break;

            while (!ProgramManager.Lock()) yield return null;
            MessagePanel.ShowMessage($"Loading {pathToFile}");

            var newTexture = TextureProcessing.GetTextureFromFile(pathToFile);

            if (newTexture && newTexture.format != TextureManager.Instance.DefaultTextureFormat)
                newTexture = TextureProcessing.ConvertToStandard(newTexture);

            if (!newTexture)
            {
                MessagePanel.HideMessage();
                ProgramManager.Unlock();
                yield break;
            }

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

            MessagePanel.HideMessage();
            ProgramManager.Unlock();
        }

        private static void ClearPanelQuickSave()
        {
            var panels = FindObjectsOfType<TexturePanel>();
            foreach (var panel in panels) panel.QuickSavePath = null;
        }

#if UNITY_STANDALONE_WIN
        private static string GetClipboardImagePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

            string pathToFile;
            if (Clipboard.ContainsFileDropList())
            {
                var file = Clipboard.GetFileDropList()[0];
                pathToFile = Path.GetFullPath(file);
                return pathToFile;
            }

            if (!Clipboard.ContainsImage()) return null;

            pathToFile = Path.GetTempFileName() + ".png";
            var image = Clipboard.GetImage();
            if (image == null) return null;
            image.Save(pathToFile, ImageFormat.Png);
            return pathToFile;
        }
#endif
#if UNITY_STANDALONE_LINUX
        private static string GetClipboardImagePath()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return null;
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

                var supported = ProgramManager.LoadFormats.Any(format => bashOut.Contains(format));
                if (!supported) return null;

                if (bashOut.Contains(filePrefix)) bashOut = bashOut.Replace(filePrefix, "/");

                var firstIndex = bashOut.IndexOf('/');
                if (bashOut.Length > firstIndex)
                {
                    var extension = Path.GetExtension(bashOut);
                    var lastIndex = bashOut.IndexOf(extension, firstIndex, StringComparison.Ordinal) + extension.Length;
                    var length = lastIndex - firstIndex;
                    pathToFile = bashOut.Substring(firstIndex, length);
                }
                else
                {
                    return null;
                }
            }

            File.Delete(pathToTextFile);

            return pathToFile;
        }
#endif
    }
}