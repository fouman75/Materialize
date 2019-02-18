using General;

namespace Settings
{
    public class ProgramSettings
    {
        public ProgramEnums.FileFormat FileFormat;
        public bool HDR;
        public bool NormalMapMaxStyle;
        public bool NormalMapMayaStyle;

        public bool PostProcessEnabled;
        public PropChannelMap PropBlue;
        public PropChannelMap PropGreen;

        public PropChannelMap PropRed;
    }
}