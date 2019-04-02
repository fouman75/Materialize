#region

using Materialize.General;

#endregion

namespace Materialize.Settings
{
    public class ProgramSettings
    {
        public ProgramEnums.FileFormat FileFormat;
        public int FrameRate;
        public ProgramEnums.GraphicsQuality GraphicsQuality;
        public bool HDR;
        public bool HideUiOnRotate;
        public bool NormalMapMaxStyle;
        public bool NormalMapMayaStyle;

        public bool PostProcessEnabled;
        public ProgramEnums.PropChannelMap PropBlue;
        public ProgramEnums.PropChannelMap PropGreen;
        public ProgramEnums.PropChannelMap PropRed;
    }
}