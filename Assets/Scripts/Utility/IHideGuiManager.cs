namespace Utility
{
    public interface IHideGuiManager
    {
        CountLocker HideGuiLocker { get; set; }
        void SaveHideStateAndHideAndLock(object sender);
    }
}