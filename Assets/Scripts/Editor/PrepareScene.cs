using UnityEngine;
using UnityEngine.Assertions;

public class PrepareScene
{

    public void Run()
    {
        Debug.Log("PrepareScene Run called");
        var groups = GameObject.FindObjectsOfType<BuildingElement>(true);
        foreach (var group in groups)
        {
            Run(group);
        }

        var sc = GameObject.FindObjectOfType<SlotTextureCameraController>(true);
        sc.gameObject.SetActive(false);
    }

    public void Run(BuildingElement group)
    {
        Debug.Log("Preparing group " + group.name);
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