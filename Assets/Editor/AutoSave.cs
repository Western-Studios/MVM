using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoSave
{
    private const float IntervalMinutes = 5f;
    private static double nextSaveTime;

    static AutoSave()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + IntervalMinutes * 60f;
        EditorApplication.update += OnUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnUpdate()
    {
        if (EditorApplication.isPlaying) return;
        if (EditorApplication.timeSinceStartup < nextSaveTime) return;

        Save("auto");
        nextSaveTime = EditorApplication.timeSinceStartup + IntervalMinutes * 60f;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            Save("before play");
    }

    private static void Save(string reason)
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log($"[AutoSave] Saved ({reason}) at {System.DateTime.Now:HH:mm:ss}");
    }
}
