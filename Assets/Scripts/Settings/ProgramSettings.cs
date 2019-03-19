#region

using General;

#endregion

namespace Settings
{
    public class ProgramSettings
    {
        public ProgramEnums.FileFormat FileFormat;
        public ProgramEnums.GraphicsQuality GraphicsQuality;
        public int FrameRate;
        public bool HDR;
        public bool HideUiOnRotate;
        public bool NormalMapMaxStyle;
        public bool NormalMapMayaStyle;

        public bool PostProcessEnabled;
        public PropChannelMap PropBlue;
        public PropChannelMap PropGreen;

        public PropChannelMap PropRed;
    }
}