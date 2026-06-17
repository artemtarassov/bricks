using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BeforePlayMode
{
    static BeforePlayMode()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void EnableEditorLights(bool enable)
    {
        var lights = GameObject.FindObjectsByType<Light>().ToList();
        var editorLights = lights.Where(l => l.gameObject.name.Contains("EditorLight")).ToList();
        foreach (var light in editorLights)
        {
            light.gameObject.SetActive(enable);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
  
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            new PrepareScene().Run();
            EnableEditorLights(true);
            return;
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EnableEditorLights(false);
            return;
        }

    }

}