namespace Materialize.Settings
{
    public class EditDiffuseSettings : TexturePanelSettings
    {
        public int AvgColorBlurSize;
        public float BlurContrast;
        public int BlurSize;
        public float ColorLerp;
        public float DarkMaskPow;
        public float DarkPow;
        public float DarkSpot;
        public float FinalBias;
        public float FinalContrast;
        public float HotSpot;
        public float LightMaskPow;
        public float LightPow;
        public float Saturation;

        public EditDiffuseSettings()
        {
            Reset();
        }


        public sealed override void Reset()
        {
            AvgColorBlurSize = 50;
            BlurSize = 20;
            BlurContrast = 0.0f;
            LightMaskPow = 0.5f;
            LightPow = 0.0f;
            DarkMaskPow = 0.5f;
            DarkPow = 0.0f;
            HotSpot = 0.0f;
            DarkSpot = 0.0f;
            FinalContrast = 1.0f;
            FinalBias = 0.0f;
            ColorLerp = 0.5f;
            Saturation = 1.0f;
        }
    }
}