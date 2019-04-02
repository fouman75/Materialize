#region

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#endregion

namespace Utility
{
    public class GuiMaker : Singleton<GuiMaker>
    {
        private static readonly List<GuiPanel> PanelsList = new List<GuiPanel>();
        private Canvas _canvas;
        public GameObject CanvasPrefab;
        public GuiPrefabPack GuiPrefabs;


        private void Awake()
        {
            _canvas = Instantiate(CanvasPrefab).GetComponent<Canvas>();
        }

        private void Start()
        {
            Test();
        }

        private void Test()
        {
            var pixelRect = _canvas.pixelRect;
            var position = new Vector2Int((int) pixelRect.width / 2, (int) pixelRect.height / 2);
            position.x -= 250;
            var canvasNumber = GetNewPanel(position, "Big Test Panel");
            AddSlider(canvasNumber);
            AddSlider(canvasNumber);
            AddSlider(canvasNumber);
            AddSlider(canvasNumber);

            var position2 = new Vector2Int((int) pixelRect.width / 2, (int) pixelRect.height / 2);
            position2.x += 50;
            var canvasNumber2 = GetNewPanel(position2, "Big Test Panel");
            AddSlider(canvasNumber2);
            AddSlider(canvasNumber2);
            AddSlider(canvasNumber2);
            AddSlider(canvasNumber2);
        }

        public int GetNewPanel(Vector2Int position, string title)
        {
            var guiPanel = new GuiPanel(GuiPrefabs, position);
            guiPanel.AddTitle(title);
            guiPanel.Transform.parent = _canvas.transform;
            PanelsList.Add(guiPanel);

            return PanelsList.Count - 1;
        }

        public static void AddSlider(int canvasNumber)
        {
            if (PanelsList.Count <= canvasNumber)
            {
                Logger.LogError($"Canvas {canvasNumber} does not exist");
                return;
            }

            var guiCanvas = PanelsList[canvasNumber];
            guiCanvas.AddSlider();
        }


        public struct GuiPanel
        {
            private const int TopOffset = 10;
            private const int BottonOffset = 20;
            private const int PanelWidth = 300;
            private readonly GuiPrefabPack _prefabPack;
            private readonly RectTransform _panel;
            public readonly Transform Transform;
            private readonly List<GameObject> _sliders;
            private TextMeshProUGUI _title;

            public GuiPanel(GuiPrefabPack guiPrefabPack, Vector2Int position)
            {
                _prefabPack = guiPrefabPack;
                _panel = Instantiate(_prefabPack.PanelPrefab).GetComponent<RectTransform>();
                var rect = _panel.rect;
                rect.width = PanelWidth;


                rect.height = TopOffset + BottonOffset;
                _panel.sizeDelta = rect.size;
                _panel.anchoredPosition = position;

                _sliders = new List<GameObject>();

                Transform = _panel.transform;
                _title = null;
            }

            public void AddTitle(string title)
            {
                _title = AddComponent(_prefabPack.TitlePrefab).GetComponent<TextMeshProUGUI>();
                _title.text = title;
            }

            public GameObject AddComponent(GameObject prefab)
            {
                var rect = _panel.rect;
                //First remove bottom margin
                rect.height -= BottonOffset;
                var component = Instantiate(prefab, _panel.transform, true);
                var position = Vector2.zero;

                var height = (int) prefab.GetComponent<RectTransform>().rect.height;
                position.y = -rect.height;
                component.GetComponent<RectTransform>().anchoredPosition = position;

                //Then add it back
                rect.height += height + BottonOffset;
                _panel.sizeDelta = rect.size;

                return component;
            }

            public int AddSlider()
            {
                var slider = AddComponent(_prefabPack.SliderPrefab);
                _sliders.Add(slider);
                return _sliders.Count - 1;
            }
        }

        [Serializable]
        public struct GuiPrefabPack
        {
            public GameObject SliderPrefab;
            public GameObject PanelPrefab;
            public GameObject TitlePrefab;
        }
    }
}