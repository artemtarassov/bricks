using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;



public class CurrentBuildingOperationCmd
{

    public enum NextOperation
    {
        RestartElement,
        RestartBuilding,
        NextElement,
        ContinueCurrentElement,
    }

    private PlayerModel playerModel => PlayerModel.Instance;
    private SlotModel slotModel => SlotModel.Instance;
    private CityModel cityModel => CityModel.Instance;
    private BalancingModel balancingModel => BalancingModel.Instance;
    private ViewModel viewModel => ViewModel.Instance;

    private CityElementDataContainer currentElementData;
    private BuildingName currentBuildingName;

    private readonly int firstCityElementIndex = 0;

    private BuildingProgressData progress => playerModel.playerData.GetCurrentBuildingProgress();

    private CityModel cm => CityModel.Instance;

    private NextOperation nextOperation;
    private bool smooth;


    public CurrentBuildingOperationCmd(NextOperation operation, bool smooth = false)
    {
        this.nextOperation = operation;
        this.smooth = smooth;
        currentElementData = progress.GetCurrentElement();
        currentBuildingName = progress.BuildingName;
    }

    public void Run()
    {
        ViewModel.Instance.ResetOutOfSpaceCounter();

        if (nextOperation == NextOperation.ContinueCurrentElement)
        {
            //continue current element, do nothing
            Assert.IsNotNull(progress.GetCurrentElement(), "CurrentBuildingOperationCmd: ContinueCurrentElement: current element data should not be null");
        }

        if (nextOperation == NextOperation.RestartBuilding)
        {
            var firstElementName = cityModel.GetElementByIndex(firstCityElementIndex).dataKey;
            progress.currentElementIndex = 0;
            progress.ResetElementsCounter();
            CreateCityElementData(firstElementName);
        }

        if (nextOperation == NextOperation.RestartElement)
        {
            var firstElementName = cityModel.GetElementByIndex(progress.currentElementIndex).dataKey;
            CreateCityElementData(firstElementName);
        }

        if (nextOperation == NextOperation.NextElement)
        {
            var currentBuilding = cm.GetBuildingByName(this.currentBuildingName);
            var elementsInBuilding = currentBuilding.GetElements().ToList();
            var elementIndexInBuilding = elementsInBuilding.FindIndex((e) => e.dataKey == currentElementData.dataKey);
            var nextElementIndexInBuilding = elementIndexInBuilding + 1;

            if (nextElementIndexInBuilding >= elementsInBuilding.Count)
            {
                //no more elements in building.
                //new CompleteCurrentBuildingCmd().Run();
                return;
            }

            {
                progress.IncCompletedElementsCounter();
                var completedElementsCounter = progress.CompletedElementsCounter;
                var buildingIndex = cm.GetBuildingNameIndex(currentBuildingName);
                var dict = new Dictionary<string, object>();
                dict["completedElementsCounter"] = completedElementsCounter;
                dict["elementName"] = currentElementData.dataKey;
                dict["themeIndex"] = buildingIndex;
                new LogEventCmd().Run("complete_element", dict);
            }

            progress.currentElementIndex = nextElementIndexInBuilding;
            var nextElement = elementsInBuilding[progress.currentElementIndex];
            CreateCityElementData(nextElement.dataKey);
        }

        UnlockElement(currentElementData);
        playerModel.OnPlayerDataChanged?.Invoke();
        new SoundCmd(SoundModel.Instance.MUSIC1).Run();
        new UpdateSkyMaterialCmd().Run();
    }

    private void CreateCityElementData(string dataKey)
    {
        var currentBuildingData = balancingModel.GetDataCopy(this.currentBuildingName);
        Assert.IsNotNull(currentBuildingData, $"UnlockNextCmd CreateCityElementData: failed to find balancing data for building {this.currentBuildingName}");
        var d = currentBuildingData.cityElementDataList.Find((e) => e.dataKey == dataKey);
        Assert.IsNotNull(d, $"UnlockNextCmd CreateCityElementData: failed to find city element data with dataKey {dataKey} in building {this.currentBuildingName}");
        this.currentElementData = d;
        var rm = RemoteConfigModel.Instance.RemoteConfig;
        d.finishElementType = (FinishElementType)rm.FinishElementType;
        if (BuildingNameUtil.IsChallengeBuilding(this.currentBuildingName))
        {
            d.timeoutSeconds = rm.AddSecondsInChallenge * 2;
        }
        progress.SetCurrentElement(currentElementData);
        new AddExtrasCmd(currentBuildingName).Run(currentElementData);
    }

    private void UnlockElement(CityElementDataContainer currentElementData)
    {
        Debug.Log("UnlockNextCmd UnlockElement with city element data " + currentElementData.dataKey);
        Assert.IsNotNull(currentElementData, "UnlockCityElementCmd UnlockElement: cityElementData should not be null");
        Assert.IsTrue(currentElementData.columns.Count > 0, "UnlockCityElementCmd UnlockElement: city element should have at least 1 column to be unlocked. " + currentElementData.dataKey);

        var cityElement = cityModel.GetElementByDataKey(currentElementData.dataKey);
        Assert.IsNotNull(cityElement, $"UnlockNextCmd UnlockElement: failed to find city element with dataKey {currentElementData.dataKey}");
        var elementIndex = cityModel.GetElementIndex(cityElement);
        cityModel.ActivateElements(elementIndex);

        cityElement.Setup(currentElementData);
        slotModel.Fill(currentElementData.columns);

        this.viewModel.ChangeBottomNav(BottomNav.Slots);

        var isChallenge = BuildingNameUtil.IsChallengeBuilding(this.currentBuildingName);
        if (isChallenge)
        {
            this.viewModel.ChangeTopNav(TopNav.Clock);
        }
        else
        {
            this.viewModel.ChangeTopNav(TopNav.Coins);
        }

        if (currentElementData.ElementCountColoredBricks() == 0)
        {
            CityModel.Instance.EnableDifferentColors(cityElement, BalancingModel.AdditionalBricksOnEmptyElement);
        }

        this.MoveCam(cityElement);
    }





    private void MoveCam(CityElement cityElement)
    {
        if (this.smooth)
        {
            CamModel.Instance.MoveCameraToCityElement(cityElement);
            new SoundCmd(SoundModel.Instance.CAM_MOVE_BACK).Run();
        }
        else
        {
            InstaMoveCameraToCityElement(cityElement);
            CamModel.Instance.MoveCameraToCityElement(cityElement);
        }


#if UNITY_EDITOR
        //select gameobject
        var go = cityElement.gameObject;
        UnityEditor.Selection.activeGameObject = go;
#endif
    }

    private void InstaMoveCameraToCityElement(CityElement cityElement)
    {
        var camPos = cityElement.camPos;
        var camRot = cityElement.camRot;
        var cam = Camera.main;
        cam.transform.position = camPos;
        cam.transform.rotation = Quaternion.Euler(camRot);
    }

}