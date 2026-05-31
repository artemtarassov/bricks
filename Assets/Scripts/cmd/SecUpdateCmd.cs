using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SecUpdateCmd
{
    private CityElement currentElement;
    public SecUpdateCmd()
    {
        this.currentElement = CityModel.Instance.GetCurrentElement();
        Assert.IsNotNull(this.currentElement, "current element in cityModel not set");
    }
    public void Run()
    {
        PlayerModel.Instance.Save();
        if (ViewModel.Instance.HasAnyView())
        {
            return;
        }
        UpdateOutOfSpace();
        UpdateAdditionalEmitter();
        UpdateNextCityElement();
    }


    private void UpdateNextCityElement()
    {
        var da = currentElement.dataContainer;
        Assert.IsNotNull(da, "Current city element data container is null");

        //Debug.Log($"SecUpdateCmd: UpdateNextCityElement: element={currentElement.name}, emittingBricks={da.ElementCountEmittingBricks()}, coloredBricks={da.ElementCountColoredBricks()}, allSlotsEmpty={da.AllSlotsEmpty()}");
        if (da.ElementCompleted() && da.AllSlotsEmpty())
        {
            DOVirtual.DelayedCall(1, new UnlockNextCmd().Run);
        }
    }


    private void UpdateOutOfSpace()
    {
        var cntEmitterSpace = SlotModel.Instance.CountEmptyEmitters();
        if (cntEmitterSpace > 0)
        {
            return;
        }

        var hasEmittingBricks = currentElement.dataContainer.ElementCountEmittingBricks() > 0;
        if (hasEmittingBricks)
        {
            return;
        }
        /*var colorsInEmitters = SlotModel.Instance.Emitters.FindAll(e => e.HasColoredBricks).Select(e => e.brickData.color).ToHashSet();
        var colorsInCityElement = currentElement.GetBrickColors();

        foreach (var c in colorsInEmitters)
        {
            if (colorsInCityElement.Contains(c))
            {
                Debug.Log($"SecUpdateCmd color {c} is still present in emitters, skipping");
                return;
            }
        }*/

        ViewModel.Instance.OutOfSpaceSeconds++;
        if (ViewModel.Instance.OutOfSpaceSeconds == 2)
        {
            new ShowViewCmd().Run(ViewName.OutOfSpaceView);
        }
    }

    private void UpdateAdditionalEmitter()
    {
        var playerData = PlayerModel.Instance.playerData;

        if (playerData.additionalEmitterUnlockTimeoutTimestamp <= 0)
        {
            return;
        }

        var curTimestamp = TimeUtils.GetUnixTimestamp();
        var timeoutReached = playerData.additionalEmitterUnlockTimeoutTimestamp <= curTimestamp;
        if (!timeoutReached)
        {
            return;
        }

        PlayerModel.Instance.LockAdditionalEmitter();

        if (SlotModel.Instance.Emitters[SlotModel.AdditionalEmitterIndex].IsEmpty)
        {
            SlotModel.Instance.LockAdditionalEmitter();
        }

    }
}