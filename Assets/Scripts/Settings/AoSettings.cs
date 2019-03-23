#region

using System.ComponentModel;

#endregion

namespace Settings
{
    public class AoSettings : TexturePanelSettings
    {
        public float Blend;
        public float Depth;
        public float FinalBias;
        public float FinalContrast;

        [DefaultValue(5.0f)] public float Spread;


        public AoSettings()
        {
            Reset();
        }

        public sealed override void Reset()
        {
            Spread = 50.0f;
            Depth = 100.0f;
            FinalBias = 0.0f;
            FinalContrast = 1.0f;
            Blend = 1.0f;
        }
    }
}