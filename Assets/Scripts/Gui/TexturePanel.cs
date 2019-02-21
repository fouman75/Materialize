using General;
using Plugins.Extension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    public class TexturePanel : MonoBehaviour
    {
        public ProgramEnums.MapType MapType;

        [Header("Gui Elements")] public RawImage TextureFrame;
        public Button SaveButton;
        public Button LoadButton;
        public Button PasteButton;
        public Button CopyButton;
        public Button CreateButton;
        public Button ClearButton;

        private void Awake()
        {
            SaveButton.onClick.AddListener(SaveImage);
            LoadButton.onClick.AddListener(LoadImage);
            CopyButton.onClick.AddListener(CopyImage);
            PasteButton.onClick.AddListener(PasteImage);
            CreateButton.onClick.AddListener(CreateImage);
            ClearButton.onClick.AddListener(ClearImage);
        }

        private void Start()
        {
            if (MapType == ProgramEnums.MapType.AnyDiffuse)
            {
                var text = CreateButton.GetComponentInChildren<TextMeshProUGUI>();
                text.text = "Edit";
            }
        }

        private void Update()
        {
            SaveButton.interactable = TextureManager.Instance.NotNull(MapType);
            CopyButton.interactable = TextureManager.Instance.NotNull(MapType);
            CreateButton.interactable = TextureManager.Instance.GetCreationCondition(MapType);
            var tex = TextureManager.Instance.GetTexture(MapType);
            if (!tex) TextureFrame.texture = null;
            if (tex && TextureFrame.texture != tex) TextureFrame.texture = tex;
            TextureFrame.color = TextureFrame.texture ? Color.white : Color.black;
        }


        public void SaveImage()
        {
            var defaultName = "_" + MapType + ".png";
            var lastPath = ProgramManager.Instance.LastPath;
            StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanelAsync("Save Height Map", lastPath, defaultName,
                ProgramManager.ImageSaveFilter, SaveTextureFileCallback);
        }

        private void SaveTextureFileCallback(string path)
        {
            if (path.IsNullOrEmpty()) return;

            var lastBar = path.LastIndexOf(ProgramManager.Instance.PathChar);
            ProgramManager.Instance.LastPath = path.Substring(0, lastBar + 1);
            var textureToSave = TextureManager.Instance.GetTexture(MapType);
            SaveLoadProject.Instance.SaveFile(path, textureToSave);
        }

        public void LoadImage()
        {
            var title = "Open " + MapType + " Map";
            var lastPath = ProgramManager.Instance.LastPath;
            StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanelAsync(title, lastPath,
                ProgramManager.ImageLoadFilter, false,
                LoadTextureCallback);
        }

        private void LoadTextureCallback(string[] path)
        {
            if (path[0].IsNullOrEmpty()) return;
            var lastBar = path[0].LastIndexOf(ProgramManager.Instance.PathChar);
            ProgramManager.Instance.LastPath = path[0].Substring(0, lastBar + 1);

            TextureManager.Instance.ClearTexture(MapType);

            StartCoroutine(SaveLoadProject.Instance.LoadTexture(MapType, path[0]));
        }

        public void PasteImage()
        {
            SaveLoadProject.Instance.PasteFile(MapType);
        }

        public void CopyImage()
        {
            TextureManager.Instance.TextureInClipboard = MapType;
        }

        public void CreateImage()
        {
            MainGui.Instance.CreateImage(MapType, CreateButton);
        }

        public void ClearImage()
        {
            TextureManager.Instance.ClearTexture(MapType);
            TextureFrame.texture = null;
            MainGui.Instance.CloseWindows();
            TextureManager.Instance.SetFullMaterial();
            TextureManager.Instance.FixSize();
        }
    }
}