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
        GameObject activeSelectedObject = Selection.activeGameObject;
        bool canToggleGeneratedBricks = activeSelectedObject != null && activeSelectedObject.GetComponent<CityElementGroup>() != null;

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
                ApplyToSelection(validSelectedObjects, target => new FillWithBricks2().Run(target, settings));
            }

            if (GUILayout.Button("Preview Colored Bricks"))
            {
                OnPreviewColoredBricksClicked(validSelectedObjects);
            }

            if (GUILayout.Button("Clear Bricks"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.ClearBricks);
            }

            /*if (GUILayout.Button("Cleanup Touching Bricks"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.CleanupTouchingBricks);
            }

            if (GUILayout.Button("clean up non visible bricks via camera"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.CleanupNonVisibleBricksViaCamera);
            }

            if (GUILayout.Button("clean up non visible bricks via scene view"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.CleanupNonVisibleBricksViaSceneView);
            }

            if (GUILayout.Button("Clear Bricks"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.ClearBricks);
            }

            if (GUILayout.Button("Cleanup Selected Bricks Against Scene"))
            {
                ApplyToSelection(validSelectedObjects, FillWithCubesGenerator.CleanupTouchingBricksAgainstScene);
            }*/

        }

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!canToggleGeneratedBricks))
        {
            if (GUILayout.Button("Toggle Generated Bricks"))
            {
                FillWithCubesGenerator.ToggleGeneratedBricksActive(activeSelectedObject);
            }
        }
    }

    private static void OnPreviewColoredBricksClicked(List<GameObject> validSelectedObjects)
    {
        foreach (var obj in validSelectedObjects)
        {
            var cityElement = obj.GetComponent<CityElement>();
            var containerChild = cityElement.GetChildByName("__GeneratedBricks");
            containerChild.gameObject.SetActive(true);
            var container = new BrickLayersContainer(containerChild);
            var n = container.sortedBricks.FindAll(b => b.gameObject.activeSelf).Count;
            Debug.Log("Currently active bricks: " + n + " out of " + container.sortedBricks.Count);

            if (n == container.sortedBricks.Count)
            {
                for (var i = 0; i < container.sortedBricks.Count; i++)
                {
                    var brick = container.sortedBricks[i];
                    brick.gameObject.SetActive(false);
                }
                n = 0;
            }

            for (var i = n; i < n + 10 && i < container.sortedBricks.Count; i++)
            {
                var brick = container.sortedBricks[i];
                brick.gameObject.SetActive(true);
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
