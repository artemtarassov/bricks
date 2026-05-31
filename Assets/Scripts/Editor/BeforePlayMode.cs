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

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        Debug.Log("Play was pressed. Running editor setup before Play Mode begins.");

        // Your preparation code here.

        var group = GameObject.FindAnyObjectByType<CityElementGroup>();
        var elements = group.GetElements();
        foreach (var element in elements)
        {
            if (element.__EnclosingGameObject == null)
                element.__EnclosingGameObject = element.GetChildByName("__EnclosingGameObject");

            if (element.__GeneratedBricks == null)
                element.__GeneratedBricks = element.GetChildByName("__GeneratedBricks");

            Assert.IsNotNull(element.__GeneratedBricks, "CityElement " + element.name + " is missing __GeneratedBricks child");
            element.gameObject.SetActive(false);

            if (element.__EnclosingGameObject != null)
                element.__EnclosingGameObject.gameObject.SetActive(false);

            element.__GeneratedBricks.gameObject.SetActive(false);

            for (var i = 0; i < element.transform.childCount; i++)
            {
                var child = element.transform.GetChild(i);
                if (!child.name.Contains("__"))
                {
                    child.gameObject.SetActive(true);
                    ActivateChildren(child, true);
                }
            }
            ActivateChildren(element.__GeneratedBricks, true);
        }

        //save current scene if dirty
        if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty)
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }

    }

    private static void ActivateChildren(Transform parent, bool a)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            child.gameObject.SetActive(a);
            ActivateChildren(child, a);
        }
    }
}