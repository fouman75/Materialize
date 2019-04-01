using System.Collections;
using System.Collections.Generic;
using Tayx.Graphy;
using UnityEngine;

public class DeveloperOptions : MonoBehaviour
{
    public GraphyManager StatsGraph;
    private bool _enabled;

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