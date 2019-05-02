using System;
using System.Collections;
using JetBrains.Annotations;
using Materialize.Gui;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;
using Logger = Utility.Logger;

namespace Materialize.General
{
    public class PersistentSettings : Utility.Singleton<PersistentSettings>
    {
        public bool ByPassUnsafe;
        public ProgramAssets ProgramAssets;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        private IEnumerator UpdateQuality(bool isUpdate)
        {
            if (Application.isEditor || ByPassUnsafe) yield break;

            var hdRenderPipelineAsset = GetRpQualityAsset();

            if (isUpdate && GraphicsSettings.renderPipelineAsset == hdRenderPipelineAsset || !hdRenderPipelineAsset)
                yield break;

            var quality = PrefsManager.GraphicsQuality;
            Logger.Log("Changing quality to " + quality);

            if (isUpdate)
            {
                GraphicsSettings.renderPipelineAsset = null;
                yield return null;
            }

            switch (quality)
            {
                case ProgramEnums.GraphicsQuality.High:
                    QualitySettings.SetQualityLevel(2);
                    break;
                case ProgramEnums.GraphicsQuality.Medium:
                    QualitySettings.SetQualityLevel(1);
                    break;
                case ProgramEnums.GraphicsQuality.Low:
                case ProgramEnums.GraphicsQuality.Minimal:
                    QualitySettings.SetQualityLevel(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isUpdate) yield return null;

            GraphicsSettings.renderPipelineAsset = hdRenderPipelineAsset;

            if (isUpdate) yield return new WaitForSeconds(0.5f);
        }

        private HDRenderPipelineAsset GetRpQualityAsset()
        {
            if (!ProgramAssets) return null;

            HDRenderPipelineAsset hdRenderPipelineAsset;
            var quality = PrefsManager.GraphicsQuality;
            switch (quality)
            {
                case ProgramEnums.GraphicsQuality.High:
                    hdRenderPipelineAsset = ProgramAssets.HighQualityAsset;
                    break;
                case ProgramEnums.GraphicsQuality.Medium:
                    hdRenderPipelineAsset = ProgramAssets.MediumQualityAsset;
                    break;
                case ProgramEnums.GraphicsQuality.Low:
                    hdRenderPipelineAsset = ProgramAssets.LowQualityAsset;
                    break;
                case ProgramEnums.GraphicsQuality.Minimal:
                    hdRenderPipelineAsset = ProgramAssets.MinimalQualityAsset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return hdRenderPipelineAsset;
        }

        public IEnumerator SetScreen(ProgramEnums.ScreenMode screenMode)
        {
            if (Application.isEditor || ByPassUnsafe) yield break;

            if (MainGui.Instance)
            {
                if (MainGui.Instance) MainGui.Instance.CloseWindows();
            }

            if (ProgramManager.Instance)
            {
                ProgramManager.Instance.PostProcessingVolume.enabled = false;
                ProgramManager.Instance.SceneVolume.enabled = false;
            }

            switch (screenMode)
            {
                case ProgramEnums.ScreenMode.FullScreen:
                    var fsRes = Screen.resolutions;
                    var highRes = fsRes[fsRes.Length - 1];
                    Screen.SetResolution(highRes.width, highRes.height, FullScreenMode.ExclusiveFullScreen);
                    break;
                case ProgramEnums.ScreenMode.Windowed:
                    var res = GetHighestResolution(2);
                    if (res == null) break;
                    Screen.SetResolution(res.Value.width, res.Value.height, FullScreenMode.Windowed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenMode), screenMode, null);
            }

            if (!ProgramManager.Instance) yield break;

            var hdRenderPipelineAsset = GetRpQualityAsset();
            if (!hdRenderPipelineAsset) yield break;

            GraphicsSettings.renderPipelineAsset = null;
            yield return null;
            GraphicsSettings.renderPipelineAsset = hdRenderPipelineAsset;

            yield return null;
            ProgramManager.Instance.PostProcessingVolume.enabled = true;
            ProgramManager.Instance.SceneVolume.enabled = true;
        }

        /// <summary>
        ///     Get the Highest resolution skipping n matches
        /// </summary>
        /// <param name="skip"> number of matches to skip</param>
        /// <returns> resolution after n skips in the same aspect ratio than native </returns>
        private static Resolution? GetHighestResolution(int skip)
        {
            var winRes = Screen.resolutions;
            if (winRes.Length <= 0) return null;
            var nativeRes = winRes[winRes.Length - 1];
            var res = nativeRes;
            var currentResolution = Screen.currentResolution;
            var nativeAspect = 1.0f * nativeRes.width / nativeRes.height;
            var count = skip;
            for (var i = winRes.Length - 1; i >= 0; i--)
            {
                var aspectRatio = 1.0f * winRes[i].width / winRes[i].height;
                if (!(Mathf.Abs(aspectRatio - nativeAspect) < 0.00001f)) continue;
                if (winRes[i].width > currentResolution.width) continue;
                if (count-- > 0) continue;

                res = winRes[i];
                break;
            }

            return res;
        }

        public void SetGraphicsQuality(ProgramEnums.GraphicsQuality quality)
        {
            SetGraphicsQuality(quality.ToString());
        }

        [UsedImplicitly]
        public void SetGraphicsQuality(string quality)
        {
            ChangeGraphicsQuality(quality, false);
        }

        public void ChangeGraphicsQuality(string quality, bool isUpdate = true)
        {
            if (PrefsManager.GraphicsQuality.ToString() == quality && isUpdate) return;

            var graphicsQuality = ProgramEnums.GraphicsQuality.Minimal;
            var isValid = false;
            foreach (var possible in Enum.GetNames(typeof(ProgramEnums.GraphicsQuality)))
            {
                if (!possible.Equals(quality)) continue;

                isValid = Enum.TryParse(possible, out graphicsQuality);

                if (isValid) break;
            }

            if (!isValid) return;


            Logger.Log("Quality " + graphicsQuality);
            PrefsManager.GraphicsQuality = graphicsQuality;
            StartCoroutine(UpdateQuality(isUpdate));
        }
    }
}