using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ValidateDataCmd
{
    public void Run()
    {

        var list = BuildingNameUtil.GetAllBuildingNames(false);
        Debug.Log("ValidateDataCmd: validating data for buildings " + string.Join(", ", list));

        foreach (var buildingName in list)
        {
            var buildingData = BalancingModel.Instance.GetDataCopy(buildingName);
            Debug.Log($"ValidateDataCmd: validating building {buildingName}. contains {buildingData.cityElementDataList.Count} elements.");

            foreach (var elementData in buildingData.cityElementDataList)
            {
                Debug.Log($"ValidateDataCmd: validating element {elementData.dataKey} in building {buildingName}. contains {elementData.brickDataList.Count} brick data items");
                foreach (var brickData in elementData.brickDataList)
                {
                    Assert.IsTrue(brickData.max > 0, $"ValidateDataCmd: brick data max should be greater than 0 for building {buildingName}, element {elementData.dataKey}");
                }
                foreach (var columnData in elementData.columns)
                {
                    Assert.IsTrue(columnData.list.Count > 2, $"ValidateDataCmd: column data list should have more than 2 items for building {buildingName}, element {elementData.dataKey}, column {columnData.columnIndex}. contains " + columnData.list.Count + " items.");
                }

            }

            Assert.IsNotNull(buildingData, $"ValidateDataCmd: no data found for building {buildingName}");
            var elementDataList = buildingData.cityElementDataList;

            var building = CityModel.Instance.GetBuildingByName(buildingName);
            Assert.AreEqual(elementDataList.Count, building.GetElements().Count, $"ValidateDataCmd: element count mismatch for building {buildingName}. data container has {elementDataList.Count} elements, but city has {building.GetElements().Count} elements.");

            foreach (var elementData in elementDataList)
            {
                var elementDataKey = elementData.dataKey;
                var brickDataAmount = elementData.brickDataList.Sum(b => b.max);
                var cityElement = building.GetElements().ToList().Find(e => e.dataKey == elementDataKey);
                var brickGameObjects = cityElement.FindAllBricks().Count;
                Debug.Log($"ValidateDataCmd: validating element {elementDataKey} in building {buildingName}. brickGameObjecs " + brickGameObjects + ", brickDataAmount " + brickDataAmount);
                Assert.AreEqual(brickDataAmount, brickGameObjects, $"ValidateDataCmd: brick count mismatch for element {elementDataKey} in building {buildingName}. brickGameObjecs " + brickGameObjects + ", brickDataAmount " + brickDataAmount);
            }
        }
    }

}