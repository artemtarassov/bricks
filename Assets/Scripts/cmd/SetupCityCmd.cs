using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SetupCityCmd
{
    private CityModel cityModel => CityModel.Instance;
    private PlayerModel playerModel => PlayerModel.Instance;
    private PlayerData playerData => playerModel.playerData;

    public void Run(List<BuildingElement> buildings)
    {
        Assert.IsTrue(buildings.Count > 0, "SetupCityCmd: buildings list should not be empty");

        this.ResetPlayingStates();
        this.ResetChallenges();
        this.ResetEmitterBricks();

        for (var i = 0; i < buildings.Count; i++)
        {
            var building = buildings[i];
            var progress = playerData.GetBuildingProgressByName(building.BuildingName);
            if (progress == null)
            {
                progress = new BuildingProgressData(building.BuildingName, BuildingState.Unlocked);
                if (BuildingNameUtil.IsPremiumBuilding(progress.BuildingName))
                {
                    progress.SetState(BuildingState.Premium);
                }
                progress.attempts = 5;
                playerData.Progress.Add(progress);
            }
            if (progress.attempts == -1)
            {
                progress.attempts = 5;
            }
        }


        if (playerData.CurrentBuildingName == BuildingName.Undefined)
        {
            playerData.SetCurrentBuilding(BuildingName.Preset_House_05, BuildingState.Unlocked);
            cityModel.SetBuildings(buildings, BuildingName.Preset_House_05);
        }
        else
        {
            var isChallenge = BuildingNameUtil.IsChallengeBuilding(playerData.CurrentBuildingName);
            if (isChallenge)
            {
                playerData.SetCurrentBuilding(BuildingName.Preset_House_05, BuildingState.Unlocked);
            }
            cityModel.SetBuildings(buildings, playerData.CurrentBuildingName);
        }

        if (playerData.appVersion != Application.version)
        {
            playerData.appVersion = Application.version;
            ResetCurrentElement();
        }

        new SwitchBuildingCmd().Run(0);

#if UNITY_EDITOR    
        new ValidateDataCmd().Run();
        new TestExtrasCmd().Run();
#endif
    }


    private void ResetPlayingStates()
    {
        foreach (var p in playerData.Progress)
        {
            if (p.State == BuildingState.Playing || p.State == BuildingState.Locked)
            {
                p.SetState(BuildingState.Unlocked); //reset in-progress building to unlocked, so player can replay it
            }
        }
#if UNITY_EDITOR
        foreach (var progress in playerData.Progress.ToList())
        {
            if (progress.State == BuildingState.Premium)
            {
                progress.SetState(BuildingState.Unlocked);
            }
        }
#endif
    }

    private void ResetChallenges()
    {
        foreach (var p in playerData.Progress)
        {
            var e = p.GetCurrentElement();
            if (e == null)
            {
                continue;
            }
            if (BuildingNameUtil.IsChallengeBuilding(p.BuildingName))
            {
                p.RemoveCurrentElement();
            }
            else
            {
                e.timeoutSeconds = -1;
            }
        }
    }

    private void ResetEmitterBricks()
    {
        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        if (progress != null && progress.GetCurrentElement() != null)
        {
            var element = progress.GetCurrentElement();
            element.brickDataList.ForEach((e) => e.ResetEmittingStates());
            element.columns.ForEach((s) => s.ResetEmittingStates());
        }
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
        var cityElement = building.GetElementByDataKey(element.dataKey);
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
