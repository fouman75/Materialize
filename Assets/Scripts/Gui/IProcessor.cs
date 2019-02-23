using System.Collections;

namespace Gui
{
    public interface IProcessor
    {
        bool Active { set; get; }
        void NewTexture();
        void DoStuff();
        IEnumerator Process();
        void Close();
    }
}