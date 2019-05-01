using System.Collections;
using JetBrains.Annotations;
using Materialize.Gui;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Materialize.General
{
    public class EntryRunner : MonoBehaviour
    {
        public string MainSceneName;

        private void Start()
        {
            if (!Application.isEditor)
            {
                GraphicsSettings.renderPipelineAsset = null;
            }

            Application.targetFrameRate = 30;

            StartCoroutine(PersistentSettings.Instance.SetScreen(ProgramEnums.ScreenMode.Windowed));
        }

        [UsedImplicitly]
        public void OnQualityChange()
        {
            StartCoroutine(LoadSceneAsync());
        }

        private IEnumerator LoadSceneAsync()
        {
            yield return null; // One Frame skip just in case

            var sceneLoad = SceneManager.LoadSceneAsync(MainSceneName);

            while (!sceneLoad.isDone)
            {
                MessagePanel.ShowMessage($"Now Loading   {(sceneLoad.progress * 100):0}%");

                if (sceneLoad.progress >= 0.9f)
                {
                    sceneLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            sceneLoad.allowSceneActivation = true;
        }
    }
}