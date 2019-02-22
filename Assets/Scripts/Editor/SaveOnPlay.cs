#region

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#endregion

namespace MkGames
{
    [InitializeOnLoad]
    public class SaveOnPlay
    {
        static SaveOnPlay()
        {
            EditorApplication.playModeStateChanged += SaveCurrentScene;
        }

        private static void SaveCurrentScene(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("Saving open scenes on play...");
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }
}