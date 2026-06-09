using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FindUnnamedLayerGameObjects
{
    [MenuItem("Tools/Find GameObjects With Unnamed Layer")]
    private static void FindInActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            EditorUtility.DisplayDialog(
                "Scene Not Loaded",
                "Open a scene before running the layer check.",
                "OK");
            return;
        }

        List<GameObject> objectsWithUnnamedLayers = new List<GameObject>();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            CollectObjectsWithUnnamedLayers(rootObjects[i].transform, objectsWithUnnamedLayers);
        }

        if (objectsWithUnnamedLayers.Count == 0)
        {
            Debug.Log($"No GameObjects with unnamed layers were found in scene '{activeScene.name}'.");
            EditorUtility.DisplayDialog(
                "Layer Check Complete",
                $"No GameObjects with unnamed layers were found in scene '{activeScene.name}'.",
                "OK");
            return;
        }

        Selection.objects = objectsWithUnnamedLayers.ToArray();

        StringBuilder report = new StringBuilder();
        report.AppendLine($"Found {objectsWithUnnamedLayers.Count} GameObject(s) with unnamed layers in scene '{activeScene.name}':");

        for (int i = 0; i < objectsWithUnnamedLayers.Count; i++)
        {
            GameObject gameObject = objectsWithUnnamedLayers[i];
            report.AppendLine($"- {GetHierarchyPath(gameObject.transform)} (layer index: {gameObject.layer})");
        }

        Debug.LogWarning(report.ToString());
        EditorUtility.DisplayDialog(
            "Unnamed Layers Found",
            $"Found {objectsWithUnnamedLayers.Count} GameObject(s) with unnamed layers. They have been selected in the Hierarchy.",
            "OK");
    }

    private static void CollectObjectsWithUnnamedLayers(Transform current, List<GameObject> results)
    {
        if (string.IsNullOrEmpty(LayerMask.LayerToName(current.gameObject.layer)))
        {
            results.Add(current.gameObject);
        }

        for (int i = 0; i < current.childCount; i++)
        {
            CollectObjectsWithUnnamedLayers(current.GetChild(i), results);
        }
    }

    private static string GetHierarchyPath(Transform current)
    {
        List<string> parts = new List<string>();

        while (current != null)
        {
            parts.Add(current.name);
            current = current.parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }
}
