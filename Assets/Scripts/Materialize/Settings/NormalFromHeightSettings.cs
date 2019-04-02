namespace Materialize.Settings
{
    public class NormalFromHeightSettings : TexturePanelSettings
    {
        public float AngularIntensity;
        public float Angularity;

        public float Blur0Contrast;
        public float Blur0Weight;
        public float Blur1Weight;
        public float Blur2Weight;
        public float Blur3Weight;
        public float Blur4Weight;
        public float Blur5Weight;
        public float Blur6Weight;

        public float FinalContrast;
        public float LightRotation;
        public float ShapeBias;
        public float ShapeRecognition;
        public int SlopeBlur;
        public bool UseDiffuse;

        public NormalFromHeightSettings()
        {
            UseDiffuse = true;
            Reset();
        }

        public sealed override void Reset()
        {
            Blur0Weight = 0.3f;
            Blur1Weight = 0.35f;
            Blur2Weight = 0.5f;
            Blur3Weight = 0.8f;
            Blur4Weight = 1.0f;
            Blur5Weight = 0.95f;
            Blur6Weight = 0.8f;

            Blur0Contrast = 20.0f;
            FinalContrast = 5.0f;

            Angularity = 0.1f;
            AngularIntensity = 0.5f;

            ShapeRecognition = 0.1f;

            LightRotation = 0.0f;

            SlopeBlur = 50;

            ShapeBias = 0.5f;
        }
    }
}