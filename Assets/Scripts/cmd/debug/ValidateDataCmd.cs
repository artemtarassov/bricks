using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ValidateDataCmd
{
    public void Run()
    {
        var allGroups = BalancingModel.Instance.GetAllGroups();
        foreach (var groupName in allGroups)
        {
            var groupData = BalancingModel.Instance.GetDataCopy(groupName);
            var elementDataList = groupData.cityElementDataList;

            var groupElement = CityModel.Instance.GetGroupByName(groupName);
            Assert.AreEqual(elementDataList.Count, groupElement.GetElements().Count, $"ValidateDataCmd: element count mismatch for group {groupName}. data container has {elementDataList.Count} elements, but city has {groupElement.GetElements().Count} elements.");

            foreach (var elementData in elementDataList)
            {
                var elementDataKey = elementData.dataKey;
                var brickDataAmount = elementData.brickDataList.Sum(b => b.max);
                var cityElement = groupElement.GetElements().ToList().Find(e => e.dataKey == elementDataKey);
                var brickGameObjects = cityElement.FindAllBricks().Count;
                Assert.AreEqual(brickDataAmount, brickGameObjects, $"ValidateDataCmd: brick count mismatch for element {elementDataKey} in group {groupName}. brickGameObjecs " + brickGameObjects + ", brickDataAmount " + brickDataAmount);
            }
        }
    }

}