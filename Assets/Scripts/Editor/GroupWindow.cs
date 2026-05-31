using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GroupWindow : EditorWindow
{
    [MenuItem("Tools/Group Window")]
    public static void ShowWindow()
    {
        GetWindow<GroupWindow>("Group");
    }

    private void OnGUI()
    {

        if (GUILayout.Button("group bricks into layer"))
        {
            OnGroupIntoLayersClicked();
        }
        if (GUILayout.Button("group"))
        {
            OnGroupClicked();
        }

        if (GUILayout.Button("Add to last group"))
        {
            OnAddToLastGroup();
        }

        if (GUILayout.Button("Merge non-group objects"))
        {
            OnMergeNonGroupObjects();
        }

        if (GUILayout.Button("Enable Next Ground"))
        {
            OnEnableNextGroup();
        }

        if (GUILayout.Button("Delete selected bricks"))
        {
            //delete ony selected bricks.
            var selection = Selection.gameObjects;
            foreach (var obj in selection)
            {
                var tag = obj.tag;
                if (tag == "Brick")
                {
                    GameObject.DestroyImmediate(obj);
                }
            }
        }
    }


    private void OnGroupIntoLayersClicked()
    {
        var selectedBricks = Selection.gameObjects;
        var bricks = new List<GameObject>();
        foreach (var obj in selectedBricks)
        {
            if (obj.tag == "Brick")
            {
                bricks.Add(obj);
            }
        }
        if (bricks.Count == 0)
        {
            throw new System.Exception("No bricks found in selection");
        }
        //ensure all bricks have the same parent
        var firstParent = bricks[0].transform.parent;
        foreach (var brick in bricks)
        {
            if (brick.transform.parent != firstParent)
            {
                throw new System.Exception("All bricks should have the same parent");
            }
        }

        var layer = new GameObject();
        layer.name = "Layer_" + firstParent.childCount;
        layer.transform.SetParent(firstParent);
        layer.transform.localPosition = Vector3.zero;
        foreach (var brick in bricks)
        {
            brick.transform.SetParent(layer.transform);
        }
        layer.gameObject.SetActive(false);
        layer.gameObject.AddComponent<BricksLayer>();
    }

    private List<GameObject> GetGroups(GameObject house)
    {
        if (house.name.Contains("House") == false)
        {
            throw new System.Exception("Selected object is not a house");
        }
        var result = new List<GameObject>();
        for (var i = 0; i < house.transform.childCount; i++)
        {
            var child = house.transform.GetChild(i);
            if (child.tag == "Group")
            {
                result.Add(child.gameObject);
            }
        }
        return result;
    }

    private GameObject GetLastGroup(GameObject house)
    {
        var groups = GetGroups(house);
        if (groups.Count == 0)
        {
            return null;
        }
        return groups[groups.Count - 1];
    }

    private void OnAddToLastGroup()
    {
        var selectedObjects = Selection.gameObjects;
        var lastGroup = GetLastGroup(selectedObjects[0].transform.parent.gameObject);
        foreach (var obj in selectedObjects)
        {
            obj.transform.SetParent(lastGroup.transform);
        }
    }

    private void OnGroupClicked()
    {
        var selectedObjects = Selection.gameObjects;
        var house = selectedObjects[0].transform.parent.gameObject;
        if (house.name.Contains("House") == false)
        {
            throw new System.Exception("Selected object is not child of house");
        }
        var lastGroup = GetLastGroup(house);
        var group = new GameObject("Group");
        group.tag = "Group";
        group.transform.SetParent(house.transform);
        foreach (var obj in selectedObjects)
        {
            obj.transform.SetParent(group.transform);
        }

        if (lastGroup == null)
        {
            group.transform.SetAsFirstSibling();
            return;
        }
        group.transform.SetSiblingIndex(lastGroup.transform.GetSiblingIndex() + 1);
        group.gameObject.SetActive(false);
    }

    private GameObject GetGroupFromSelectedObjects()
    {
        var selectedObjects = Selection.gameObjects;
        foreach (var obj in selectedObjects)
        {
            if (obj.transform.parent.tag == "Group")
            {
                return obj.transform.parent.gameObject;
            }
        }
        return null;
    }

    private void OnMergeNonGroupObjects()
    {
        var selectedObjects = Selection.gameObjects;
        var group = GetGroupFromSelectedObjects();
        if (group == null)
        {
            throw new System.Exception("No group found in selected objects");
        }
        foreach (var obj in selectedObjects)
        {
            obj.transform.SetParent(group.transform);
        }
    }

    private void OnEnableNextGroup()
    {
        var house = Selection.gameObjects[0].transform;
        for (var i = 0; i < house.childCount; i++)
        {
            var child = house.GetChild(i);
            if (child.tag == "Group" && child.gameObject.activeSelf == false)
            {
                child.gameObject.SetActive(true);
                return;
            }
        }
    }
}
