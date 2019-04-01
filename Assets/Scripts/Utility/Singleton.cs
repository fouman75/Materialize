#region

#region

using UnityEngine;

#endregion

// ReSharper disable StaticMemberInGenericType

#endregion

/// <inheritdoc />
/// <summary>
///     Classe com apenas uma instancia
/// </summary>
/// <typeparam name="T">
///     O parametro deve ser a mesma classe que esta herdando de Singleton
/// </typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    private static readonly object Lock = new object();

    private static bool _applicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                 "' already destroyed on application quit." +
                                 " Won't create again - returning null.");
                return null;
            }

            lock (Lock)
            {
                if (_instance != null) return _instance;

                _instance = (T) FindObjectOfType(typeof(T));

                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    Debug.LogError("[Singleton] Something went really wrong " +
                                   " - there should never be more than 1 singleton!" +
                                   " Reopening the scene might fix it.");
                    return _instance;
                }

                if (_instance == null)
                {
                    var singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = "(singleton) " + typeof(T);

                    DontDestroyOnLoad(singleton);

                    Debug.Log("[Singleton] An instance of " + typeof(T) +
                              " is needed in the scene, so '" + singleton +
                              "' was created with DontDestroyOnLoad.");
                }
                else
                {
                    Debug.Log("[Singleton] Using instance already created: " +
                              _instance.gameObject.name);
                }

                return _instance;
            }
        }
    }

    /// <summary>
    ///     When Unity quits, it destroys objects in a random order.
    ///     In principle, a Singleton is only destroyed when application quits.
    ///     If any script calls Instance after it have been destroyed,
    ///     it will create a buggy ghost object that will stay on the Editor scene
    ///     even after stopping playing the Application. Really bad!
    ///     So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}