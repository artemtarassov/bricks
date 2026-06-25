using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SetupCityCmd
{
    private CityModel cityModel => CityModel.Instance;
    private PlayerModel playerModel => PlayerModel.Instance;

    public void Run(List<BuildingElement> buildings)
    {
        Assert.IsTrue(buildings.Count > 0, "SetupCityCmd: buildings list should not be empty");
        var pd = PlayerModel.Instance.playerData;

        for (var i = 0; i < buildings.Count; i++)
        {
            var building = buildings[i];
            var progress = pd.GetBuildingProgressByName(building.BuildingName);
            if (progress == null)
            {
                progress = new BuildingProgressData(building.BuildingName, BuildingState.Unlocked);
                if (progress.BuildingName == BuildingName.Preset_Bath_House_01)
                {
                    progress.SetState(BuildingState.Premium);
                }
                progress.attempts = 5;
                pd.Progress.Add(progress);
            }
            if (progress.attempts == -1)
            {
                progress.attempts = 5;
            }
        }

#if UNITY_EDITOR
        foreach (var progress in pd.Progress.ToList())
        {
            if (progress.State == BuildingState.Premium)
            {
                progress.SetState(BuildingState.Unlocked);
            }
        }
#endif

        if (pd.CurrentBuildingName == BuildingName.Undefined)
        {
            pd.SetCurrentBuilding(BuildingName.Preset_House_05, BuildingState.Unlocked);
            cityModel.SetBuildings(buildings, BuildingName.Preset_House_05);
        }
        else
        {
            cityModel.SetBuildings(buildings, pd.CurrentBuildingName);
        }

        if (pd.appVersion != Application.version)
        {
            pd.appVersion = Application.version;
            ResetCurrentElement();
        }

        new SwitchBuildingCmd().Run(0);

#if UNITY_EDITOR    
        new ValidateDataCmd().Run();
        new TestExtrasCmd().Run();
#endif
    }


    private void ResetCurrentElement()
    {
        var pd = PlayerModel.Instance.playerData;
        foreach (var progress in pd.Progress.ToList())
        {
            ResetCurrentElement(progress);
        }
    }
    private void ResetCurrentElement(BuildingProgressData progress)
    {
        var building = cityModel.GetBuildingByName(progress.BuildingName);
        if (building == null)
        {
            //whole building was removed.
            playerModel.playerData.Progress.Remove(progress);
            return;
        }
        var element = progress.GetCurrentElement();
        if (element == null || element.ElementCompleted())
        {
            //element was not set or was completed.
            return;
        }
        var balancingBuildingData = BalancingModel.Instance.GetDataCopy(building.BuildingName);
        var prevElementIndex = progress.currentElementIndex;
        if (prevElementIndex > 0)
        {
            var freshData = balancingBuildingData.GetElementDataContainerByIndex(prevElementIndex);
            if (freshData != null)
                progress.SetCurrentElement(freshData);
            return;
        }
        if (progress.CompletedElementsCounter > 0)
        {
            var index = progress.CompletedElementsCounter - 1;
            var freshData = balancingBuildingData.GetElementDataContainerByIndex(index);
            if (freshData != null)
                progress.SetCurrentElement(freshData);
            return;
        }

    }

    private void ValidateProgress(BuildingProgressData progress)
    {
        var building = cityModel.GetBuildingByName(progress.BuildingName);
        if (building == null)
        {
            //whole building was removed.
            playerModel.playerData.Progress.Remove(progress);
            Debug.Log($"SetupCityCmd: ValidateProgress: building {progress.BuildingName} was removed since player progress was made. removing building progress.");
            return;
        }
        var element = progress.GetCurrentElement();
        if (element == null || element.ElementCompleted())
        {
            //element was not set or was completed.
            Debug.Log($"SetupCityCmd: ValidateProgress: no current element progress found for building {building.BuildingName} or element was completed. setting element progress to first element.");
            return;
        }
        var cityElement = building.GetElements().First(e => e.dataKey == element.dataKey);
        if (cityElement == null)
        {
            //whole element was removed.
            var index = progress.currentElementIndex;
            var balancingBuildingData = BalancingModel.Instance.GetDataCopy(building.BuildingName);
            if (index < 0)
            {
                index = 0;
            }
            else if (index >= balancingBuildingData.cityElementDataList.Count)
            {
                index = balancingBuildingData.cityElementDataList.Count - 1;
            }
            var newElementData = balancingBuildingData.cityElementDataList[index];
            progress.SetCurrentElement(newElementData);
            progress.currentElementIndex = index;
            Debug.Log($"SetupCityCmd: ValidateProgress: element {element.dataKey} in building {building.BuildingName} was removed since player progress was made. setting element progress to {newElementData.dataKey}.");
            return;
        }
        var bricksInCityElement = cityElement.GetBrickLayersContainer().sortedBricks;
        var bricksInProgress = element.brickDataList.Sum(b => b.max);
        if (bricksInCityElement.Count != bricksInProgress)
        {
            //element was changed since progress was made, removing element progress to avoid issues.
            progress.SetCurrentElement(BalancingModel.Instance.GetDataCopy(building.BuildingName, element.dataKey));
            Debug.Log($"SetupCityCmd: ValidateProgress: element {element.dataKey} in building {building.BuildingName} was changed since player progress was made. resetting element progress.");
        }
    }
}
/*var appVersion = Application.version;
 element.brickDataList.ForEach((e) => e.ResetEmittingStates());
 element.columns.ForEach((s) => s.ResetEmittingStates());
 if (element.ElementCompleted() == false && element.appVersion != Application.version)*/
