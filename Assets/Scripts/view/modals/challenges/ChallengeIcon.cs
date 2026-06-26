using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ChallengeIcon : MonoBehaviour
{
    public void Setup(BuildingName buildingName)
    {
        this.Setup(buildingName, Color.white);

    }

    public void Setup(BuildingName buildingName, Color color)
    {
        Assert.IsTrue(BuildingNameUtil.IsChallengeBuilding(buildingName), "invalid building name passed" + buildingName);
        for (var i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i);
            child.gameObject.SetActive(child.gameObject.name == buildingName.ToString());
            if (child.gameObject.activeSelf)
            {
                var img = child.GetComponent<Image>();
                img.color = color;
            }
        }

    }

}