#region

using System.Collections;
using UnityEngine;

#endregion

namespace Gui
{
    public class WelcomeScreen : MonoBehaviour
    {
        private static readonly int GlobalCubemap = Shader.PropertyToID("_GlobalCubemap");
        private float _backgroundFade = 1.0f;
        private float _logoFade = 1.0f;
        public Texture2D Background;
        public Texture2D Logo;

        public bool SkipWelcomeScreen;

        private void Start()
        {
            if (!SkipWelcomeScreen && !Application.isEditor) StartCoroutine(Intro());

            gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            GUI.color = new Color(1, 1, 1, _backgroundFade);

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Background);

            var logoWidth = Mathf.FloorToInt(Screen.width * 0.75f);
            var logoHeight = Mathf.FloorToInt(logoWidth * 0.5f);
            var logoPosX = Mathf.FloorToInt(Screen.width * 0.5f - logoWidth * 0.5f);
            var logoPosY = Mathf.FloorToInt(Screen.height * 0.5f - logoHeight * 0.5f);

            GUI.color = new Color(1, 1, 1, _logoFade);

            GUI.DrawTexture(new Rect(logoPosX, logoPosY, logoWidth, logoHeight), Logo);
        }

        private IEnumerator FadeLogo(float target, float overTime)
        {
            var timer = overTime;
            var original = _logoFade;

            while (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                _logoFade = Mathf.Lerp(target, original, timer / overTime);
                yield return new WaitForEndOfFrame();
            }

            _logoFade = target;

            //yield return new WaitForEndOfFrame();
        }

        private IEnumerator FadeBackground(float target, float overTime)
        {
            var timer = overTime;
            var original = _backgroundFade;

            while (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                _backgroundFade = Mathf.Lerp(target, original, timer / overTime);
                yield return new WaitForEndOfFrame();
            }

            _backgroundFade = target;

            gameObject.SetActive(false);
        }

        private IEnumerator Intro()
        {
            StartCoroutine(FadeLogo(1.0f, 0.5f));

            yield return new WaitForSeconds(3.0f);

            StartCoroutine(FadeLogo(0.0f, 1.0f));

            yield return new WaitForSeconds(1.0f);

            StartCoroutine(FadeBackground(0.0f, 1.0f));
        }
    }
}