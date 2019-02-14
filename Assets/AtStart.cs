using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtStart : MonoBehaviour
{
    public int TargetFps = 30;
     
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFps;
    }
     
    void Update()
    {
        if(Application.targetFrameRate != TargetFps)
            Application.targetFrameRate = TargetFps;
    }
}
