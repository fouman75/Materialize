#define ENABLE_LOGGER

using UnityEngine;


namespace General
{
    public static class Logger
    {
        public static void Log(string message)
        {
#if ENABLE_LOGGER
            Debug.Log(message);
#endif
        }

        public static void LogError(string message)
        {
#if ENABLE_LOGGER
            Debug.LogError(message);
#endif
        }
    }
}