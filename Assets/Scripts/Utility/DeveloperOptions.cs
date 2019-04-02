#region

using Tayx.Graphy;
using UnityEngine;

#endregion

namespace Utility
{
    public class DeveloperOptions : MonoBehaviour
    {
        private bool _enabled;
        public GraphyManager StatsGraph;

        private void Start()
        {
            if (!Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
            }

            StatsGraph.gameObject.SetActive(true);
        }

        public void EnableDeveloperOptions()
        {
            _enabled = !_enabled;
            if (_enabled) StatsGraph.Enable();
            else StatsGraph.Disable();
        }
    }
}