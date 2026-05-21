using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class FillWithCubesWindow : EditorWindow
{
    private readonly FillWithCubesSettings settings = new FillWithCubesSettings();

    [MenuItem("Tools/FillWithCubes")]
    public static void ShowWindow()
    {
        GetWindow<FillWithCubesWindow>("FillWithCubes");
    }

    private void OnGUI()
    {
        List<GameObject> validSelectedObjects = GetValidSelectedObjects();
        int ignoredSelectionCount = Selection.gameObjects.Length - validSelectedObjects.Count;

        EditorGUILayout.LabelField("Brick Settings", EditorStyles.boldLabel);
        settings.BrickSize = EditorGUILayout.FloatField("Brick Size", settings.BrickSize);
        settings.BrickGap = EditorGUILayout.FloatField("Brick Gap", settings.BrickGap);
        settings.IncludeInactiveObjects = EditorGUILayout.Toggle("Include Inactive", settings.IncludeInactiveObjects);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Brick Setup", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Brick Prefab", FillWithCubesSettings.BrickPrefabPath);
        EditorGUILayout.LabelField("Brick Material", FillWithCubesSettings.BrickMaterialPath);
        settings.AddBrickColliders = EditorGUILayout.Toggle("Add Brick Colliders", settings.AddBrickColliders);

        EditorGUILayout.Space();

        if (validSelectedObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("Select one or more GameObjects with a CityElement component to generate bricks for them.", MessageType.Info);
        }
        else
        {
            string selectionMessage = validSelectedObjects.Count == 1
                ? "Selected GameObject: " + validSelectedObjects[0].name
                : "Selected GameObjects: " + validSelectedObjects.Count;

            if (ignoredSelectionCount > 0)
            {
                selectionMessage += " (" + ignoredSelectionCount + " ignored without CityElement)";
            }

            EditorGUILayout.HelpBox(selectionMessage, MessageType.None);
        }

        using (new EditorGUI.DisabledScope(validSelectedObjects.Count == 0))
        {
            if (GUILayout.Button("Add Bricks"))
            {
                Debug.Log("Generating bricks for " + validSelectedObjects.Count + " selected GameObjects...");
                ApplyToSelection(validSelectedObjects, target => FillWithCubesGenerator.AddBricks(target, settings));
            }

            if (GUILayout.Button("Cleanup Touching Bricks"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.CleanupTouchingBricks);
            }

            if (GUILayout.Button("clean up non visible bricks"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.CleanupNonVisibleBricks);
            }

            if (GUILayout.Button("Clear Bricks"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.ClearBricks);
            }
        }
    }

    private static List<GameObject> GetValidSelectedObjects()
    {
        var validSelectedObjects = new List<GameObject>();
        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            if (selectedObject != null && selectedObject.GetComponent<CityElement>() != null)
            {
                validSelectedObjects.Add(selectedObject);
            }
        }

        return validSelectedObjects;
    }

    private static void ApplyToSelection(List<GameObject> selectedObjects, System.Action<GameObject> action)
    {
        foreach (GameObject selectedObject in selectedObjects)
        {
            action(selectedObject);
        }
    }
}
