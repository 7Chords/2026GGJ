using UnityEngine;

public static class SCDebugHelper
{
    public static void Log(string _str)
    {
#if UNITY_EDITOR
        Debug.Log(_str);
#endif
    }

    public static void Log(int _intVal)
    {
#if UNITY_EDITOR
        Debug.Log(_intVal);
#endif
    }

    public static void LogWarning(string _str)
    {
#if UNITY_EDITOR
        Debug.LogWarning(_str);
#endif
    }

    public static void LogError(string _str)
    {
#if UNITY_EDITOR
        Debug.LogError(_str);
#endif
    }
}
