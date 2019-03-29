using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperOptions : MonoBehaviour
{
    public GameObject StatsGraph;
    private bool _enabled;

    private void Start()
    {
        if (!Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
        }
    }

    public void EnableDeveloperOptions()
    {
        _enabled = !_enabled;
        StatsGraph.SetActive(_enabled);
    }
}