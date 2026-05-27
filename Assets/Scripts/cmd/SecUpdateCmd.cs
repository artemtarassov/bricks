using System.Linq;
using UnityEngine;

public class SecUpdateCmd
{
    public void Run()
    {
        UpdateOutOfSpace();
        UpdateAdditionalEmitter();
    }


    private void UpdateOutOfSpace()
    {
        if (ViewModel.Instance.OutOfSpaceFlag)
        {
            return;
        }
        var currentElement = CityModel.Instance.GetCurrentElement();

        if (ViewModel.Instance.HasAnyView())
        {
            return;
        }

        var cntEmitterSpace = SlotModel.Instance.CountEmptyEmitters();
        if (cntEmitterSpace > 0)
        {
            return;
        }

        var colorsInEmitters = SlotModel.Instance.Emitters.FindAll(e => e.HasColoredBricks).Select(e => e.brickData.color).ToHashSet();
        var colorsInCityElement = CityModel.Instance.GetCurrentElement().GetBrickColors();

        foreach (var c in colorsInEmitters)
        {
            if (colorsInCityElement.Contains(c))
            {
                Debug.Log($"SecUpdateCmd color {c} is still present in emitters, skipping");
                return;
            }
        }

        ViewModel.Instance.OutOfSpaceFlag = true;
        new ShowViewCmd().Run(ViewName.OutOfSpaceView);
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